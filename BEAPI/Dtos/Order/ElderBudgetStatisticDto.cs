namespace BEAPI.Dtos.Order
{
    public class ElderBudgetStatisticDto
    {
        public Guid? ElderId { get; set; }
        public string? ElderName { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal? LimitSpent { get; set; }
        public int OrderCount { get; set; }
    }
}
