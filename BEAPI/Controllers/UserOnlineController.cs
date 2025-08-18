using BEAPI.Dtos.Common;
using BEAPI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BEAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserOnlineController : ControllerBase
    {
        [HttpGet("[action]/{userId}")]
        public IActionResult CheckUserOnline(string userId)
        {
            try
            {
                var isOnline = UserOnlineHub.IsUserOnline(userId);
                var userInfo = UserOnlineHub.GetUserInfo(userId);

                return Ok(new ResponseDto
                {
                    Message = "Check user online status successfully",
                    Data = new
                    {
                        UserId = userId,
                        IsOnline = isOnline,
                        UserName = userInfo?.UserName,
                        ConnectedAt = userInfo?.ConnectedAt,
                        LastActivity = userInfo?.LastActivity
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public IActionResult CheckCurrentUserOnline()
        {
            try
            {
                var currentUserId = User?.FindFirst("UserId")?.Value;
                var currentUserName = User?.FindFirst("UserName")?.Value;

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new ResponseDto { Message = "User not authenticated" });
                }

                var isOnline = UserOnlineHub.IsUserOnline(currentUserId);
                var userInfo = UserOnlineHub.GetUserInfo(currentUserId);

                return Ok(new ResponseDto
                {
                    Message = "Check current user online status successfully",
                    Data = new
                    {
                        UserId = currentUserId,
                        UserName = currentUserName,
                        IsOnline = isOnline,
                        ConnectedAt = userInfo?.ConnectedAt,
                        LastActivity = userInfo?.LastActivity,
                        IsAuthenticated = true
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public IActionResult GetOnlineUsers()
        {
            try
            {
                var onlineUsers = UserOnlineHub.GetAllOnlineUsers()
                    .Select(u => new
                    {
                        u.UserId,
                        u.UserName,
                        u.ConnectedAt,
                        u.LastActivity,
                        u.Status
                    })
                    .ToList();

                return Ok(new ResponseDto
                {
                    Message = "Get online users successfully",
                    Data = new
                    {
                        TotalOnline = onlineUsers.Count,
                        Users = onlineUsers
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [HttpGet("[action]")]
        public IActionResult GetOnlineCount()
        {
            try
            {
                var count = UserOnlineHub.GetOnlineUserCount();

                return Ok(new ResponseDto
                {
                    Message = "Get online user count successfully",
                    Data = new { OnlineCount = count }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        private string GetStatusDescription(UserOnlineHub.UserStatus status)
        {
            return status switch
            {
                UserOnlineHub.UserStatus.Online => "Đang online và hoạt động",
                UserOnlineHub.UserStatus.Busy => "Đang bận (có thể đang chat, làm việc)",
                UserOnlineHub.UserStatus.Away => "Vắng mặt (không hoạt động trong 5 phút)",
                UserOnlineHub.UserStatus.Offline => "Đã offline",
                _ => "Không xác định"
            };
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> SetUserStatus([FromQuery] UserOnlineHub.UserStatus statusDto)
        {
            try
            {
                var currentUserId = User?.FindFirst("UserId")?.Value;
                var currentUserName = User?.FindFirst("UserName")?.Value;

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new ResponseDto { Message = "User not authenticated" });
                }

                if (!UserOnlineHub.IsUserOnline(currentUserId))
                {
                    return BadRequest(new ResponseDto { Message = "User is not online" });
                }

                var userInfo = UserOnlineHub.GetUserInfo(currentUserId);
                if (userInfo == null)
                {
                    return BadRequest(new ResponseDto { Message = "User info not found" });
                }

                switch (statusDto)
                {
                    case UserOnlineHub.UserStatus.Online:
                        userInfo.Status = UserOnlineHub.UserStatus.Online;
                        userInfo.LastActivity = DateTime.UtcNow;
                        UserOnlineHub.ResetUserTimers(currentUserId);
                        break;

                    case UserOnlineHub.UserStatus.Busy:
                        userInfo.Status = UserOnlineHub.UserStatus.Busy;
                        userInfo.LastActivity = DateTime.UtcNow;
                        UserOnlineHub.StopAwayTimer(currentUserId);
                        break;

                    case UserOnlineHub.UserStatus.Away:
                        userInfo.Status = UserOnlineHub.UserStatus.Away;
                        userInfo.LastActivity = DateTime.UtcNow;
                        UserOnlineHub.StartOfflineTimer(currentUserId);
                        break;

                    case UserOnlineHub.UserStatus.Offline:
                        return BadRequest(new ResponseDto { Message = "Cannot set status to Offline manually. Use logout instead." });

                    default:
                        return BadRequest(new ResponseDto { Message = "Invalid status" });
                }

                return Ok(new ResponseDto
                {
                    Message = $"User status changed to {statusDto} successfully",
                    Data = new
                    {
                        UserId = currentUserId,
                        UserName = currentUserName,
                        Status = userInfo.Status.ToString(),
                        StatusDescription = GetStatusDescription(userInfo.Status),
                        LastActivity = userInfo.LastActivity
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> ReportUserActivity()
        {
            try
            {
                var currentUserId = User?.FindFirst("UserId")?.Value;
                var currentUserName = User?.FindFirst("UserName")?.Value;

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new ResponseDto { Message = "User not authenticated" });
                }

                if (!UserOnlineHub.IsUserOnline(currentUserId))
                {
                    return BadRequest(new ResponseDto { Message = "User is not online" });
                }

                var userInfo = UserOnlineHub.GetUserInfo(currentUserId);
                if (userInfo == null)
                {
                    return BadRequest(new ResponseDto { Message = "User info not found" });
                }

                userInfo.LastActivity = DateTime.UtcNow;

                if (userInfo.Status == UserOnlineHub.UserStatus.Away)
                {
                    userInfo.Status = UserOnlineHub.UserStatus.Online;
                }

                UserOnlineHub.ResetUserTimers(currentUserId);

                return Ok(new ResponseDto
                {
                    Message = "User activity reported successfully",
                    Data = new
                    {
                        UserId = currentUserId,
                        UserName = currentUserName,
                        Status = userInfo.Status.ToString(),
                        LastActivity = userInfo.LastActivity
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("[action]/{targetUserId}")]
        public async Task<IActionResult> SetOtherUserStatus(string targetUserId, [FromQuery] UserOnlineHub.UserStatus dto)
        {
            try
            {
                var currentUserId = User?.FindFirst("UserId")?.Value;
                var currentUserRole = User?.FindFirst("Role")?.Value;

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new ResponseDto { Message = "User not authenticated" });
                }

                if (currentUserRole != "Admin" && currentUserId != targetUserId)
                {
                    return Forbid();
                }

                if (!UserOnlineHub.IsUserOnline(targetUserId))
                {
                    return BadRequest(new ResponseDto { Message = "Target user is not online" });
                }

                var userInfo = UserOnlineHub.GetUserInfo(targetUserId);
                if (userInfo == null)
                {
                    return BadRequest(new ResponseDto { Message = "Target user info not found" });
                }

                switch (dto)
                {
                    case UserOnlineHub.UserStatus.Online:
                        userInfo.Status = UserOnlineHub.UserStatus.Online;
                        userInfo.LastActivity = DateTime.UtcNow;
                        UserOnlineHub.ResetUserTimers(targetUserId);
                        break;

                    case UserOnlineHub.UserStatus.Busy:
                        userInfo.Status = UserOnlineHub.UserStatus.Busy;
                        userInfo.LastActivity = DateTime.UtcNow;
                        UserOnlineHub.StopAwayTimer(targetUserId);
                        break;

                    case UserOnlineHub.UserStatus.Away:
                        userInfo.Status = UserOnlineHub.UserStatus.Away;
                        userInfo.LastActivity = DateTime.UtcNow;
                        UserOnlineHub.StartOfflineTimer(targetUserId);
                        break;

                    case UserOnlineHub.UserStatus.Offline:
                        return BadRequest(new ResponseDto { Message = "Cannot set status to Offline manually. Use logout instead." });

                    default:
                        return BadRequest(new ResponseDto { Message = "Invalid status" });
                }

                return Ok(new ResponseDto
                {
                    Message = $"User {userInfo.UserName} status changed to {dto} successfully",
                    Data = new
                    {
                        UserId = targetUserId,
                        UserName = userInfo.UserName,
                        Status = userInfo.Status.ToString(),
                        StatusDescription = GetStatusDescription(userInfo.Status),
                        LastActivity = userInfo.LastActivity,
                        SetBy = currentUserId
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDto { Message = ex.Message });
            }
        }
    }
}
