using System.ComponentModel.DataAnnotations;

namespace BEAPI.Dtos.Order
{
    public class CancelOrderDto
    {
        [Required]
        public string OrderId { get; set; } = string.Empty;
        
        [Required]
        public string CancelReason { get; set; } = string.Empty;

    }
}
