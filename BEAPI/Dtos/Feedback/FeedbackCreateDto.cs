namespace BEAPI.Dtos.Feedback
{
    public class FeedbackCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public Guid UserId { get; set; }
    }
}
