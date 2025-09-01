namespace BEAPI.Dtos.Common
{
    public class SendNotificationToUserDto
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
