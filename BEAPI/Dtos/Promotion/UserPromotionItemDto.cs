namespace BEAPI.Dtos.Promotion
{
    public class UserPromotionItemDto
    {
        public Guid UserPromotionId { get; set; }
        public Guid PromotionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DiscountPercent { get; set; }
        public bool IsUsed { get; set; }
        public DateTimeOffset? UsedAt { get; set; }
        public DateTimeOffset RedeemedAt { get; set; }
        public DateTimeOffset? StartAt { get; set; }
        public DateTimeOffset? EndAt { get; set; }
    }
}
