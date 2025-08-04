using BEAPI.Entities;
using BEAPI.Repositories;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class InternalLocationService : IinternalLocationService
    {
        private readonly IRepository<Province> _provinceRepo;
        private readonly IRepository<District> _districtRepo;
        private readonly IRepository<Ward> _wardRepo;

        public InternalLocationService(
            IRepository<Province> provinceRepo,
            IRepository<District> districtRepo,
            IRepository<Ward> wardRepo)
        {
            _provinceRepo = provinceRepo;
            _districtRepo = districtRepo;
            _wardRepo = wardRepo;
        }

        public async Task<List<Province>> GetProvincesAsync()
        {
            return await _provinceRepo.Get()
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.ProvinceName)
                .ToListAsync();
        }

        public async Task<List<District>> GetDistrictsByProvinceAsync(int provinceId)
        {
            var provinceExists = await _provinceRepo.Get().AnyAsync(p => p.ProvinceID == provinceId && !p.IsDeleted);
            if (!provinceExists)
                throw new Exception($"ProvinceId {provinceId} not found");

            return await _districtRepo.Get()
                .Where(d => d.ProvinceID == provinceId && !d.IsDeleted)
                .OrderBy(d => d.DistrictName)
                .ToListAsync();
        }

        public async Task<List<Ward>> GetWardsByDistrictAsync(int districtId)
        {
            var districtExists = await _districtRepo.Get().AnyAsync(d => d.DistrictID == districtId && !d.IsDeleted);
            if (!districtExists)
                throw new Exception($"DistrictId {districtId} not found");

            return await _wardRepo.Get()
                .Where(w => w.DistrictID == districtId && !w.IsDeleted)
                .OrderBy(w => w.WardName)
                .ToListAsync();
        }
    }
}
