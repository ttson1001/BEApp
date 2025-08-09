namespace BEAPI.Dtos.Promotion
{
    public class PromotionUpdateDto : PromotionCreateDto
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
