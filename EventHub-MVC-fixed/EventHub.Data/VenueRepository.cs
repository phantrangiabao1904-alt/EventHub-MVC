using EventHub.Models;
using Microsoft.Data.SqlClient;
using static EventHub.Data.Db;

namespace EventHub.Data;

public class VenueRepository
{
    private readonly Db _db;
    public VenueRepository(Db db) => _db = db;

    private const string Select = "SELECT id, name, description, address, capacity, price, status, image_url FROM venues";

    private static Venue Map(SqlDataReader r) => new()
    {
        Id = r.GetInt64(0), Name = r.GetString(1),
        Description = r.IsDBNull(2) ? null : r.GetString(2),
        Address = r.GetString(3), Capacity = r.GetInt32(4), Price = r.GetDecimal(5),
        Status = r.GetString(6), ImageUrl = r.IsDBNull(7) ? null : r.GetString(7)
    };

    public List<Venue> GetAll(string? search = null) =>
        string.IsNullOrWhiteSpace(search)
            ? _db.Query(Select + " ORDER BY id DESC", Map)
            : _db.Query(Select + " WHERE name LIKE @s OR address LIKE @s ORDER BY id DESC", Map, P("@s", "%" + search.Trim() + "%"));

    public Venue? GetById(long id) => _db.QueryOne(Select + " WHERE id = @id", Map, P("@id", id));
    public int CountAll() => _db.Count("SELECT COUNT(*) FROM venues");

    public void Create(Venue v) => _db.Execute(
        "INSERT INTO venues(name, description, address, capacity, price, status, image_url) VALUES (@n,@d,@a,@c,@p,@s,@i)",
        P("@n", v.Name), P("@d", v.Description), P("@a", v.Address), P("@c", v.Capacity),
        P("@p", v.Price), P("@s", v.Status), P("@i", v.ImageUrl));

    public void Update(Venue v) => _db.Execute(
        "UPDATE venues SET name=@n, description=@d, address=@a, capacity=@c, price=@p, status=@s, image_url=@i WHERE id=@id",
        P("@n", v.Name), P("@d", v.Description), P("@a", v.Address), P("@c", v.Capacity),
        P("@p", v.Price), P("@s", v.Status), P("@i", v.ImageUrl), P("@id", v.Id));

    public void Delete(long id) => _db.Execute("DELETE FROM venues WHERE id = @id", P("@id", id));
}
