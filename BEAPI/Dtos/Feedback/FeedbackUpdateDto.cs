namespace BEAPI.Dtos.Feedback
{
    public class FeedbackUpdateDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
    }
}
