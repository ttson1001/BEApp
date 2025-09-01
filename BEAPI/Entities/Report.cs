namespace BEAPI.Entities
{
    public class Report : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; }

        public Guid ConsultantId { get; set; }

        public User Consultant { get; set; }

        public Guid? ProductId { get; set; }
    }
}
