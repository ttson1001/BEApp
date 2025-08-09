namespace BEAPI.Dtos.Promotion
{
    public class PromotionCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DiscountPercent { get; set; }
        public Guid? ApplicableProductId { get; set; }
        public int RequiredPoints { get; set; }
        public DateTimeOffset? StartAt { get; set; }
        public DateTimeOffset? EndAt { get; set; }

    }
}
