using System.ComponentModel.DataAnnotations;

namespace BEAPI.Dtos.Cart
{
    public class CartReplaceAllDto
    {
        public string CustomerId { get; set; } = string.Empty;
        [MinLength(1, ErrorMessage = "Không thể thêm giỏi hàng rỗng")]
        public List<CartItemReplaceAllDto> Items { get; set; } = new();
    }
}
