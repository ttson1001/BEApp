namespace BEAPI.Dtos.Feedback
{
    public class FeedbackRespondDto
    {
        public Guid FeedbackId { get; set; }
        public string ResponseMessage { get; set; } = string.Empty;
        public string? ResponseAttachment { get; set; }
    }
}
