using BEAPI.Entities.Enum;

namespace BEAPI.Entities
{
    public class Feedback : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string? ImagePath { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid AdminId { get; set; }
        public User Admin { get; set; }

        public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;
    }
}
