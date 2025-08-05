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
        private readonly IOtpService _otpService;

        public AuthService(IRepository<User> userRepo, IOtpService otpService, IMapper mapper, IJwtService jwtService)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _jwtService = jwtService;
            _otpService = otpService;
        }

        public async Task RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userRepo.Get().Where(u => u.UserName == registerDto.UserName || u.Email == registerDto.Email || u.PhoneNumber == registerDto.PhoneNumber).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                throw new Exception(ExceptionConstant.UserAlreadyExists);
            }
            var user = _mapper.Map<User>(registerDto);
            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();
        }

        public async Task<User> FindUserByEmailOrPhoneAsync(string emailOrPhone)
        {
            var user = await _userRepo.Get().FirstOrDefaultAsync(x =>
                x.Email == emailOrPhone || x.PhoneNumber == emailOrPhone);

            return user == null ? throw new KeyNotFoundException("User not found") : user;
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            var user = await _userRepo.Get().Include(x => x.Role).FirstOrDefaultAsync(u => u.UserName == dto.UserName) ?? throw new KeyNotFoundException("User not found");
            if (!user.IsVerified)
            {
                throw new Exception("Acount is not verify");
            }

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception(ExceptionConstant.InvalidCredentials);

            return _jwtService.GenerateToken(user, null);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto request)
        {
            var user = await _userRepo.Get()
                .FirstOrDefaultAsync(u => u.OtpCode == request.Otp)
                ?? throw new Exception("OTP không hợp lệ.");

            await _otpService.VerifyOtpAsync(user, request.Otp);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            await _userRepo.SaveChangesAsync();
        }

        public async Task VerifyUserAsync(string otp)
        {
            var user = await _userRepo.Get()
                .FirstOrDefaultAsync(u => u.OtpCode == otp)
                ?? throw new Exception("OTP không hợp lệ.");

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
