using System.Net.Http.Headers;
using System.Net.Http.Json;                 // ⬅️ cần cho PostAsJsonAsync
using System.Text.Json;

namespace BEAPI.Services.Shipping
{
    public class GhnOptions
    {
        public string BaseUrl { get; set; } = "https://online-gateway.ghn.vn/shiip/public-api/";
        public string Token { get; set; } = string.Empty;
        public int ShopId { get; set; }
        public int FromDistrictId { get; set; }
        public string FromWardCode { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string FromPhone { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
    }

    public class GhnClient
    {
        private readonly HttpClient _http;
        private readonly GhnOptions _opt;

        public GhnClient(HttpClient http, GhnOptions opt)
        {
            _http = http;
            _opt = opt;

            _http.BaseAddress = new Uri(_opt.BaseUrl.TrimEnd('/') + "/");
            _http.DefaultRequestHeaders.Remove("Token");
            _http.DefaultRequestHeaders.Remove("ShopId");
            _http.DefaultRequestHeaders.TryAddWithoutValidation("Token", _opt.Token);
            _http.DefaultRequestHeaders.TryAddWithoutValidation("ShopId", _opt.ShopId.ToString());
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // ============== Helpers ==============
        private static async Task<JsonDocument> PostAndReadJsonAsync(HttpClient http, string path, object payload, CancellationToken ct)
        {
            var res = await http.PostAsJsonAsync(path, payload, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"{path} {(int)res.StatusCode} {res.ReasonPhrase}. Body: {body}");

            try { return JsonDocument.Parse(body); }
            catch { throw new HttpRequestException($"{path} ok but cannot parse JSON. Raw: {body}"); }
        }

        // ============== API wrappers ==============

        // 1) Lấy dịch vụ khả dụng
        public async Task<List<dynamic>> GetAvailableServicesAsync(int toDistrictId, CancellationToken ct = default)
        {
            // Lưu ý: endpoint này dùng from_district / to_district (không có _id)
            var payload = new
            {
                shop_id = _opt.ShopId,
                from_district = _opt.FromDistrictId,
                to_district = toDistrictId
            };

            using var doc = await PostAndReadJsonAsync(_http, "v2/shipping-order/available-services", payload, ct);
            var data = doc.RootElement.GetProperty("data");
            var list = new List<dynamic>();
            foreach (var e in data.EnumerateArray())
            {
                list.Add(new
                {
                    service_id = e.GetProperty("service_id").GetInt32(),
                    service_type_id = e.TryGetProperty("service_type_id", out var t) ? t.GetInt32() : 0, // ⬅️ thêm
                    short_name = e.TryGetProperty("short_name", out var n) ? n.GetString() : null
                });
            }
            return list.Cast<dynamic>().ToList();
        }

        // 2) Tính phí
        public async Task<decimal> CalcFeeAsync(
            int toDistrictId, string toWardCode, int serviceId,
            int weight, int length, int width, int height,
            CancellationToken ct = default)
        {
            var payload = new
            {
                from_district_id = _opt.FromDistrictId,
                to_district_id = toDistrictId,
                to_ward_code = toWardCode,
                service_id = serviceId,
                weight,
                length,
                width,
                height,
                shop_id = _opt.ShopId
            };

            using var doc = await PostAndReadJsonAsync(_http, "v2/shipping-order/fee", payload, ct);
            var total = doc.RootElement.GetProperty("data").GetProperty("total").GetDecimal();
            return total;
        }

        // 3) Tạo đơn GHN (bắt buộc truyền cả service_type_id)
        public async Task<(string OrderCode, DateTimeOffset? Etd)> CreateOrderAsync(
            int serviceId, int serviceTypeId,                 // ⬅️ thêm serviceTypeId
            int toDistrictId, string toWardCode,
            string clientOrderCode,
            string toName, string toPhone, string toAddress,  // ⬅️ dùng tham số toPhone, không hardcode
            IEnumerable<object>? items,
            int weight, int length, int width, int height,
            int paymentTypeId = 1,
            CancellationToken ct = default)
        {
            var payload = new
            {
                shop_id = _opt.ShopId,
                from_district_id = _opt.FromDistrictId,
                from_ward_code = _opt.FromWardCode,
                from_name = _opt.FromName,
                from_phone = _opt.FromPhone,
                from_address = _opt.FromAddress,

                to_district_id = toDistrictId,
                to_ward_code = toWardCode,
                to_name = toName,
                to_phone = toPhone,               // ⬅️ FIX: dùng tham số
                to_address = toAddress,

                service_id = serviceId,
                service_type_id = serviceTypeId,  // ⬅️ FIX: bắt buộc
                weight,
                length,
                width,
                height,
                payment_type_id = paymentTypeId,
                client_order_code = clientOrderCode,
                items = items ?? Array.Empty<object>(),
                required_note = "KHONGCHOXEMHANG"
            };

            using var doc = await PostAndReadJsonAsync(_http, "v2/shipping-order/create", payload, ct);
            var data = doc.RootElement.GetProperty("data");
            var code = data.GetProperty("order_code").GetString() ?? string.Empty;

            DateTimeOffset? etd = null;
            if (data.TryGetProperty("expected_delivery_time", out var etdProp)
                && etdProp.ValueKind == JsonValueKind.String
                && DateTimeOffset.TryParse(etdProp.GetString(), out var parsed))
            {
                etd = parsed;
            }

            return (code, etd);
        }

        // 4) /shop/all – đọc linh hoạt
        public async Task<List<(int shopId, string name, string phone, int districtId, string wardCode, string address)>>
            GetShopsAsync(CancellationToken ct = default)
        {
            using var doc = await PostAndReadJsonAsync(_http, "v2/shop/all", new { }, ct);
            var root = doc.RootElement;

            if (!root.TryGetProperty("data", out var data))
                throw new Exception("GHN /shop/all: missing 'data'");

            IEnumerable<JsonElement> items;

            if (data.ValueKind == JsonValueKind.Array)
                items = data.EnumerateArray(); // data = [ {...} ]
            else if (data.ValueKind == JsonValueKind.Object &&
                     data.TryGetProperty("shops", out var shops) &&
                     shops.ValueKind == JsonValueKind.Array)
                items = shops.EnumerateArray(); // data = { shops: [ {...} ] }
            else if (data.ValueKind == JsonValueKind.Object)
                items = new[] { data };         // data = { ... } (1 shop)
            else
                throw new Exception($"GHN /shop/all: unexpected data type = {data.ValueKind}. Raw: {root}");

            var list = new List<(int, string, string, int, string, string)>();
            foreach (var e in items)
            {
                int shopId = e.TryGetProperty("shop_id", out var sid) ? sid.GetInt32() : 0;
                string name = e.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                string phone = e.TryGetProperty("phone", out var p) ? p.GetString() ?? "" : "";
                int districtId = e.TryGetProperty("district_id", out var d) ? d.GetInt32() : 0;
                string wardCode = e.TryGetProperty("ward_code", out var w) ? w.GetString() ?? "" : "";
                string address = e.TryGetProperty("address", out var a) ? a.GetString() ?? "" : "";
                list.Add((shopId, name, phone, districtId, wardCode, address));
            }
            return list;
        }

        public async Task SetDefaultShopFromApiAsync(CancellationToken ct = default)
        {
            var shops = await GetShopsAsync(ct);
            if (shops.Count == 0) throw new Exception("No shops found for this token");
            var s = shops[0];

            _opt.ShopId = s.shopId;
            _opt.FromDistrictId = s.districtId;
            _opt.FromWardCode = s.wardCode;
            _opt.FromName = s.name;
            _opt.FromPhone = s.phone;
            _opt.FromAddress = s.address;
        }

        public async Task RequestPickupSmartAsync(IEnumerable<string> orderCodes, CancellationToken ct = default)
        {
            var codes = orderCodes.ToArray();
            var payload = new { shop_id = _opt.ShopId, order_codes = codes, note = "" };

            // 1) ready_to_pick
            try { using var _ = await PostAndReadJsonAsync(_http, "v2/switch-status/ready_to_pick", payload, ct); return; }
            catch (HttpRequestException ex) when (ex.Message.Contains("permission") || ex.Message.Contains("Not Found") || ex.Message.Contains("400"))
            { /* fallback */ }

            // 2) pickup/create (một số shop dev/prod dùng cái này)
            try
            {
                var p2 = new
                {
                    shop_id = _opt.ShopId,
                    client_pickup_code = Guid.NewGuid().ToString("N")[..16],
                    pickup_time = (string?)null,
                    note = "",
                    order_codes = codes
                };
                using var _ = await PostAndReadJsonAsync(_http, "v2/pickup/create", p2, ct); return;
            }
            catch (HttpRequestException)
            { /* fallback */ }

            // 3) ready_to_ship (thường dùng khi bạn đã giao kiện cho GHN)
            using var __ = await PostAndReadJsonAsync(_http, "v2/switch-status/ready_to_ship", payload, ct);
        }

        public async Task<JsonDocument> GetOrderDetailAsync(string orderCode, CancellationToken ct = default)
        {
            return await PostAndReadJsonAsync(_http, "v2/shipping-order/detail", new { order_code = orderCode }, ct);
        }

        public async Task CancelOrderAsync(string orderCode, string? note = null, CancellationToken ct = default)
        {
            var payload = new { order_code = orderCode, cancel_note = note ?? "" };
            using var _ = await PostAndReadJsonAsync(_http, "v2/switch-status/cancel", payload, ct);
        }
    }
}
