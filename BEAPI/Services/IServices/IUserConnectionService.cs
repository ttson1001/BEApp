using BEAPI.Dtos.Common;
using BEAPI.Entities;

namespace BEAPI.Services
{
    public interface IUserConnectionService
    {
        Task ConnectAsync(Guid userId, string channelName, string type, string token, Guid? productId, Guid? consultant = null);
        Task DisconnectAsync(Guid consultantId);
        Task<UserConnectionDto?> GetByConsultantAsync(Guid consultantId);
        Task Decline(Guid consultantId);
    }
}
