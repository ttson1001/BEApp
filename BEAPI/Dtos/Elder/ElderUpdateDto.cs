namespace BEAPI.Dtos.Elder
{
    public class ElderUpdateDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public DateTime BirthDate { get; set; }
        public decimal SpendLimit { get; set; }
        public string EmergencyPhoneNumber { get; set; }
        public string RelationShip { get; set; }
        public string? Avatar { get; set; }
        public int Gender { get; set; }

    }
}
