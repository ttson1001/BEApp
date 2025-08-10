using BEAPI.Dtos.Addreess;
using BEAPI.Dtos.Category;
using BEAPI.Dtos.Promotion;
using BEAPI.Entities.Enum;
using System.ComponentModel.DataAnnotations;

namespace BEAPI.Dtos.User
{
    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        [MaxLength(255)]
        public string? Avatar { get; set; }
        public Gender? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EmergencyPhoneNumber { get; set; }
        public DateTimeOffset? BirthDate { get; set; }
        public int Age { get; set; }
        public int RewardPoint { get; set; }
        public string? Description { get; set; }
        public string? RelationShip { get; set; }
        public Guid? GuardianId { get; set; }
        public Guid? RoleId { get; set; }
        public string? RoleName { get; set; }

        public List<AddressDto> Addresses { get; set; } = new();
        public List<UserPromotionItemDto> UserPromotions { get; set; } = new();
        public List<CategoryValueDto> CategoryValues { get; set; } = new();

        public int CartCount { get; set; }
        public int PaymentCount { get; set; }
    }
}
