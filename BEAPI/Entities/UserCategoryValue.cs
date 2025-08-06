namespace BEAPI.Entities
{
    public class UserCategoryValue : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid ValueId { get; set; }
        public Value Value { get; set; }
    }
}
