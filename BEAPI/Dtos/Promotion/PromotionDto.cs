namespace BEAPI.Dtos.Promotion
{
    public class PromotionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DiscountPercent { get; set; }
        public Guid? ApplicableProductId { get; set; }
        public int RequiredPoints { get; set; }
        public DateTimeOffset? StartAt { get; set; }
        public DateTimeOffset? EndAt { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreationDate { get; set; }
    }
}
