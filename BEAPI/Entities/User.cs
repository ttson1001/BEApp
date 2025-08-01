using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BEAPI.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(UserName), IsUnique = true)]
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class User: BaseEntity
    {
        [MaxLength(255)]
        public string FullName { get; set; } = string.Empty;
        public Guid? OTPId { get; set; }
        public string? RefreshToken { get; set; }
        [MaxLength(256)]
        public string UserName { get; set; } = string.Empty;
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;
        [MaxLength(20)]
        public string? Avatar { get; set; } 
        public string? Gender { get; set; }
        public int Age { get; set; }
        public DateTimeOffset? BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public Guid? Guardian { get; set; }
        public Guid? RoleId { get; set; }
        public Role Role { get; set; }
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    }
}
