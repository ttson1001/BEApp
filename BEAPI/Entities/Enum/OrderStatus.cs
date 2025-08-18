namespace BEAPI.Entities.Enum
{
    public enum OrderStatus
    {
        Created,         // Đã tạo
        Paid,            // Đã thanh toán
        PendingChecked,  // Chờ kiểm tra
        PendingConfirm,  // Chờ xác nhận
        PendingPickup,   // Chờ lấy hàng
        PendingDelivery, // Chờ giao hàng
        Shipping,        // Đang giao
        Delivered,       // Đã giao
        Completed,       // Hoàn tất
        Canceled,        // Đã hủy
        Fail             // Thất bại
    }
}
