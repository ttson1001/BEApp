namespace BEAPI.Dtos.Addreess
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        public string StreetAddress { get; set; }
        public string WardCode { get; set; }
        public string WardName { get; set; }
        public int DistrictID { get; set; }
        public string DistrictName { get; set; }
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
