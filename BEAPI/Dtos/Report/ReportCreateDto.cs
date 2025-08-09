namespace BEAPI.Dtos.Report
{
    public class ReportCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public Guid ConsultantId { get; set; }
    }
}
