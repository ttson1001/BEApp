// BEAPI/Services/Shipping/ShippingService.cs
using BEAPI.Database;
using BEAPI.Entities;
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

        // ====== ĐÓNG GÓI MẶC ĐỊNH (hard-code) ======
        private const int W_DEFAULT = 1000; // gram
        private const int L_DEFAULT = 20;   // cm
        private const int WIDTH_DEF = 15;   // cm
        private const int H_DEFAULT = 10;   // cm

        public ShippingService(BeContext db, GhnClient ghn, GhnOptions opt, ILogger<ShippingService> logger)
        {
            _db = db; _ghn = ghn; _opt = opt; _logger = logger;
        }

        // ====== Helpers ======
        private (int weight, int length, int width, int height) GetPackDimsFromOrder(Order order)
        {
            // fallback theo số lượng item (nếu bạn muốn dùng theo order thực tế)
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

        // ====================================================================
        // ============== NHÓM HÀM "MẶC ĐỊNH" – CHỈ CẦN orderId ===============
        // ====================================================================

        /// <summary>
        /// Chỉ tính phí GHN (không lưu DB), auto pick service + pack mặc định.
        /// </summary>
        public async Task<(int serviceId, int serviceTypeId, decimal fee)> QuoteFeeDefaultAsync(Guid orderId, CancellationToken ct = default)
        {
            var order = await _db.Set<Order>().AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new Exception("Order not found");

            EnsureAddress(order);

            var (serviceId, serviceTypeId) = await AutoPickServiceAsync(order.DistrictID, ct);

            var fee = await _ghn.CalcFeeAsync(
                toDistrictId: order.DistrictID,
                toWardCode: order.WardCode,
                serviceId: serviceId,
                weight: W_DEFAULT,
                length: L_DEFAULT,
                width: WIDTH_DEF,
                height: H_DEFAULT,
                ct: ct
            );

            return (serviceId, serviceTypeId, fee);
        }

        /// <summary>
        /// Tính phí và LƯU vào Order.ShippingServiceId/ShippingFee (pack mặc định).
        /// </summary>
        public async Task<(int serviceId, int serviceTypeId, decimal fee)> RecalcAndSaveFeeDefaultAsync(Guid orderId, CancellationToken ct = default)
        {
            var (serviceId, serviceTypeId, fee) = await QuoteFeeDefaultAsync(orderId, ct);

            var order = await _db.Set<Order>().FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new Exception("Order not found");

            order.ShippingServiceId = serviceId;
            order.ShippingFee = fee;
            await _db.SaveChangesAsync(ct);

            return (serviceId, serviceTypeId, fee);
        }

        /// <summary>
        /// Tạo đơn GHN (auto pick service + pack mặc định), lưu code/fee/status.
        /// </summary>
        public async Task<(string code, DateTimeOffset? etd, int serviceId, int serviceTypeId, decimal fee)>
            CreateGhnShipmentDefaultAsync(Guid orderId, CancellationToken ct = default)
        {
            var order = await _db.Set<Order>()
                .Include(o => o.OrderDetails).ThenInclude(d => d.ProductVariant).ThenInclude(v => v.Product)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new Exception("Order not found");

            EnsureAddress(order);

            var (serviceId, serviceTypeId) = await AutoPickServiceAsync(order.DistrictID, ct);

            var (code, etd) = await _ghn.CreateOrderAsync(
                serviceId: serviceId,
                serviceTypeId: serviceTypeId,               // yêu cầu GHN
                toDistrictId: order.DistrictID,
                toWardCode: order.WardCode,
                clientOrderCode: order.Id.ToString(),
                toName: order.Customer?.FullName ?? "Customer",
                toPhone: order.PhoneNumber,           // không hardcode
                toAddress: order.StreetAddress,
                items: BuildGhnItems(order),
                weight: W_DEFAULT,
                length: L_DEFAULT,
                width: WIDTH_DEF,
                height: H_DEFAULT,
                paymentTypeId: 1,                           // người nhận trả phí
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

            AddEvent(order.Id, "created", "create");
            await _db.SaveChangesAsync(ct);

            return (code, etd, serviceId, serviceTypeId, fee);
        }

        /// <summary>
        /// Gửi yêu cầu GHN đến lấy hàng (pickup) cho đơn đã có ShippingCode.
        /// </summary>
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


        /// <summary>
        /// Đồng bộ trạng thái GHN về DB.
        /// </summary>
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

        /// <summary>
        /// Hủy đơn GHN (nếu GHN cho phép), lưu event.
        /// </summary>
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

        // ====================================================================
        // ====== NHÓM HÀM "NÂNG CAO" – GIỮ NGUYÊN CHỮ KÝ CŨ (KHÔNG GÃY) ======
        // ====================================================================

        /// <summary>
        /// Tính phí theo serviceId truyền vào (giữ API cũ).
        /// </summary>
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

        /// <summary>
        /// Chọn service "tốt nhất" (hiện tại lấy cái đầu tiên) và trả phí.
        /// </summary>
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

        /// <summary>
        /// Tạo đơn GHN với serviceId truyền vào (giữ API cũ). 
        /// KHÁCH LƯU Ý: GhnClient.CreateOrderAsync ở bản mới yêu cầu serviceTypeId.
        /// Sử dụng AutoPickServiceAsync để lấy luôn serviceTypeId.
        /// </summary>
        public async Task<(string code, DateTimeOffset? etd)> CreateGhnShipmentForOrderAsync(Guid orderId, int? serviceId, CancellationToken ct = default)
        {
            var order = await _db.Set<Order>()
                .Include(o => o.OrderDetails).ThenInclude(od => od.ProductVariant).ThenInclude(pv => pv.Product)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct)
                ?? throw new Exception("Order not found");

            EnsureAddress(order);

            // Chọn service nếu chưa truyền vào + lấy service_type_id
            int finalServiceId;
            int finalServiceTypeId;
            if (serviceId.HasValue)
            {
                finalServiceId = serviceId.Value;
                // cố gắng tìm type từ available-services (khớp theo id)
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
    }
}
