using EventHub.Models;
using Microsoft.Data.SqlClient;
using static EventHub.Data.Db;

namespace EventHub.Data;

public class BookingRepository
{
    private readonly Db _db;
    public BookingRepository(Db db) => _db = db;

    private const string Select = @"SELECT b.id, b.user_id, b.venue_id, b.event_date, b.start_time, b.end_time, b.guest_count, b.status, b.notes,
       v.name, u.name
       FROM bookings b
       JOIN venues v ON v.id = b.venue_id
       JOIN users u ON u.id = b.user_id";

    private static Booking Map(SqlDataReader r) => new()
    {
        Id = r.GetInt64(0), UserId = r.GetInt64(1), VenueId = r.GetInt64(2), EventDate = r.GetDateTime(3),
        StartTime = r.GetTimeSpan(4), EndTime = r.GetTimeSpan(5), GuestCount = r.GetInt32(6), Status = r.GetString(7),
        Notes = r.IsDBNull(8) ? null : r.GetString(8), VenueName = r.GetString(9), UserName = r.GetString(10)
    };

    public List<Booking> GetAll() => _db.Query(Select + " ORDER BY b.id DESC", Map);
    public List<Booking> GetByUser(long userId) => _db.Query(Select + " WHERE b.user_id = @u ORDER BY b.id DESC", Map, P("@u", userId));
    public Booking? GetById(long id) => _db.QueryOne(Select + " WHERE b.id = @id", Map, P("@id", id));

    public void Create(Booking b) => _db.Execute(
        "INSERT INTO bookings(user_id, venue_id, event_date, start_time, end_time, guest_count, status, notes) VALUES (@u,@v,@d,@s,@e,@g,@st,@n)",
        P("@u", b.UserId), P("@v", b.VenueId), P("@d", b.EventDate), P("@s", b.StartTime), P("@e", b.EndTime),
        P("@g", b.GuestCount), P("@st", b.Status), P("@n", b.Notes));

    public bool HasConflict(long venueId, DateTime date, TimeSpan start, TimeSpan end) =>
        _db.Count("SELECT COUNT(*) FROM bookings WHERE venue_id = @v AND event_date = @d AND status <> 'CANCELLED' AND start_time < @e AND end_time > @s",
            P("@v", venueId), P("@d", date), P("@s", start), P("@e", end)) > 0;

    public void UpdateStatus(long id, string status) => _db.Execute("UPDATE bookings SET status = @s WHERE id = @id", P("@s", status), P("@id", id));
}
