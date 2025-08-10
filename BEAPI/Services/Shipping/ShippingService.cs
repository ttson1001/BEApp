// BEAPI/Services/Shipping/ShippingService.cs
using BEAPI.Database;
using BEAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAPI.Services.Shipping
{
    public class ShippingService
    {
        private readonly BeContext _db;
        private readonly GhnClient _ghn;
        private readonly GhnOptions _opt;

        public ShippingService(BeContext db, GhnClient ghn, GhnOptions opt)
        { _db = db; _ghn = ghn; _opt = opt; }

        public async Task CreateGhnShipmentForOrderAsync(Guid orderId, CancellationToken ct = default)
        {
            var order = await _db.Set<Order>()
                .Include(o => o.OrderDetails).ThenInclude(od => od.ProductVariant).ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new Exception("Order not found");

            if (string.IsNullOrWhiteSpace(order.WardCode))
                throw new Exception("Order missing WardCode (GHN requires ward_code string)");

            int toDistrictId = order.DistrictID;
            string toWardCode = order.WardCode;
            string toAddress = order.StreetAddress;
            string toName = order.Customer?.FullName ?? "Customer";
            string toPhone = order.PhoneNumber;

            int weight = Math.Max((int)order.OrderDetails.Sum(d => d.Quantity * 250), 500); // tạm 250g/item
            int height = 10, length = 20, width = 15; // fallback cm

            var serviceId = await _ghn.GetFirstAvailableServiceIdAsync(toDistrictId, ct);
            var fee = await _ghn.CalculateFeeAsync(serviceId, toDistrictId, toWardCode, weight, height, length, width, ct);

            var items = order.OrderDetails.Select(d => new {
                name = d.ProductVariant?.Product?.Name ?? d.ProductName,
                code = d.ProductVariantId.ToString(),
                quantity = d.Quantity
            });

            var (orderCode, etd) = await _ghn.CreateOrderAsync(toName, toPhone, toAddress, toDistrictId, toWardCode,
                serviceId, weight, height, length, width, paymentTypeId: 1, clientOrderCode: order.Id.ToString(),
                items: items, ct: ct);

            order.ShippingProvider = "GHN";
            order.ShippingServiceId = serviceId;
            order.ShippingFee = fee;
            order.ShippingCode = orderCode;
            order.ShippingStatus = "created";
            order.ExpectedDeliveryTime = etd;

            _db.Set<OrderShipmentEvent>().Add(new OrderShipmentEvent
            {
                OrderId = order.Id,
                Provider = "GHN",
                Status = "created",
                Type = "create",
                OccurredAt = DateTimeOffset.UtcNow
            });

            await _db.SaveChangesAsync(ct);
        }

        public async Task<Order?> GetOrderWithShipmentAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _db.Set<Order>()
                .Include(o => o.ShipmentEvents)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct);
        }
    }
}