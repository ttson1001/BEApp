using BEAPI.Entities.Enum;

namespace BEAPI.Dtos.Order
{
    public class OrderFilterDto
    {
        public OrderStatus? OrderStatus { get; set; }
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = true;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
