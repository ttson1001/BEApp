using BEAPI.Entities.Enum;
using System.ComponentModel.DataAnnotations;

namespace BEAPI.Dtos.Auth
{
    public class ElderRegisterDto
    {
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Description { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public Gender Gender { get; set; }
    }
}
