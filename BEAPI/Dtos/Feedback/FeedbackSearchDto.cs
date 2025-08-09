using BEAPI.Entities.Enum;

namespace BEAPI.Dtos.Feedback
{
    public class FeedbackSearchDto
    {
        public string? Keyword { get; set; }
        public FeedbackStatus? Status { get; set; }
        public Guid? UserId { get; set; }

        public string SortBy { get; set; } = "CreationDate";
        public bool SortAscending { get; set; } = false;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
