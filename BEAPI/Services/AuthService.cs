using AutoMapper;
using BEAPI.Dtos.Auth;
using BEAPI.Entities;
using BEAPI.Exceptions;
using BEAPI.Helper;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class AuthService: IAuthService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private readonly IRepository<Wallet> _walletRepo;
        private readonly IOtpService _otpService;

        public AuthService(IRepository<User> userRepo, IOtpService otpService, IMapper mapper, IJwtService jwtService, IRepository<Wallet> walletRepo)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _jwtService = jwtService;
            _otpService = otpService;
            _walletRepo = walletRepo;
        }

        public async Task RegisterAsync(RegisterDto registerDto)
        {
            if (!IsValidPassword(registerDto.Password))
                throw new Exception("Password must be at least 8 characters long and contain uppercase, lowercase, number, and special character.");

            var existingUser = await _userRepo.Get()
                .Where(u =>
                    u.Email == registerDto.Email
                    || u.PhoneNumber == registerDto.PhoneNumber)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                if (existingUser.Email == registerDto.Email)
                    throw new Exception(ExceptionConstant.EmailAlreadyExists);

                if (existingUser.PhoneNumber == registerDto.PhoneNumber)
                    throw new Exception(ExceptionConstant.PhoneNumberAlreadyExists);
            }

            var user = _mapper.Map<User>(registerDto);
            user.UserName = registerDto.Email;

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            var wallet = new Wallet { UserId = user.Id, Amount = 0 };
            await _walletRepo.AddAsync(wallet);
            await _walletRepo.SaveChangesAsync();
        }

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }


        public async Task<User> FindUserByEmailOrPhoneAsync(string emailOrPhone)
        {
            var user = await _userRepo.Get().FirstOrDefaultAsync(x =>
                x.Email == emailOrPhone || x.PhoneNumber == emailOrPhone);

            return user == null ? throw new KeyNotFoundException("User not found") : user;
        }

        public async Task<(string Token, User User)> LoginAsync(LoginDto dto)
        {
            var user = await _userRepo.Get().Include(x => x.Role).FirstOrDefaultAsync(u => u.UserName == dto.UserName || u.Email == dto.UserName) ?? throw new KeyNotFoundException("User not found");
            if (!user.IsVerified)
            {
                throw new Exception("Acount is not verify");
            }

            user.DeviceId = dto.DeviceId;
            user.PresenceStatus = Entities.Enum.PresenceStatus.Online;

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception(ExceptionConstant.InvalidCredentials);

            var token = _jwtService.GenerateToken(user, null);
            await _userRepo.SaveChangesAsync();
            return (token, user);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto request)
        {
            var user = await _userRepo.Get()
                .FirstOrDefaultAsync(u => u.OtpCode == request.Otp)
                ?? throw new Exception("OTP invalid.");

            await _otpService.VerifyOtpAsync(user, request.Otp);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            await _userRepo.SaveChangesAsync();
        }

        public async Task VerifyUserAsync(string otp)
        {
            var user = await _userRepo.Get()
                .FirstOrDefaultAsync(u => u.OtpCode == otp)
                ?? throw new Exception("OTP invalid.");

            await _otpService.VerifyUserAsync(user, otp);

            await _userRepo.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var parsedUserId = GuidHelper.ParseOrThrow(userId, nameof(userId));

            var user = await _userRepo.Get().FirstOrDefaultAsync(x => x.Id == parsedUserId);
            if (user == null)
                throw new Exception("Người dùng không tồn tại");

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                throw new Exception("Mật khẩu cũ không đúng");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _userRepo.SaveChangesAsync();
        }

    }
}
