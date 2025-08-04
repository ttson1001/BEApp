using System.ComponentModel.DataAnnotations;

namespace BEAPI.Entities
{
    public class District : BaseEntity
    {
        public int DistrictID { get; set; }
        public int ProvinceID { get; set; }
        [Required, MaxLength(255)]
        public string DistrictName { get; set; } = null!;
        [MaxLength(50)]
        public string? Code { get; set; }
        public int? Type { get; set; }
        public int? SupportType { get; set; }
    }
}
