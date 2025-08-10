using AutoMapper;
using BEAPI.Database;
using BEAPI.Dtos.Addreess;
using BEAPI.Dtos.Category;
using BEAPI.Dtos.Elder;
using BEAPI.Entities;
using BEAPI.Helper;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class ElderService : IElderService
    {
        private readonly IRepository<User> _repository;
        private readonly IRepository<Address> _addressRepo;
        private readonly IRepository<District> _districtRepo;
        private readonly IRepository<Ward> _warRepo;
        private readonly IRepository<Province> _provineRepo;
        private readonly IRepository<UserCategoryValue> _userCateValueRepo;
        private readonly IMapper _mapper;

        public ElderService(IMapper mapper, IRepository<UserCategoryValue> userCateValueRepo, IRepository<User> repository, IRepository<Address> addressRepo, IRepository<District> districtRepo, IRepository<Ward> warRepo, IRepository<Province> provineRepo)
        {
            _mapper = mapper;
            _repository = repository;
            _addressRepo = addressRepo;
            _districtRepo = districtRepo;
            _warRepo = warRepo;
            _provineRepo = provineRepo;
            _userCateValueRepo = userCateValueRepo;
        }

        public async Task<List<ElderDto>> GetElderByCusId(string cusId)
        {
            var guidCus = GuidHelper.ParseOrThrow(cusId, nameof(cusId));
            var list = await _repository.Get().Include(x => x.Addresses).Include(x => x.UserCategories)
        .ThenInclude(uc => uc.Value).Where(x => x.GuardianId == guidCus).ToListAsync();
            return _mapper.Map<List<ElderDto>>(list);
        }

        public async Task UpdateElderAdressAsync(List<UpdateAddressDto> addressesDto, string userId) {
            var user = await _repository.Get().Where(x => x.Id == GuidHelper.ParseOrThrow(userId, nameof(userId)))
                .FirstOrDefaultAsync()
                ?? throw new Exception("User not found");
            var listAddress = await _addressRepo.Get().Where(x => x.UserId == user.Id).ToListAsync();
            if (listAddress.Count > 0)
            {
                _addressRepo.DeleteRange(listAddress);
            }

            if (addressesDto.Count != 0)
            {
                var addresses = _mapper.Map<List<Address>>(addressesDto);

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
                await _addressRepo.AddRangeAsync(addresses);
            }

            await _addressRepo.SaveChangesAsync();
        }

        public async Task UpdateElderCategory( List<UpdateCategoryElderDto> updateCategoryElderDtos , Guid elderId)
        {
            var user = await _repository.Get()
                .FirstOrDefaultAsync(u => u.Id == elderId)
                ?? throw new Exception("User not found");
            var listCate = await _userCateValueRepo.Get().Where(x => x.UserId == elderId).ToListAsync();
            
            if (listCate.Count > 0)
            {
                _userCateValueRepo.DeleteRange(listCate);
            }

            if (updateCategoryElderDtos.Count > 0)
            {
                var categoryValues = updateCategoryElderDtos
                    .Select(x => new UserCategoryValue
                    {
                        Id = Guid.NewGuid(),
                        ValueId = x.CategoryId,
                        User = user
                    }).ToList();
                await _userCateValueRepo.AddRangeAsync(categoryValues);
            }

            await _userCateValueRepo.SaveChangesAsync();
        }

        public async Task UpdateElderAsync(ElderUpdateDto dto)
        {
            var userId = GuidHelper.ParseOrThrow(dto.Id, nameof(dto.Id));

            var user = await _repository.Get()
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new Exception("User not found");

            if (dto.BirthDate > DateTimeOffset.UtcNow)
                throw new Exception("BirthDate cannot be in the future");

            var age = DateTimeOffset.UtcNow.Year - dto.BirthDate.Year;
            if (dto.BirthDate.Date > DateTimeOffset.UtcNow.AddYears(-age).Date)
                age--;

            if (age is < 45 or > 120)
                throw new Exception(age < 45
                    ? "Invalid BirthDate (must be at least 45 years old)"
                    : "Age cannot exceed 120 years");

            user.FullName = dto.FullName?.Trim() ?? user.FullName;
            user.UserName = dto.UserName?.Trim() ?? user.UserName;
            user.Description = dto.Description;
            user.BirthDate = dto.BirthDate;
            user.Avatar = dto.Avatar;
            user.EmergencyPhoneNumber = dto.EmergencyPhoneNumber;
            user.RelationShip = dto.RelationShip;
            user.Gender = dto.Gender;
            user.Spendlimit = dto.Spendlimit;
            user.Age = age;

            await _repository.SaveChangesAsync();
        }

        public async Task ChangeIsDeletedAsync(string id)
        {
            var userId = GuidHelper.ParseOrThrow(id, "UserId");

            var user = await _repository.Get().FirstOrDefaultAsync(u => u.Id == userId) ?? throw new Exception("User not found");
            if (user.IsDeleted) { user.DeletionDate = DateTimeOffset.UtcNow; }
            else { user.DeletionDate = null; }
            _repository.Update(user);
            await _repository.SaveChangesAsync();
        }
    }
}
