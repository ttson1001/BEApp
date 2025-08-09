namespace BEAPI.Dtos.Report
{
    public class ReportSearchDto
    {
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string SortBy { get; set; } = "CreationDate";
        public bool SortAscending { get; set; } = false;
    }

}
