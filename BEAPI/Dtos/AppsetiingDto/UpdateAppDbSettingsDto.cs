﻿namespace BEAPI.Dtos.AppsetiingDto
{
    public class UpdateAppDbSettingsDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }   
}
