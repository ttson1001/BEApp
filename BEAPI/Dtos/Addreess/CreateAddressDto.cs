namespace BEAPI.Dtos.Addreess
{
    public class CreateAddressDto
    {
        public string StreetAddress { get; set; } = string.Empty;
        public long WardCode { get; set; }
        public string WardName { get; set; } = string.Empty;
        public int DistrictID { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
