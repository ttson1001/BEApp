namespace BEAPI.Dtos.Payment
{
    public class DateRangeDto
    {
        public Guid UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
