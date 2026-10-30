using System.Security.Cryptography;
using EventHub.Models;
using Microsoft.Data.SqlClient;
using static EventHub.Data.Db;

namespace EventHub.Data;

public class UserRepository
{
    private readonly Db _db;
    public UserRepository(Db db) => _db = db;

    private const string Select = "SELECT id, name, email, password_hash, role FROM users";

    private static User Map(SqlDataReader r) => new()
    {
        Id = r.GetInt64(0), Name = r.GetString(1), Email = r.GetString(2),
        PasswordHash = r.GetString(3), Role = r.GetString(4)
    };

    public List<User> GetAll() => _db.Query(Select + " ORDER BY id", Map);
    public User? GetByEmail(string email) => _db.QueryOne(Select + " WHERE email = @e", Map, P("@e", email));
    public int CountAll() => _db.Count("SELECT COUNT(*) FROM users");

    public void Create(User u) => _db.Execute(
        "INSERT INTO users(name, email, password_hash, role) VALUES (@n, @e, @p, @r)",
        P("@n", u.Name), P("@e", u.Email), P("@p", u.PasswordHash), P("@r", u.Role));

    public void SetRole(long id, string role) =>
        _db.Execute("UPDATE users SET role = @r WHERE id = @id", P("@r", role), P("@id", id));

    public void Delete(long id) => _db.Execute("DELETE FROM users WHERE id = @id", P("@id", id));

    // ---- Mat khau: PBKDF2 ----
    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
