namespace BEAPI.Dtos.Statistic
{
    public class StatisticResponseDto
    {
        public RevenueStatisticResponseDto RevenueStatisticResponse { get; set; }
        public OrderStatisticResponseDto OrderStatisticResponse { get; set; }
        public UserStatisticResponseDto UserStatisticResponse { get; set; }
    }
}
