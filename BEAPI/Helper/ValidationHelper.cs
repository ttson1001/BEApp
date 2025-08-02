using BEAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Helper
{
    public static class ValidationHelper
    {
        public static async Task<List<Guid>> ValidateIdsExistAsync<T>(
            IRepository<T> repo,
            List<Guid> ids
        ) where T : class
        {
            var existingIds = await repo.Get()
                .Where(e => ids.Contains(EF.Property<Guid>(e, "Id")))
                .Select(e => EF.Property<Guid>(e, "Id"))
                .ToListAsync();

            return ids.Except(existingIds).ToList();
        }
    }
}
