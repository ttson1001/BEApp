namespace BEAPI.Entities
{
    public class UserPromotion : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid PromotionId { get; set; }
        public Promotion Promotion { get; set; }

        public bool IsUsed { get; set; } = false;
        public DateTimeOffset? UsedAt { get; set; }
    }
}
