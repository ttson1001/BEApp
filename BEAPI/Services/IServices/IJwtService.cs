using BEAPI.Entities;
using System.Security.Claims;

namespace BEAPI.Services.IServices
{
    public interface IJwtService
    {
        string GenerateToken(User user, int? expiresInMinutes);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
