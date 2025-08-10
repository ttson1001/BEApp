using System.Net.Http.Json;

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
            _http = http; _opt = opt;
            _http.BaseAddress = new Uri(opt.BaseUrl);
            _http.DefaultRequestHeaders.Remove("Token");
            _http.DefaultRequestHeaders.Add("Token", opt.Token);
            _http.DefaultRequestHeaders.Remove("ShopId");
            _http.DefaultRequestHeaders.Add("ShopId", opt.ShopId.ToString());
        }

        public async Task<int> GetFirstAvailableServiceIdAsync(int toDistrictId, CancellationToken ct = default)
        {
            var payload = new { shop_id = _opt.ShopId, from_district = _opt.FromDistrictId, to_district = toDistrictId };
            var res = await _http.PostAsJsonAsync("v2/shipping-order/available-services", payload, ct);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadFromJsonAsync<dynamic>(cancellationToken: ct);
            return (int)json.data[0].service_id;
        }

        public async Task<decimal> CalculateFeeAsync(int serviceId, int toDistrictId, string toWardCode,
                                                     int weight, int height, int length, int width,
                                                     CancellationToken ct = default)
        {
            var body = new
            {
                service_id = serviceId,
                from_district_id = _opt.FromDistrictId,
                to_district_id = toDistrictId,
                to_ward_code = toWardCode,
                weight,
                height,
                length,
                width,
                shop_id = _opt.ShopId
            };
            var res = await _http.PostAsJsonAsync("v2/shipping-order/fee", body, ct);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadFromJsonAsync<dynamic>(cancellationToken: ct);
            return (decimal)json.data.total;
        }

        public async Task<(string OrderCode, DateTimeOffset? Etd)> CreateOrderAsync(
            string toName, string toPhone, string toAddress, int toDistrictId, string toWardCode,
            int serviceId, int weight, int height, int length, int width,
            int paymentTypeId = 1, string? clientOrderCode = null,
            IEnumerable<object>? items = null, CancellationToken ct = default)
        {
            var payload = new
            {
                shop_id = _opt.ShopId,
                from_name = _opt.FromName,
                from_phone = _opt.FromPhone,
                from_address = _opt.FromAddress,
                from_ward_code = _opt.FromWardCode,
                from_district_id = _opt.FromDistrictId,
                to_name = toName,
                to_phone = toPhone,
                to_address = toAddress,
                to_ward_code = toWardCode,
                to_district_id = toDistrictId,
                service_id = serviceId,
                weight,
                height,
                length,
                width,
                payment_type_id = paymentTypeId,
                client_order_code = clientOrderCode,
                items = items ?? Array.Empty<object>()
            };
            var res = await _http.PostAsJsonAsync("v2/shipping-order/create", payload, ct);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadFromJsonAsync<dynamic>(cancellationToken: ct);
            string code = (string)json.data.order_code;
            DateTimeOffset? etd = null;
            try { etd = DateTimeOffset.Parse((string)json.data.expected_delivery_time); } catch { }
            return (code, etd);
        }
    }
}