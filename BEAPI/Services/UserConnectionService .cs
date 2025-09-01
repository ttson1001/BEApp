using BEAPI.Dtos.Common;
using BEAPI.Entities;
using BEAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class UserConnectionService : IUserConnectionService
    {
        private readonly IRepository<UserConnection> _repo;
        private readonly IRepository<User> _userRepository;

        public UserConnectionService(IRepository<UserConnection> repo, IRepository<User> userRepository)
        {
            _repo = repo;
            _userRepository = userRepository;
        }

        public async Task ConnectAsync(Guid userId, string channelName, string type, string token,Guid? prodcutId, Guid? consultant = null)
        {
            if (consultant == null)
            {
                var consultantRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

                var randomConsultant = await _userRepository.Get()
                    .Where(c => c.RoleId == consultantRoleId)
                    .OrderBy(x => Guid.NewGuid())
                    .FirstOrDefaultAsync();

                if (randomConsultant != null)
                {
                    consultant = randomConsultant.Id;
                }
            }

            var entity = new UserConnection
            {
                UserId = userId,
                ChannelName = channelName,
                Type = type,
                Token = token,
                Consultant = consultant,
                ProductId = prodcutId,
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
        }

        public async Task DisconnectAsync(Guid consultantId)
        {
            var entity = await _repo.Get().Where(x => x.Consultant == consultantId).FirstOrDefaultAsync();
             _repo.Delete(entity);
            await _repo.SaveChangesAsync();
        }

        public async Task Decline(Guid consultantId)
        {
            var entity = await _repo.Get()
                .FirstOrDefaultAsync(x => x.Consultant == consultantId);

            if (entity == null)
                throw new Exception("Consultant not found");

            if (string.IsNullOrEmpty(entity.SkippedConsultants))
            {
                entity.SkippedConsultants = consultantId.ToString();
            }
            else
            {
                entity.SkippedConsultants += ";" + consultantId;
            }

            entity.Consultant = null;

            var skippedList = entity.SkippedConsultants?
                .Split(";", StringSplitOptions.RemoveEmptyEntries)
                ?? Array.Empty<string>();

            var consultantRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

            var newConsultant = await _userRepository.Get()
                .Where(c => c.RoleId == consultantRoleId)
                .Where(c => !skippedList.Contains(c.Id.ToString()))
                .OrderBy(x => Guid.NewGuid())
                .FirstOrDefaultAsync();

            if (newConsultant != null)
            {
                entity.Consultant = newConsultant.Id;
                _repo.Update(entity);
            }
            else
            {
                _repo.Delete(entity);
            }

            await _repo.SaveChangesAsync();
        }

        public async Task<UserConnectionDto?> GetByConsultantAsync(Guid consultantId)
        {
            return await _repo.Get().Include(x => x.User)
                .Where(x => x.Consultant == consultantId)
                .Select(x => new UserConnectionDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    FullName = x.User.FullName,
                    ChannelName = x.ChannelName,
                    Type = x.Type,
                    Token = x.Token,
                    Consultant = x.Consultant,
                    ProductId = x.ProductId
                })
                .FirstOrDefaultAsync();
        }

    }
}
