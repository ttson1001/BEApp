using BEAPI.Database;
using BEAPI.Entities;
using BEAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BEAPI.Services.Shipping
{
    public class ShippingService
    {
        private readonly BeContext _db;
        private readonly GhnClient _ghn;
        private readonly GhnOptions _opt;
        private readonly ILogger<ShippingService> _logger;
        private readonly IUserService _userService;

        private const int W_DEFAULT = 1000; // gram
        private const int L_DEFAULT = 20;   // cm
        private const int WIDTH_DEF = 15;   // cm
        private const int H_DEFAULT = 10;   // cm

        public ShippingService(IUserService userService, BeContext db, GhnClient ghn, GhnOptions opt, ILogger<ShippingService> logger)
        {
            _db = db; _ghn = ghn; _opt = opt; _logger = logger;
            _userService = userService;
        }

        private (int weight, int length, int width, int height) GetPackDimsFromOrder(Order order)
        { 
            int weight = Math.Max((int)order.OrderDetails.Sum(d => d.Quantity * 250), 500); // gram
            int length = 20, width = 15, height = 10; // cm
            return (weight, length, width, height);
        }

        private void EnsureAddress(Order o)
        {
            if (string.IsNullOrWhiteSpace(o.WardCode)) throw new Exception("Order missing WardCode (GHN requires ward_code)");
            if (o.DistrictID <= 0) throw new Exception("Order missing GHN DistrictID");
            if (string.IsNullOrWhiteSpace(o.StreetAddress)) throw new Exception("Order missing StreetAddress");
            if (string.IsNullOrWhiteSpace(o.PhoneNumber)) throw new Exception("Order missing PhoneNumber");
        }

        private async Task<(int serviceId, int serviceTypeId)> AutoPickServiceAsync(int toDistrictId, CancellationToken ct)
        {
            var services = await _ghn.GetAvailableServicesAsync(toDistrictId, ct);
            if (services == null || services.Count == 0)
                throw new Exception("No GHN service available for this route");

            int serviceId = (int)services[0].service_id;
            int serviceTypeId = 0;
            try { serviceTypeId = (int)services[0].service_type_id; } catch { /* may be absent on some envs */ }

            return (serviceId, serviceTypeId);
        }

        private IEnumerable<object> BuildGhnItems(Order order)
        {
            return order.OrderDetails.Select(d => new
            {
                name = d.ProductVariant?.Product?.Name ?? d.ProductName,
                code = d.ProductVariantId.ToString(),
                quantity = d.Quantity
            });
        }

        private void AddEvent(Guid orderId, string status, string type)
        {
            _db.Set<OrderShipmentEvent>().Add(new OrderShipmentEvent
            {
                OrderId = orderId,
                Provider = "GHN",
                Status = status,
                Type = type,
                OccurredAt = DateTimeOffset.UtcNow
            });
        }

        public async Task<(int serviceId, int serviceTypeId, decimal fee)> QuoteFeeDefaultAsync(Guid adrresId, CancellationToken ct = default)
        {
            var address = await _db.Set<Address>().AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == adrresId, ct)
                ?? throw new Exception("Address not found");

            var (serviceId, serviceTypeId) = await AutoPickServiceAsync(address.DistrictID, ct);

            var fee = await _ghn.CalcFeeAsync(
                toDistrictId: address.DistrictID,
                toWardCode: address.WardCode,
                serviceId: serviceId,
                weight: W_DEFAULT,
                length: L_DEFAULT,
                width: WIDTH_DEF,
                height: H_DEFAULT,
                ct: ct
            );

            return (serviceId, serviceTypeId, fee);
        }

        public async Task<(int serviceId, int serviceTypeId, decimal fee)> RecalcAndSaveFeeDefaultAsync(Guid addressId, CancellationToken ct = default)
        {
            var (serviceId, serviceTypeId, fee) = await QuoteFeeDefaultAsync(addressId, ct);

            return (serviceId, serviceTypeId, fee);
        }

        public async Task<(string code, DateTimeOffset? etd, int serviceId, int serviceTypeId, decimal fee)>
            CreateGhnShipmentDefaultAsync(Guid orderId, CancellationToken ct = default)
        {
            var order = await _db.Set<Order>()
                .Include(o => o.OrderDetails).ThenInclude(d => d.ProductVariant).ThenInclude(v => v.Product)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new Exception("Order not found");
            await _userService.SendNotificationToUserAsync(order.CustomerId, "Silver Cart", "Đơn hàng của bạn vận chuyển đến đơn vị giao hàng");
            EnsureAddress(order);

            var (serviceId, serviceTypeId) = await AutoPickServiceAsync(order.DistrictID, ct);

            var (code, etd) = await _ghn.CreateOrderAsync(
                serviceId: serviceId,
                serviceTypeId: serviceTypeId,
                toDistrictId: order.DistrictID,
                toWardCode: order.WardCode,
                clientOrderCode: order.Id.ToString(),
                toName: order.Customer?.FullName ?? "Customer",
                toPhone: order.PhoneNumber,         
                toAddress: order.StreetAddress,
                items: BuildGhnItems(order),
                weight: W_DEFAULT,
                length: L_DEFAULT,
                width: WIDTH_DEF,
                height: H_DEFAULT,
                paymentTypeId: 1,                           
                ct: ct
            );

            var fee = await _ghn.CalcFeeAsync(order.DistrictID, order.WardCode, serviceId,
                                              W_DEFAULT, L_DEFAULT, WIDTH_DEF, H_DEFAULT, ct);

            order.ShippingProvider = "GHN";
            order.ShippingServiceId = serviceId;
            order.ShippingFee = fee;
            order.ShippingCode = code;
            order.ExpectedDeliveryTime = etd;
            order.ShippingStatus = "created";
            order.OrderStatus = Entities.Enum.OrderStatus.PendingConfirm;

            AddEvent(order.Id, "created", "create");
            await _db.SaveChangesAsync(ct);

            return (code, etd, serviceId, serviceTypeId, fee);
        }

        public async Task RequestPickupAsync(Guid orderId, DateTimeOffset? pickupAt = null, CancellationToken ct = default)
        {
            var order = await _db.Set<Order>().FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new Exception("Order not found");
            if (string.IsNullOrWhiteSpace(order.ShippingCode))
                throw new Exception("Order has no GHN code");

            await _ghn.RequestPickupSmartAsync(new[] { order.ShippingCode }, ct);

            _db.Set<OrderShipmentEvent>().Add(new OrderShipmentEvent
            {
                OrderId = order.Id,
                Provider = "GHN",
                Status = "pickup_requested",
                Type = "pickup",
                OccurredAt = DateTimeOffset.UtcNow
            });
            await _db.SaveChangesAsync(ct);
        }

        public async Task<string> SyncShipmentStatusAsync(Guid orderId, CancellationToken ct = default)
        {
            var order = await _db.Set<Order>().FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new Exception("Order not found");
            if (string.IsNullOrWhiteSpace(order.ShippingCode))
                throw new Exception("Order has no GHN code");

            using var doc = await _ghn.GetOrderDetailAsync(order.ShippingCode!, ct);
            var data = doc.RootElement.GetProperty("data");
            var ghnStatus = data.GetProperty("status").GetString() ?? "";

            order.ShippingStatus = ghnStatus;
            AddEvent(order.Id, ghnStatus, "sync");
            await _db.SaveChangesAsync(ct);

            return ghnStatus;
        }

        public async Task CancelShipmentAsync(Guid orderId, string? note = null, CancellationToken ct = default)
        {
            var order = await _db.Set<Order>().FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new Exception("Order not found");
            if (string.IsNullOrWhiteSpace(order.ShippingCode))
                throw new Exception("Order has no GHN code");

            await _ghn.CancelOrderAsync(order.ShippingCode!, note, ct);

            order.ShippingStatus = "cancelled";
            AddEvent(order.Id, "cancelled", "cancel");
            await _db.SaveChangesAsync(ct);
        }

        public async Task<decimal> RecalcShippingFeeAsync(Guid orderId, int serviceId, CancellationToken ct = default)
        {
            var order = await _db.Set<Order>()
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new Exception("Order not found");

            EnsureAddress(order);
            var (weight, length, width, height) = GetPackDimsFromOrder(order);

            var fee = await _ghn.CalcFeeAsync(order.DistrictID, order.WardCode, serviceId,
                                              weight, length, width, height, ct);

            order.ShippingServiceId = serviceId;
            order.ShippingFee = fee;
            await _db.SaveChangesAsync(ct);
            return fee;
        }

        public async Task<(int serviceId, decimal fee)> QuoteBestServiceAsync(Guid orderId, CancellationToken ct = default)
        {
            var order = await _db.Set<Order>()
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new Exception("Order not found");

            EnsureAddress(order);

            var services = await _ghn.GetAvailableServicesAsync(order.DistrictID, ct);
            if (services == null || services.Count == 0) throw new Exception("No GHN service available");

            int serviceId = (int)services[0].service_id;
            var (weight, length, width, height) = GetPackDimsFromOrder(order);

            var fee = await _ghn.CalcFeeAsync(order.DistrictID, order.WardCode, serviceId,
                                              weight, length, width, height, ct);
            return (serviceId, fee);
        }

        public async Task<(string code, DateTimeOffset? etd)> CreateGhnShipmentForOrderAsync(Guid orderId, int? serviceId, CancellationToken ct = default)
        {
            var order = await _db.Set<Order>()
                .Include(o => o.OrderDetails).ThenInclude(od => od.ProductVariant).ThenInclude(pv => pv.Product)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new Exception("Order not found");

            EnsureAddress(order);

            int finalServiceId;
            int finalServiceTypeId;
            if (serviceId.HasValue)
            {
                finalServiceId = serviceId.Value;
                var svcs = await _ghn.GetAvailableServicesAsync(order.DistrictID, ct);
                var svc = svcs.FirstOrDefault(s => (int)s.service_id == finalServiceId);
                finalServiceTypeId = svc != null ? (int)(svc.service_type_id ?? 0) : 0;
            }
            else
            {
                (finalServiceId, finalServiceTypeId) = await AutoPickServiceAsync(order.DistrictID, ct);
            }

            var (weight, length, width, height) = GetPackDimsFromOrder(order);

            var (code, etd) = await _ghn.CreateOrderAsync(
                serviceId: finalServiceId,
                serviceTypeId: finalServiceTypeId,
                toDistrictId: order.DistrictID,
                toWardCode: order.WardCode,
                clientOrderCode: order.Id.ToString(),
                toName: order.Customer?.FullName ?? "Customer",
                toPhone: order.PhoneNumber,
                toAddress: order.StreetAddress,
                items: BuildGhnItems(order),
                weight: weight,
                length: length,
                width: width,
                height: height,
                paymentTypeId: 1,
                ct: ct
            );

            order.ShippingProvider = "GHN";
            order.ShippingServiceId = finalServiceId;
            order.ShippingCode = code;
            order.ExpectedDeliveryTime = etd;
            order.ShippingFee ??= await _ghn.CalcFeeAsync(order.DistrictID, order.WardCode, finalServiceId,
                                                          weight, length, width, height, ct);

            AddEvent(order.Id, "created", "create");
            await _db.SaveChangesAsync(ct);

            return (code, etd);
        }

        public async Task SimulateStatusAsync(Guid orderId, string status, CancellationToken ct = default)
        {
            var order = await _db.Set<Order>().FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new Exception("Order not found");

            order.ShippingStatus = status;
            _db.Set<OrderShipmentEvent>().Add(new OrderShipmentEvent
            {
                OrderId = order.Id,
                Provider = "GHN",
                Status = status,
                Type = "simulate",
                OccurredAt = DateTimeOffset.UtcNow
            });
            await _db.SaveChangesAsync(ct);
        }

        public async Task<Entities.Enum.OrderStatus> AdvanceOrderOnlyAsync(Guid orderId, CancellationToken ct = default)
        {
            var order = await _db.Set<Order>().FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new Exception("Order not found");

            var current = order.OrderStatus;
            var next = GetNextStatus(current);

            if (next != current)
            {
                order.OrderStatus = next;
                await _db.SaveChangesAsync(ct);
            }

            var nextText = GetOrderStatusText(next);

            // gửi cho customer
            await _userService.SendNotificationToUserAsync(
                order.CustomerId,
                "Silver Cart",
                $"Đơn hàng của bạn đã chuyển sang trạng thái: {nextText}"
            );

            // gửi cho elder nếu có
            if (order.ElderId != null)
            {
                await _userService.SendNotificationToUserAsync(
                    order.ElderId.Value,
                    "Silver Cart",
                    $"Đơn hàng bạn theo dõi đã chuyển sang trạng thái: {nextText}"
                );
            }

            return next;
        }
        private static Entities.Enum.OrderStatus GetNextStatus(Entities.Enum.OrderStatus current) =>
            current switch
            {
                Entities.Enum.OrderStatus.Created => Entities.Enum.OrderStatus.Paid,
                Entities.Enum.OrderStatus.Paid => Entities.Enum.OrderStatus.PendingConfirm,
                Entities.Enum.OrderStatus.PendingConfirm => Entities.Enum.OrderStatus.PendingPickup,
                Entities.Enum.OrderStatus.PendingPickup => Entities.Enum.OrderStatus.PendingDelivery,
                Entities.Enum.OrderStatus.PendingDelivery => Entities.Enum.OrderStatus.Shipping,
                Entities.Enum.OrderStatus.Shipping => Entities.Enum.OrderStatus.Delivered,
                Entities.Enum.OrderStatus.Delivered => Entities.Enum.OrderStatus.Completed,
                _ => current
            };

            private static string GetOrderStatusText(Entities.Enum.OrderStatus status) =>
            status switch
            {
                Entities.Enum.OrderStatus.Created => "Mới tạo",
                Entities.Enum.OrderStatus.Paid => "Đã thanh toán",
                Entities.Enum.OrderStatus.PendingConfirm => "Chờ xác nhận",
                Entities.Enum.OrderStatus.PendingPickup => "Chờ lấy hàng",
                Entities.Enum.OrderStatus.PendingDelivery => "Chờ giao hàng",
                Entities.Enum.OrderStatus.Shipping => "Đang giao",
                Entities.Enum.OrderStatus.Delivered => "Đã giao",
                Entities.Enum.OrderStatus.Completed => "Hoàn tất",
                Entities.Enum.OrderStatus.Canceled => "Đã hủy",
                Entities.Enum.OrderStatus.Fail => "Thất bại",
                _ => status.ToString()
            };
    }
}
