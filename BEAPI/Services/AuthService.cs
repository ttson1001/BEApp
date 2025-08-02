using AutoMapper;
using BEAPI.Dtos.Auth;
using BEAPI.Entities;
using BEAPI.Exceptions;
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

        public AuthService(IRepository<User> userRepo, IMapper mapper, IJwtService jwtService)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _jwtService = jwtService;
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

        public async Task<string> LoginAsync(LoginDto dto)
        {
            var user = await _userRepo.Get().Include(x => x.Role).FirstOrDefaultAsync(u => u.UserName == dto.UserName);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception(ExceptionConstant.InvalidCredentials);

            return _jwtService.GenerateToken(user, null);
        }
    }
}
