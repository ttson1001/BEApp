using System.ComponentModel.DataAnnotations;

namespace BEAPI.Entities
{
    public class Ward : BaseEntity
    {
        [MaxLength(50)]
        public string WardCode { get; set; } = null!;
        public int DistrictID { get; set; }
        [Required, MaxLength(255)]
        public string WardName { get; set; } = null!;
    }
}
