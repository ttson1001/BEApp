using BEAPI.Entities.Enum;

namespace BEAPI.Dtos.Feedback
{
    public class FeedbackUpdateStatusDto
    {
        public Guid FeedbackId { get; set; }
        public FeedbackStatus Status { get; set; }
    }
}
