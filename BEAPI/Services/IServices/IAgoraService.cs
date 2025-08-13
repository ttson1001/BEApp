namespace BEAPI.Services.IServices
{
    public interface IAgoraService
    {
        Task<(string AppId, string Channel, string CallerUid, string? CallerToken, string CalleeUid, string? CalleeToken)> CreateChannelAsync(Guid callerId, Guid calleeId);
    }
}


