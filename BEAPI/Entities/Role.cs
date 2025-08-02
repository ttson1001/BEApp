﻿namespace BEAPI.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public List<User> Users { get; set; } = new List<User>();
    }
}
