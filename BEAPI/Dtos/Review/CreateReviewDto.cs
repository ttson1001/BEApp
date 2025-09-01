namespace BEAPI.Dtos.Review
{
    public class CreateReviewDto
    {
        public Guid OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public Guid CustomerId { get; set; }
    }

}
