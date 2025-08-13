using AutoMapper;
using AutoMapper.QueryableExtensions;
using BEAPI.Dtos.Addreess;
using BEAPI.Dtos.Category;
using BEAPI.Dtos.Common;
using BEAPI.Dtos.Elder;
using BEAPI.Dtos.Promotion;
using BEAPI.Dtos.User;
using BEAPI.Entities;
using BEAPI.Exceptions;
using BEAPI.Helper;
using BEAPI.Model;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QRCoder;

namespace BEAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private readonly IRepository<District> _districtRepo;
        private readonly IRepository<Ward> _warRepo;
        private readonly IRepository<Province> _provineRepo;
        private readonly IRepository<Wallet> _walletRepo;
        private readonly string _baseUrl;

        public UserService(IOptions<AppSettings> options,
            IRepository<District> districtRepo,
            IRepository<Ward> warRepo,
            IRepository<Province> provineRepo,
            IRepository<Wallet> walletRepo,
            IRepository<User> userRepo,
            IMapper mapper,
            IJwtService jwtService)
        {
            _userRepo = userRepo;
            _mapper = mapper;
            _jwtService = jwtService;
            _baseUrl = options.Value.BaseUrl;
            _districtRepo = districtRepo;
            _warRepo = warRepo;
            _provineRepo = provineRepo;
            _walletRepo = walletRepo;
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

            if (elderRegisterDto.Addresses.Count != 0)
            {
                var addresses = _mapper.Map<List<Address>>(elderRegisterDto.Addresses);

                var provinceIds = await _provineRepo.Get().Select(p => p.ProvinceID).ToListAsync();
                var districtIds = await _districtRepo.Get().Select(d => d.DistrictID).ToListAsync();
                var wardCodes = await _warRepo.Get().Select(w => w.WardCode).ToListAsync();

                foreach (var address in addresses)
                {
                    if (!provinceIds.Contains(address.ProvinceID))
                        throw new Exception($"ProvinceID {address.ProvinceID} does not exist");

                    if (!districtIds.Contains(address.DistrictID))
                        throw new Exception($"DistrictID {address.DistrictID} does not exist");

                    if (!wardCodes.Contains(address.WardCode))
                        throw new Exception($"WardCode {address.WardCode} does not exist");

                    address.User = user;
                }
            }

            if (elderRegisterDto.CategoryValueIds.Count > 0)
            {
                user.UserCategories = elderRegisterDto.CategoryValueIds
                    .Select(catId => new UserCategoryValue
                    {
                        Id = Guid.NewGuid(),
                        ValueId = GuidHelper.ParseOrThrow(catId, nameof(catId)),
                        User = user
                    }).ToList();
            }

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            // Create wallet immediately for the new user
            var wallet = new Wallet { UserId = user.Id, Amount = 0 };
            await _walletRepo.AddAsync(wallet);
            await _walletRepo.SaveChangesAsync();
        }

        public async Task CreateUserAsync(UserCreateDto dto)
        {
            if (await _userRepo.Get().AnyAsync(u => u.Email == dto.Email || u.UserName == dto.UserName || u.PhoneNumber == dto.PhoneNumber))
                throw new Exception("Email, username or Phonenumber have been already.");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.UserName,
                PhoneNumber = dto.PhoneNumber,
                RoleId = GuidHelper.ParseOrThrow(dto.RoleId, nameof(dto.RoleId)),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsVerified = true
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(UserUpdateDto dto)
        {
            var userId = GuidHelper.ParseOrThrow(dto.Id, nameof(dto.Id));
            var user = await _userRepo.Get().Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("User not found");

            if (user.Role?.Name == "Elder")
                throw new Exception("Cannot update Elder with this endpoint");

            if (!string.IsNullOrWhiteSpace(dto.FullName)) user.FullName = dto.FullName;
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;
            if (!string.IsNullOrWhiteSpace(dto.Avatar)) user.Avatar = dto.Avatar;
            if (!string.IsNullOrWhiteSpace(dto.Description)) user.Description = dto.Description;

            await _userRepo.SaveChangesAsync();
        }

        public async Task BanOrUnbanUserAsync(string userId)
        {
            var guidUser = GuidHelper.ParseOrThrow(userId, nameof(userId));

            var user = await _userRepo.Get()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == guidUser)
                ?? throw new Exception("User not found");

            if (user.Role?.Name == "Admin")
                throw new Exception("You cannot ban an Admin user.");

            if (user.IsDeleted)
                throw new Exception("User is already banned.");

            user.IsDeleted = !user.IsDeleted;
            await _userRepo.SaveChangesAsync();
        }

        public async Task<PagedResult<UserListDto>> FilterUsersAsync(UserFilterDto request)
        {
            var query = _userRepo.Get().Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(u =>
                    u.FullName.Contains(request.SearchTerm) ||
                    u.UserName.Contains(request.SearchTerm) ||
                    u.Email.Contains(request.SearchTerm) || 
                    u.PhoneNumber.Contains(request.SearchTerm));
            }

            if (!string.IsNullOrWhiteSpace(request.RoleId))
            {
                var roleIdGuid = GuidHelper.ParseOrThrow(request.RoleId, nameof(request.RoleId));
                query = query.Where(u => u.RoleId == roleIdGuid);
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(u => u.CreationDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ProjectTo<UserListDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PagedResult<UserListDto>
            {
                Items = items,
                TotalItems = totalItems,
                Page = request.Page,
                PageSize = request.PageSize
            };

        }

        public async Task<(string Token, string QrBase64)> GenerateElderLoginQrAsync(Guid elderId)
        {
            var elder = await _userRepo.Get().FirstOrDefaultAsync(u => u.Id == elderId && u.Role.Name == "Elder") ?? throw new Exception(ExceptionConstant.ElderNotFound);
            var tokens = _jwtService.GenerateToken(elder, expiresInMinutes: 2);

            var qrUrl = $"{_baseUrl}/api/auth/qr-login?token={tokens}";

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(qrUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(20);
            var qrBase64 = Convert.ToBase64String(qrBytes);

            var token = await LoginByQrAsync(tokens);

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

        public async Task<UserDetailDto> GetDetailAsync(Guid id)
        {
            var user = await _userRepo.Get()
                .Include(u => u.Role)
                .Include(u => u.Addresses)
                .Include(u => u.Carts)
                .Include(u => u.PaymentHistory)
                .Include(u => u.UserCategories).ThenInclude(uc => uc.Value)
                .Include(u => u.UserPromotions).ThenInclude(up => up.Promotion)
                .FirstOrDefaultAsync(u => u.Id == id)
                ?? throw new Exception("User not found");

            var dto = _mapper.Map<UserDetailDto>(user);

            dto.Addresses = _mapper.Map<List<AddressDto>>(user.Addresses);

            dto.CategoryValues = user.UserCategories
                    .Select(uc => new CategoryValueDto
                    {
                        Id = uc.Value.Id.ToString(),
                        Code = uc.Value.Code,
                        Description = uc.Value.Description,
                        Label = uc.Value.Label,
                        Type = uc.Value.Type,
                        ChildrenId = uc.Value.ChildListOfValueId?.ToString(),
                        ChildrentLabel = uc.Value.ChildListOfValue?.Label
                    })
                    .ToList();

            dto.UserPromotions = user.UserPromotions
                .OrderByDescending(up => up.CreationDate)
                .Select(up => _mapper.Map<UserPromotionItemDto>(up))
                .ToList();

            return dto;
        }
    }
}
