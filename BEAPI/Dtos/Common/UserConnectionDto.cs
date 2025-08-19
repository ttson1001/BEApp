namespace BEAPI.Dtos.Common
{
    public class UserConnectionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ChannelName { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Token { get; set; } = null!;
        public Guid? Consultant { get; set; }
    }
}
