﻿namespace BEAPI.Dtos.User
{
    public class UserListDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? RoleName { get; set; }
        public bool IsVerified { get; set; }
    }
}
