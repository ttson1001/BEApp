using System.ComponentModel.DataAnnotations;

namespace BEAPI.Entities
{
    public class Address : BaseEntity
    {
        [MaxLength(255)]
        public string StreetAddress { get; set; } = string.Empty;
        [MaxLength(255)]
        public string WardCode { get; set; } = string.Empty;
        [MaxLength(255)]
        public string WardName { get; set; } = string.Empty;
        [MaxLength(255)]
        public string DistrictName { get; set; } = string.Empty;
        [MaxLength(255)]
        public string ProvinceName { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User? User { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
    }

}
