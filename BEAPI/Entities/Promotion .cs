namespace BEAPI.Entities
{
    public class Promotion : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DiscountPercent { get; set; }
        public Guid? ApplicableProductId { get; set; }
        public int RequiredPoints { get; set; } = 0;
        public DateTimeOffset? StartAt { get; set; }
        public DateTimeOffset? EndAt { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual List<UserPromotion> UserPromotions { get; set; } = new();
    }
}
