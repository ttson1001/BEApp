using BEAPI.Dtos.Common;
using BEAPI.Entities;
using BEAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services
{
    public class UserConnectionService : IUserConnectionService
    {
        private readonly IRepository<UserConnection> _repo;

        public UserConnectionService(IRepository<UserConnection> repo)
        {
            _repo = repo;
        }

        public async Task ConnectAsync(Guid userId, string channelName, string type, string token, Guid? consultant = null)
        {
            var entity = new UserConnection
            {
                UserId = userId,
                ChannelName = channelName,
                Type = type,
                Token = token,
                Consultant = consultant
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
                    Consultant = x.Consultant
                })
                .FirstOrDefaultAsync();
        }

    }
}
