using System.ComponentModel.DataAnnotations;

namespace BEAPI.Entities
{
    public class Province : BaseEntity
    {
        public int ProvinceID { get; set; }
        [Required, MaxLength(255)]
        public string ProvinceName { get; set; } = null!;
        [MaxLength(50)]
        public string? Code { get; set; }
    }
}
