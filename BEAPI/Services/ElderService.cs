using AutoMapper;
using BEAPI.Database;
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
        private readonly IMapper _mapper;

        public ElderService(IMapper mapper, IRepository<User> repository, IRepository<Address> addressRepo, IRepository<District> districtRepo, IRepository<Ward> warRepo, IRepository<Province> provineRepo)
        {
            _mapper = mapper;
            _repository = repository;
            _addressRepo = addressRepo;
            _districtRepo = districtRepo;
            _warRepo = warRepo;
            _provineRepo = provineRepo;
        }

        public async Task<List<ElderDto>> GetElderByCusId(string cusId)
        {
            var guidCus = GuidHelper.ParseOrThrow(cusId, nameof(cusId));
            var list = await _repository.Get().Include(x => x.Addresses).Include(x => x.UserCategories)
        .ThenInclude(uc => uc.Value).Where(x => x.GuardianId == guidCus).ToListAsync();
            return _mapper.Map<List<ElderDto>>(list);
        }

        public async Task UpdateElderAsync(ElderUpdateDto dto)
        {
            var userId = GuidHelper.ParseOrThrow(dto.Id, nameof(dto.Id));

            var user = await _repository.Get()
                .Include(u => u.Addresses)
                .Include(u => u.UserCategories)
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

            _mapper.Map(dto, user);

            user.UserCategories.Clear();

            foreach (var catId in dto.CategoryValueIds)
            {
                user.UserCategories.Add(new UserCategoryValue
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    ValueId = GuidHelper.ParseOrThrow(catId, nameof(catId)),
                });
            }

            if (dto.Addresses != null)
            {
                var dtoAddressList = _mapper.Map<List<Address>>(dto.Addresses);

                var provinceIds = await _provineRepo.Get().Select(p => p.ProvinceID).ToListAsync();
                var districtIds = await _districtRepo.Get().Select(d => d.DistrictID).ToListAsync();
                var wardCodes = await _warRepo.Get().Select(w => w.WardCode).ToListAsync();

                var dtoIds = dtoAddressList.Select(a => a.Id).ToHashSet();
                var existingAddresses = user.Addresses.ToList();

                var toDelete = existingAddresses.Where(e => !dtoIds.Contains(e.Id)).ToList();
                if (toDelete.Any())
                    _addressRepo.DeleteRange(toDelete);

                foreach (var existing in existingAddresses.Where(e => dtoIds.Contains(e.Id)))
                {
                    var dtoAddr = dtoAddressList.First(a => a.Id == existing.Id);

                    if (!provinceIds.Contains(dtoAddr.ProvinceID))
                        throw new Exception($"ProvinceID {dtoAddr.ProvinceID} does not exist");

                    if (!districtIds.Contains(dtoAddr.DistrictID))
                        throw new Exception($"DistrictID {dtoAddr.DistrictID} does not exist");

                    if (!wardCodes.Contains(dtoAddr.WardCode))
                        throw new Exception($"WardCode {dtoAddr.WardCode} does not exist");

                    existing.StreetAddress = dtoAddr.StreetAddress;
                    existing.WardCode = dtoAddr.WardCode;
                    existing.WardName = dtoAddr.WardName;
                    existing.DistrictID = dtoAddr.DistrictID;
                    existing.DistrictName = dtoAddr.DistrictName;
                    existing.ProvinceID = dtoAddr.ProvinceID;
                    existing.ProvinceName = dtoAddr.ProvinceName;
                    existing.PhoneNumber = dtoAddr.PhoneNumber;
                }

                var existingIds = existingAddresses.Select(e => e.Id).ToHashSet();
                var toAdd = dtoAddressList.Where(a => !existingIds.Contains(a.Id) || a.Id == Guid.Empty).ToList();

                foreach (var address in toAdd)
                {
                    if (!provinceIds.Contains(address.ProvinceID))
                        throw new Exception($"ProvinceID {address.ProvinceID} does not exist");

                    if (!districtIds.Contains(address.DistrictID))
                        throw new Exception($"DistrictID {address.DistrictID} does not exist");

                    if (!wardCodes.Contains(address.WardCode))
                        throw new Exception($"WardCode {address.WardCode} does not exist");

                    address.Id = Guid.NewGuid();
                    address.UserId = user.Id;

                   await _addressRepo.AddAsync(address);
                }
            }

            _repository.Update(user);
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
