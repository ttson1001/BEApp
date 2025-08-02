using BEAPI.Dtos.Auth;

namespace BEAPI.Services.IServices
{
    public interface IUserService
    {
        Task CreateElder(ElderRegisterDto elderRegisterDto, Guid userId);
        Task<string> LoginByQrAsync(string token);
        Task<(string Token, string QrBase64)> GenerateElderLoginQrAsync(Guid elderId);
    }
}
