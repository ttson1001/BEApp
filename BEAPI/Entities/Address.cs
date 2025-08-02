using System.ComponentModel.DataAnnotations;

namespace BEAPI.Entities
{
    public class Address : BaseEntity
    {
        [MaxLength(255)]
        public string StreetAddress { get; set; } = string.Empty;
        public long WardCode { get; set; }
        public string WardName { get; set; } = string.Empty;
        public int DistrictID { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User? User { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
