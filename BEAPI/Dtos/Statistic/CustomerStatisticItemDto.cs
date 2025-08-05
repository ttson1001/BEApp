namespace BEAPI.Dtos.Statistic
{
    public class CustomerStatisticItemDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalSpending { get; set; }
    }
}
