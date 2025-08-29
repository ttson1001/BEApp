namespace BEAPI.Dtos.Statistic
{
    public class RevenueStatisticResponseDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalRevenueLastMonth { get; set; }
        public double PercentageCompareLastMonth { get; set; }
    }
}
