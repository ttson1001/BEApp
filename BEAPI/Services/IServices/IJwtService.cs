namespace BEAPI.Services.IServices
{
    public interface IJwtService
    {
        string GenerateToken(string userId);
    }
}
