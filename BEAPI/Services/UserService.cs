using AutoMapper;
using BEAPI.Dtos.Auth;
using BEAPI.Entities;
using BEAPI.Exceptions;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace BEAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        public UserService(IRepository<User> userRepo, IMapper mapper, IJwtService jwtService)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _jwtService = jwtService;
        }

        public async Task CreateElder(ElderRegisterDto elderRegisterDto, Guid userId)
        {
            if (elderRegisterDto.BirthDate > DateTimeOffset.UtcNow)
                throw new Exception("BirthDate cannot be in the future");

            var age = DateTimeOffset.UtcNow.Year - elderRegisterDto.BirthDate.Year;
            if (elderRegisterDto.BirthDate.Date > DateTimeOffset.UtcNow.AddYears(-age).Date)
                age--;

            if (age is < 45 or > 120)
                throw new Exception(age < 45
                    ? "Invalid BirthDate (must be at least 45 years old)"
                    : "Age cannot exceed 120 years");

            var user = _mapper.Map<User>(elderRegisterDto);
            user.Age = age;
            user.GuardianId = userId;

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();
        }

        public async Task<(string Token, string QrBase64)> GenerateElderLoginQrAsync(Guid elderId)
        {
            var elder = await _userRepo.Get().FirstOrDefaultAsync(u => u.Id == elderId && u.Role.Name == "Elder") ?? throw new Exception(ExceptionConstant.ElderNotFound);
            var token = _jwtService.GenerateToken(elder, expiresInMinutes: 2);

            var qrUrl = $"https://localhost:7264/api/auth/qr-login?token={token}";

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(qrUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(20);
            var qrBase64 = Convert.ToBase64String(qrBytes);

            return (token, $"data:image/png;base64,{qrBase64}");
        }

        public async Task<string> LoginByQrAsync(string token)
        {
            var principal = _jwtService.ValidateToken(token) ?? throw new Exception("Invalid or expired token");
            var elderId = principal.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(elderId))
                throw new Exception("Invalid token data");

            var elder = await _userRepo.Get().Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == Guid.Parse(elderId));
            return elder == null ? throw new Exception(ExceptionConstant.ElderNotFound) : _jwtService.GenerateToken(elder, null);
        }
    }
}
