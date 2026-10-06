using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CrownBarbers;

public class BookingController : Controller
{
    private readonly AppDb db;
    public BookingController(AppDb db) { this.db = db; }
    static readonly SemaphoreSlim Gate = new(1, 1);
    static bool Free(List<Booking> day, string barberId, int st, int du) =>
        !day.Any(k => k.BarberId == barberId && st < k.StartMin + k.Minutes && k.StartMin < st + du);

    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Slots(string date, string service, string barber)
    {
        var sv = Catalog.Service(service);
        if (sv is null || !DateOnly.TryParse(date, out var d)) return BadRequest();
        var h = Catalog.Hours(d);
        if (h is null) return Json(new { closed = true, slots = Array.Empty<object>() });
        var day = await db.Bookings.Where(b => b.Date == d).ToListAsync();
        var now = Sast.Now;
        var list = new List<object>();
        for (int t = h.Value.Open * 60; t + sv.Minutes <= h.Value.Close * 60; t += 30)
        {
            bool past = d < Sast.Today || (d == Sast.Today && t <= now.Hour * 60 + now.Minute + 30);
            bool ok = !past && Catalog.Barbers.Any(b => (barber == "any" || b.Id == barber) && Free(day, b.Id, t, sv.Minutes));
            list.Add(new { t, label = Cal.T(t), ok });
        }
        return Json(new { closed = false, slots = list });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BookingRequest r)
    {
        var sv = Catalog.Service(r.ServiceId);
        if (sv is null) return BadRequest(new { error = "Please choose a service." });
        if (!DateOnly.TryParse(r.Date, out var d)) return BadRequest(new { error = "Please choose a date." });
        var h = Catalog.Hours(d);
        if (h is null || d < Sast.Today || d > Sast.Today.AddDays(60)) return BadRequest(new { error = "Please choose an open day within the next 60 days." });
        if (r.StartMin % 30 != 0 || r.StartMin < h.Value.Open * 60 || r.StartMin + sv.Minutes > h.Value.Close * 60) return BadRequest(new { error = "That time is outside opening hours." });
        var now = Sast.Now;
        if (d == Sast.Today && r.StartMin <= now.Hour * 60 + now.Minute + 30) return BadRequest(new { error = "That time has already passed." });
        var name = (r.Name ?? "").Trim(); var email = (r.Email ?? "").Trim(); var phone = (r.Phone ?? "").Trim();
        if (name.Length < 2) return BadRequest(new { error = "Please enter your full name." });
        if (phone.Count(char.IsDigit) < 9) return BadRequest(new { error = "Please enter a valid phone number." });
        if (!Regex.IsMatch(email, @"^\S+@\S+\.\S+$")) return BadRequest(new { error = "Please enter a valid email address." });

        await Gate.WaitAsync();
        try
        {
            var day = await db.Bookings.Where(b => b.Date == d).ToListAsync();
            var barber = Catalog.Barbers.Where(b => r.BarberId == "any" || b.Id == r.BarberId).FirstOrDefault(b => Free(day, b.Id, r.StartMin, sv.Minutes));
            if (barber is null) return Conflict(new { error = "Sorry, that time was just taken. Please pick another." });
            var bk = new Booking
            {
                Ref = "HR" + Guid.NewGuid().ToString("N")[..6].ToUpper(), ServiceId = sv.Id, BarberId = barber.Id, Date = d,
                StartMin = r.StartMin, Minutes = sv.Minutes, Name = name, Phone = phone, Email = email,
                Promo = r.Promo, Price = (int)Math.Round(sv.Price * (r.Promo ? 0.85 : 1.0))
            };
            db.Bookings.Add(bk);
            await db.SaveChangesAsync();
            return Json(new { url = Url.Action("Confirmation", new { id = bk.Ref }) });
        }
        finally { Gate.Release(); }
    }

    public async Task<IActionResult> Confirmation(string id)
    {
        var b = await db.Bookings.FirstOrDefaultAsync(x => x.Ref == id);
        if (b is null) return NotFound();
        return View(new ConfirmVm(b, Catalog.Service(b.ServiceId)!, Catalog.Barber(b.BarberId)!, Cal.Google(b)));
    }

    public async Task<IActionResult> Ics(string id)
    {
        var b = await db.Bookings.FirstOrDefaultAsync(x => x.Ref == id);
        if (b is null) return NotFound();
        return File(Encoding.UTF8.GetBytes(Cal.Ics(b)), "text/calendar; charset=utf-8", $"crown-barber-shop-{b.Date:yyyy-MM-dd}.ics");
    }
}