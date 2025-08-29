namespace BEAPI.Dtos.Statistic
{
    public class OrderStatisticResponseDto
    {
        public int TotalOrders { get; set; }
        public int TotalOrdersLastMonth { get; set; }
        public double PercentageCompareLastMonth { get; set; }
    }
}
