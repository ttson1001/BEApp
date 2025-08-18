using BEAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BEAPI.Middleware
{
    public class AutoSignalRMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHubContext<UserOnlineHub> _hubContext;

        public AutoSignalRMiddleware(RequestDelegate next, IHubContext<UserOnlineHub> hubContext)
        {
            _next = next;
            _hubContext = hubContext;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kiểm tra nếu đây là request đăng nhập thành công
            if (IsLoginSuccessRequest(context))
            {
                // Lấy thông tin user từ response
                var userId = GetUserIdFromResponse(context);
                var userName = GetUserNameFromResponse(context);

                if (!string.IsNullOrEmpty(userId))
                {
                    // Tự động thêm user vào danh sách online
                    await AddUserToOnlineList(userId, userName);
                    
                    // Gửi thông báo cho tất cả clients
                    await _hubContext.Clients.All.SendAsync("UserOnline", userId, userName);
                    
                    Console.WriteLine($"Auto-connected user {userName} ({userId}) to SignalR after login");
                }
            }

            await _next(context);
        }

        private bool IsLoginSuccessRequest(HttpContext context)
        {
            // Kiểm tra nếu đây là POST request đến endpoint đăng nhập và có response thành công
            return context.Request.Method == "POST" &&
                   context.Request.Path.StartsWithSegments("/api/auth/login") &&
                   context.Response.StatusCode == 200;
        }

        private string? GetUserIdFromResponse(HttpContext context)
        {
            // Lấy UserId từ JWT token hoặc response body
            // Có thể cần điều chỉnh tùy theo cấu trúc response của bạn
            return context.User?.FindFirst("UserId")?.Value;
        }

        private string? GetUserNameFromResponse(HttpContext context)
        {
            // Lấy UserName từ JWT token hoặc response body
            return context.User?.FindFirst("UserName")?.Value;
        }

        private async Task AddUserToOnlineList(string userId, string userName)
        {
            // Thêm user vào danh sách online trong UserOnlineHub
            var userInfo = new UserOnlineHub.UserConnectionInfo
            {
                UserId = userId,
                UserName = userName,
                ConnectedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            };

            // Sử dụng static method để thêm user
            // Lưu ý: Cần thêm method này vào UserOnlineHub
            UserOnlineHub.AddUserToOnlineList(userInfo);
        }
    }

    public static class AutoSignalRMiddlewareExtensions
    {
        public static IApplicationBuilder UseAutoSignalR(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AutoSignalRMiddleware>();
        }
    }
}
