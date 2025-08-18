using System.ComponentModel.DataAnnotations;
using BEAPI.Hubs;

namespace BEAPI.Dtos.UserStatus
{
    public class SetUserStatusDto
    {
        [Required]
        public UserOnlineHub.UserStatus Status { get; set; }
    }
}
