using BEAPI.Model;
using BEAPI.Services.IServices;
using Microsoft.Extensions.Options;
using BEAPI.Helper.Agora;

namespace BEAPI.Services
{
    public class AgoraService : IAgoraService
    {
        private readonly AgoraSettings _settings;

        public AgoraService(IOptions<AgoraSettings> options)
        {
            _settings = options.Value;
        }

        public Task<(string AppId, string Channel, string CallerUid, string? CallerToken, string CalleeUid, string? CalleeToken)> CreateChannelAsync(Guid callerId, Guid calleeId)
        {
            // Simple deterministic channel id for both users
            var channel = $"call_{callerId:N}_{calleeId:N}";
            var callerUid = callerId.ToString("N");
            var calleeUid = calleeId.ToString("N");
            string? callerToken = null;
            string? calleeToken = null;

            if (!string.IsNullOrWhiteSpace(_settings.AppCertificate))
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var expire = (uint)(_settings.TokenExpireSeconds > 0 ? now + _settings.TokenExpireSeconds : now + 3600);
                callerToken = RtcTokenBuilder.BuildTokenWithUserAccount(_settings.AppId, _settings.AppCertificate, channel, callerUid, RtcTokenBuilder.Role.Publisher, (uint)expire);
                calleeToken = RtcTokenBuilder.BuildTokenWithUserAccount(_settings.AppId, _settings.AppCertificate, channel, calleeUid, RtcTokenBuilder.Role.Publisher, (uint)expire);
            }

            return Task.FromResult((_settings.AppId, channel, callerUid, callerToken, calleeUid, calleeToken));
        }
    }
}


