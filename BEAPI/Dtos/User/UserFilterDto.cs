namespace BEAPI.Dtos.User
{
    public class UserFilterDto
    {
        public string? RoleId { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
