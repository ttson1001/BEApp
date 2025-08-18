using BEAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BEAPI.Middleware
{
    public class UserActivityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHubContext<UserOnlineHub> _hubContext;

        public UserActivityMiddleware(RequestDelegate next, IHubContext<UserOnlineHub> hubContext)
        {
            _next = next;
            _hubContext = hubContext;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.User?.FindFirst("UserId")?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Báo cáo hoạt động cho user đang online
                await ReportUserActivity(userId);
            }
            
            await _next(context);
        }

        private async Task ReportUserActivity(string userId)
        {
            try
            {
                if (UserOnlineHub.IsUserOnline(userId))
                {
                    var userInfo = UserOnlineHub.GetUserInfo(userId);
                    if (userInfo != null)
                    {
                        // Cập nhật last activity
                        userInfo.LastActivity = DateTime.UtcNow;
                        
                        // Reset về Online nếu đang Away
                        if (userInfo.Status == UserOnlineHub.UserStatus.Away)
                        {
                            userInfo.Status = UserOnlineHub.UserStatus.Online;
                            await _hubContext.Clients.All.SendAsync("UserStatusChanged", userId, userInfo.UserName, UserOnlineHub.UserStatus.Online);
                        }
                        
                        // Reset timers
                        UserOnlineHub.ResetUserTimers(userId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reporting user activity: {ex.Message}");
            }
        }
    }

    public static class UserActivityMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserActivityTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserActivityMiddleware>();
        }
    }
}
