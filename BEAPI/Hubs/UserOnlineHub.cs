using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BEAPI.Hubs
{
    public class UserOnlineHub : Hub
    {
        private static readonly ConcurrentDictionary<string, UserConnectionInfo> _onlineUsers = new();
        private static readonly ConcurrentDictionary<string, System.Timers.Timer> _awayTimers = new();
        private static readonly ConcurrentDictionary<string, System.Timers.Timer> _offlineTimers = new();
        
        private const int AWAY_TIMEOUT_MINUTES = 5;
        private const int OFFLINE_TIMEOUT_MINUTES = 30;

        public class UserConnectionInfo
        {
            public string UserId { get; set; } = string.Empty;
            public string ConnectionId { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
            public DateTime LastActivity { get; set; } = DateTime.UtcNow;
            public UserStatus Status { get; set; } = UserStatus.Online;
        }

        public enum UserStatus
        {
            Online,     // Đang online và hoạt động
            Busy,       // Đang bận (có thể đang chat, làm việc)
            Away,       // Vắng mặt (không hoạt động trong 5 phút)
            Offline     // Đã offline
        }

        public override async Task OnConnectedAsync()
        {
            // Lấy user ID từ JWT token
            var userId = Context.User?.FindFirst("UserId")?.Value;
            var userName = Context.User?.FindFirst("UserName")?.Value ?? "Unknown";

            if (!string.IsNullOrEmpty(userId))
            {
                var userInfo = new UserConnectionInfo
                {
                    UserId = userId,
                    ConnectionId = Context.ConnectionId,
                    UserName = userName,
                    ConnectedAt = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow
                };

                _onlineUsers.TryAdd(userId, userInfo);

                // Thông báo cho tất cả clients biết user đã online
                await Clients.All.SendAsync("UserOnline", userId, userName);
                
                // Gửi danh sách user online cho user mới kết nối
                var onlineUsers = _onlineUsers.Values
                    .Select(u => new { u.UserId, u.UserName, u.ConnectedAt })
                    .ToList();
                
                await Clients.Caller.SendAsync("OnlineUsersList", onlineUsers);
                
                Console.WriteLine($"User {userName} ({userId}) connected. Total online: {_onlineUsers.Count}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserIdFromConnection(Context.ConnectionId);
            
            if (!string.IsNullOrEmpty(userId) && _onlineUsers.TryRemove(userId, out var userInfo))
            {
                // Cleanup timers
                CleanupTimers(userId);
                
                // Thông báo cho tất cả clients biết user đã offline
                await Clients.All.SendAsync("UserOffline", userId, userInfo.UserName);
                
                Console.WriteLine($"User {userInfo.UserName} ({userId}) disconnected. Total online: {_onlineUsers.Count}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Client gọi để báo cáo hoạt động
        public async Task UserActivity()
        {
            var userId = GetUserIdFromConnection(Context.ConnectionId);
            if (!string.IsNullOrEmpty(userId) && _onlineUsers.TryGetValue(userId, out var userInfo))
            {
                userInfo.LastActivity = DateTime.UtcNow;
                
                // Reset về Online nếu đang Away
                if (userInfo.Status == UserStatus.Away)
                {
                    userInfo.Status = UserStatus.Online;
                    await Clients.All.SendAsync("UserStatusChanged", userId, userInfo.UserName, UserStatus.Online);
                }
                
                // Reset timers
                ResetAwayTimer(userId);
                ResetOfflineTimer(userId);
                
                await Clients.All.SendAsync("UserActivity", userId, userInfo.UserName);
            }
        }

        // Client gọi để set trạng thái Busy
        public async Task SetBusyStatus()
        {
            var userId = GetUserIdFromConnection(Context.ConnectionId);
            if (!string.IsNullOrEmpty(userId) && _onlineUsers.TryGetValue(userId, out var userInfo))
            {
                userInfo.Status = UserStatus.Busy;
                userInfo.LastActivity = DateTime.UtcNow;
                
                // Stop away timer khi busy
                StopAwayTimer(userId);
                
                await Clients.All.SendAsync("UserStatusChanged", userId, userInfo.UserName, UserStatus.Busy);
            }
        }

        // Client gọi để set trạng thái Online
        public async Task SetOnlineStatus()
        {
            var userId = GetUserIdFromConnection(Context.ConnectionId);
            if (!string.IsNullOrEmpty(userId) && _onlineUsers.TryGetValue(userId, out var userInfo))
            {
                userInfo.Status = UserStatus.Online;
                userInfo.LastActivity = DateTime.UtcNow;
                
                // Start away timer
                StartAwayTimer(userId);
                
                await Clients.All.SendAsync("UserStatusChanged", userId, userInfo.UserName, UserStatus.Online);
            }
        }

        // Lấy danh sách user đang online
        public async Task GetOnlineUsers()
        {
            var onlineUsers = _onlineUsers.Values
                .Select(u => new { u.UserId, u.UserName, u.ConnectedAt, u.LastActivity })
                .ToList();
            
            await Clients.Caller.SendAsync("OnlineUsersList", onlineUsers);
        }

        // Kiểm tra user có đang online không
        public async Task CheckUserOnline(string targetUserId)
        {
            var isOnline = _onlineUsers.TryGetValue(targetUserId, out var userInfo);
            
            await Clients.Caller.SendAsync("UserOnlineStatus", targetUserId, isOnline, 
                isOnline ? userInfo?.UserName : null);
        }

        // Gửi tin nhắn cho user cụ thể
        public async Task SendToUser(string targetUserId, string message)
        {
            var senderUserId = GetUserIdFromConnection(Context.ConnectionId);
            var senderUserName = _onlineUsers.TryGetValue(senderUserId ?? "", out var senderInfo) 
                ? senderInfo.UserName : "Unknown";

            if (_onlineUsers.TryGetValue(targetUserId, out var targetInfo))
            {
                // Gửi tin nhắn cho user đích
                await Clients.Client(targetInfo.ConnectionId)
                    .SendAsync("PrivateMessage", senderUserId, senderUserName, message, DateTime.UtcNow);
                
                // Gửi xác nhận cho người gửi
                await Clients.Caller.SendAsync("MessageSent", targetUserId, message, DateTime.UtcNow);
            }
            else
            {
                // User không online
                await Clients.Caller.SendAsync("UserNotOnline", targetUserId);
            }
        }

        // Gửi tin nhắn cho tất cả
        public async Task SendToAll(string message)
        {
            var senderUserId = GetUserIdFromConnection(Context.ConnectionId);
            var senderUserName = _onlineUsers.TryGetValue(senderUserId ?? "", out var senderInfo) 
                ? senderInfo.UserName : "Unknown";

            await Clients.All.SendAsync("BroadcastMessage", senderUserId, senderUserName, message, DateTime.UtcNow);
        }

        private string? GetUserIdFromConnection(string connectionId)
        {
            return _onlineUsers.Values
                .FirstOrDefault(u => u.ConnectionId == connectionId)?.UserId;
        }

        // Static methods để kiểm tra từ bên ngoài
        public static bool IsUserOnline(string userId)
        {
            return _onlineUsers.ContainsKey(userId);
        }

        public static UserConnectionInfo? GetUserInfo(string userId)
        {
            _onlineUsers.TryGetValue(userId, out var userInfo);
            return userInfo;
        }

        public static List<UserConnectionInfo> GetAllOnlineUsers()
        {
            return _onlineUsers.Values.ToList();
        }

        public static int GetOnlineUserCount()
        {
            return _onlineUsers.Count;
        }

        // Thêm user vào danh sách online (cho middleware)
        public static void AddUserToOnlineList(UserConnectionInfo userInfo)
        {
            _onlineUsers.TryAdd(userInfo.UserId, userInfo);
            // Start away timer cho user mới
            StartAwayTimer(userInfo.UserId);
        }

        // Xóa user khỏi danh sách online (cho logout)
        public static void RemoveUserFromOnlineList(string userId)
        {
            if (_onlineUsers.TryRemove(userId, out _))
            {
                CleanupTimers(userId);
            }
        }

        // Timer methods
        public static void StartAwayTimer(string userId)
        {
            StopAwayTimer(userId);
            
            var timer = new System.Timers.Timer(AWAY_TIMEOUT_MINUTES * 60 * 1000);
            timer.Elapsed += (sender, e) => OnAwayTimeout(userId);
            timer.AutoReset = false;
            timer.Start();
            
            _awayTimers.TryAdd(userId, timer);
        }

        private static void ResetAwayTimer(string userId)
        {
            if (_awayTimers.TryGetValue(userId, out var timer))
            {
                timer.Stop();
                timer.Dispose();
                _awayTimers.TryRemove(userId, out _);
            }
            StartAwayTimer(userId);
        }

        public static void StopAwayTimer(string userId)
        {
            if (_awayTimers.TryGetValue(userId, out var timer))
            {
                timer.Stop();
                timer.Dispose();
                _awayTimers.TryRemove(userId, out _);
            }
        }

        public static void StartOfflineTimer(string userId)
        {
            StopOfflineTimer(userId);
            
            var timer = new System.Timers.Timer(OFFLINE_TIMEOUT_MINUTES * 60 * 1000);
            timer.Elapsed += (sender, e) => OnOfflineTimeout(userId);
            timer.AutoReset = false;
            timer.Start();
            
            _offlineTimers.TryAdd(userId, timer);
        }

        private static void ResetOfflineTimer(string userId)
        {
            if (_offlineTimers.TryGetValue(userId, out var timer))
            {
                timer.Stop();
                timer.Dispose();
                _offlineTimers.TryRemove(userId, out _);
            }
            StartOfflineTimer(userId);
        }

        private static void StopOfflineTimer(string userId)
        {
            if (_offlineTimers.TryGetValue(userId, out var timer))
            {
                timer.Stop();
                timer.Dispose();
                _offlineTimers.TryRemove(userId, out _);
            }
        }

        private static async void OnAwayTimeout(string userId)
        {
            if (_onlineUsers.TryGetValue(userId, out var userInfo))
            {
                userInfo.Status = UserStatus.Away;
                Console.WriteLine($"User {userInfo.UserName} ({userId}) is now Away");
                
                // Start offline timer
                StartOfflineTimer(userId);
            }
        }

        private static async void OnOfflineTimeout(string userId)
        {
            if (_onlineUsers.TryRemove(userId, out var userInfo))
            {
                userInfo.Status = UserStatus.Offline;
                Console.WriteLine($"User {userInfo.UserName} ({userId}) is now Offline due to inactivity");
                
                // Cleanup timers
                StopAwayTimer(userId);
                StopOfflineTimer(userId);
            }
        }

        // Cleanup timers khi user disconnect
        private static void CleanupTimers(string userId)
        {
            StopAwayTimer(userId);
            StopOfflineTimer(userId);
        }

        // Reset timers cho user (cho middleware)
        public static void ResetUserTimers(string userId)
        {
            ResetAwayTimer(userId);
            ResetOfflineTimer(userId);
        }
    }
}
