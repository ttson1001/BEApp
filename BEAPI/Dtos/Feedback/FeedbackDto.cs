using BEAPI.Entities.Enum;

namespace BEAPI.Dtos.Feedback
{
    public class FeedbackDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public Guid UserId { get; set; }
        public Guid? AdminId { get; set; }
        public FeedbackStatus Status { get; set; }

        public string? ResponseMessage { get; set; }
        public string? ResponseAttachment { get; set; }
        public DateTimeOffset? RespondedAt { get; set; }
    }   
}
