﻿namespace BEAPI.Dtos.Auth
{
    public class ResetPasswordDto
    {
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

}
