using BEAPI.Dtos.Addreess;
using BEAPI.Entities.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEAPI.Dtos.Elder
{
    public class ElderUpdateDto
    {
        public string Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Spendlimit { get; set; }
        public string? Avatar { get; set; }
        public string? EmergencyPhoneNumber { get; set; }
        public string? RelationShip { get; set; }
        public Gender Gender { get; set; }

    }
}
