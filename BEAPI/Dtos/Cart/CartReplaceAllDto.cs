using System.ComponentModel.DataAnnotations;

namespace BEAPI.Dtos.Cart
{
    public class CartReplaceAllDto
    {
        public string CustomerId { get; set; } = string.Empty;
        [MinLength(1, ErrorMessage = "Cart must have at least one item")]
        public List<CartItemReplaceAllDto> Items { get; set; } = new();
    }
}
