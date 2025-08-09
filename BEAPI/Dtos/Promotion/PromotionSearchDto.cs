namespace BEAPI.Dtos.Promotion
{
    public class PromotionSearchDto
    {
        public string? Keyword { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "CreationDate";
        public bool SortAscending { get; set; } = false;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
