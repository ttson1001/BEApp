// BEAPI/Controllers/GhnWebhookController.cs
using BEAPI.Database;
using BEAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/ghn/webhook")]
public class GhnWebhookController : ControllerBase
{
    private readonly BeContext _db;
    public GhnWebhookController(BeContext db) => _db = db;

    public class GhnWebhookPayload
    {
        public string Type { get; set; } = string.Empty;     // create, Switch_status, ...
        public string Status { get; set; } = string.Empty;   // delivering, delivered, ...
        public string OrderCode { get; set; } = string.Empty; // GHN order_code
        public DateTimeOffset Time { get; set; }
        public string? Reason { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] GhnWebhookPayload payload)
    {
        var order = await _db.Set<Order>().FirstOrDefaultAsync(o => o.ShippingCode == payload.OrderCode);
        if (order == null) return Ok(); // idempotent; vẫn trả 200 để GHN không retry

        order.ShippingStatus = payload.Status;
        _db.Set<OrderShipmentEvent>().Add(new OrderShipmentEvent
        {
            OrderId = order.Id,
            Provider = "GHN",
            Status = payload.Status,
            Type = payload.Type,
            Reason = payload.Reason,
            OccurredAt = payload.Time
        });

        await _db.SaveChangesAsync();
        return Ok(new { received = true });
    }
}