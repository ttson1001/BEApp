public class OrderDetailDto
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int Discount { get; set; }

    public string Style { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new();
}