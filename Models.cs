using Microsoft.EntityFrameworkCore;
namespace CrownBarbers;

public class Booking
{
    public int Id { get; set; }
    public string Ref { get; set; } = "";
    public string ServiceId { get; set; } = "";
    public string BarberId { get; set; } = "";
    public DateOnly Date { get; set; }
    public int StartMin { get; set; }
    public int Minutes { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public int Price { get; set; }
    public bool Promo { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> o) : base(o) { }
    public DbSet<Booking> Bookings => Set<Booking>();
}

public record Service(string Id, string Name, string Desc, int Minutes, int Price);
public record Barber(string Id, string Name, string Role, string Bio);
public record BookingRequest(string ServiceId, string BarberId, string Date, int StartMin, string Name, string Phone, string Email, bool Promo);
public record ConfirmVm(Booking B, Service S, Barber Br, string Google);

public static class Sast
{
    public static DateTime Now => DateTime.UtcNow.AddHours(2); // South Africa has no daylight saving
    public static DateOnly Today => DateOnly.FromDateTime(Now);
}

public static class Catalog
{
    public static readonly Service[] Services =
    {
        new("classic", "Classic Haircut", "Scissor and clipper cut, neck shave and style.", 30, 180),
        new("fade", "Skin Fade", "Tight, clean blend from skin to length. Includes line-up.", 45, 220),
        new("beard", "Beard Trim & Shape", "Shape, trim and edge with a hot towel finish.", 30, 120),
        new("shave", "Hot Towel Shave", "Traditional straight-razor shave with steamed towels.", 30, 160),
        new("combo", "Cut & Beard Combo", "Any haircut plus a full beard trim and shape.", 60, 320),
        new("kids", "Kids Cut (under 12)", "Patient, friendly cut for our youngest clients.", 30, 130),
        new("works", "The Full Works", "Fade or classic cut, beard, hot towel shave and scalp massage.", 75, 450),
    };
    public static readonly Barber[] Barbers =
    {
        new("thabo", "Thabo Mokoena", "Fade specialist", "Eight years behind the chair. If it needs a sharp blend, it goes to Thabo."),
        new("lerato", "Lerato Dlamini", "Beards & shaves", "Our straight-razor expert. Lerato has shaped beards here since 2020."),
        new("ryan", "Ryan van der Merwe", "Classic cuts & kids", "A founding barber with ten years of scissor work and endless patience with kids."),
    };
    public static Service? Service(string? id) => Services.FirstOrDefault(s => s.Id == id);
    public static Barber? Barber(string? id) => Barbers.FirstOrDefault(b => b.Id == id);
    public static (int Open, int Close)? Hours(DateOnly d) => d.DayOfWeek switch
    {
        DayOfWeek.Sunday => null,
        DayOfWeek.Saturday => (8, 16),
        _ => (9, 18)
    };
}

public static class Cal
{
    public const string Loc = "Crown Barber Shop, 12 Cedar Avenue, Fourways, Johannesburg, 2055, South Africa";
    public static string T(int m) => $"{m / 60:00}:{m % 60:00}";
    static string Stamp(DateOnly d, int m) => d.ToString("yyyyMMdd") + $"T{m / 60:00}{m % 60:00}00";
    public static string Title(Booking b) => $"{Catalog.Service(b.ServiceId)!.Name} at Crown Barber Shop";
    public static string Details(Booking b) =>
        $"{Catalog.Service(b.ServiceId)!.Name} with {Catalog.Barber(b.BarberId)!.Name}.\nBooking ref: {b.Ref}.\nPlease arrive 5 minutes early. To cancel or reschedule (24h notice) call 011 555 0142.";
    public static string Google(Booking b) =>
        "https://calendar.google.com/calendar/render?action=TEMPLATE&text=" + Uri.EscapeDataString(Title(b)) +
        "&dates=" + Stamp(b.Date, b.StartMin) + "/" + Stamp(b.Date, b.StartMin + b.Minutes) +
        "&ctz=Africa/Johannesburg&details=" + Uri.EscapeDataString(Details(b)) + "&location=" + Uri.EscapeDataString(Loc);
    static string Esc(string s) => s.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\n", "\\n");
    public static string Ics(Booking b)
    {
        var lines = new[]
        {
            "BEGIN:VCALENDAR", "VERSION:2.0", "PRODID:-//Crown Barber Shop//Booking//EN", "CALSCALE:GREGORIAN", "METHOD:PUBLISH",
            "BEGIN:VTIMEZONE", "TZID:Africa/Johannesburg", "BEGIN:STANDARD", "DTSTART:19700101T000000", "TZOFFSETFROM:+0200", "TZOFFSETTO:+0200", "TZNAME:SAST", "END:STANDARD", "END:VTIMEZONE",
            "BEGIN:VEVENT", $"UID:{b.Ref}@crownbarbershop.example", $"DTSTAMP:{DateTime.UtcNow:yyyyMMdd'T'HHmmss'Z'}",
            $"DTSTART;TZID=Africa/Johannesburg:{Stamp(b.Date, b.StartMin)}", $"DTEND;TZID=Africa/Johannesburg:{Stamp(b.Date, b.StartMin + b.Minutes)}",
            $"SUMMARY:{Esc(Title(b))}", $"LOCATION:{Esc(Loc)}", $"DESCRIPTION:{Esc(Details(b).Replace("\\n", "\n"))}",
            "BEGIN:VALARM", "TRIGGER:-PT2H", "ACTION:DISPLAY", "DESCRIPTION:Appointment reminder", "END:VALARM", "END:VEVENT", "END:VCALENDAR"
        };
        return string.Join("\r\n", lines) + "\r\n";
    }
}