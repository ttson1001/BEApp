using System.ComponentModel.DataAnnotations;

namespace BEAPI.Entities
{
    public abstract class BaseEntity : IEntity
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? CreationDate { get; set; }
        public DateTimeOffset? ModificationDate { get; set; }
        public DateTimeOffset? DeletionDate { get; set; }
        public Guid? CreatedById { get; set; }
        public Guid? ModificationById { get; set; }
        public Guid? DeleteById { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}

