using BEAPI.Entities.Enum;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace BEAPI.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(UserName), IsUnique = true)]
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class User: BaseEntity
    {
        [MaxLength(255)]
        public string FullName { get; set; } = string.Empty;
        public string? OtpCode { get; set; }
        public DateTimeOffset? OtpExpiredAt { get; set; }
        public bool IsOtpUsed { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public string? DeviceId { get; set; }
        public string? RefreshToken { get; set; }
        [MaxLength(256)]
        public string? UserName { get; set; }
        [MaxLength(256)]
        public string? Email { get; set; }
        [MaxLength(20)]
        public string? Avatar { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Spendlimit { get; set; }
        public string? EmergencyPhoneNumber { get; set; }
        public Gender? Gender { get; set; }
        public string? RelationShip { get; set; }
        public int Age { get; set; }
        [MaxLength(2000)]
        public string? Description { get; set; }
        public DateTimeOffset? BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PasswordHash { get; set; }
        public Guid? GuardianId { get; set; }
        public User? Guardian { get; set; }
        public Guid? RoleId { get; set; }
        public int RewardPoint { get; set; } = 0;
        public Role Role { get; set; }
        public virtual List<UserPromotion> UserPromotions { get; set; } = new();
        public virtual List<Address> Addresses { get; set; } = new List<Address>();
        public virtual List<Cart> Carts { get; set; } = new List<Cart>();
        public virtual List<PaymentHistory> PaymentHistory { get; set; } = new List<PaymentHistory>();
        public virtual List<UserCategoryValue> UserCategories { get; set; } = new List<UserCategoryValue>();
    }
}
