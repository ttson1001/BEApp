using System.ComponentModel.DataAnnotations;

namespace BEAPI.Entities
{
    public class UserConnection : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        public User User { get; set; }

        [Required]
        public string ChannelName { get; set; } = null!;

        [Required]
        public string Type { get; set; } = null!; // VD: "signalR"

        public Guid? Consultant { get; set; } // consultantId

        [Required]
        public string Token { get; set; } = null!; // connectionId

    }
}
