using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace EventHub.Data;

/// <summary>Lop truy cap SQL Server duy nhat. Moi Repository chi goi qua day.</summary>
public class Db
{
    private readonly string _connectionString;

    public Db(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
    }

    public static SqlParameter P(string name, object? value) => new(name, value ?? DBNull.Value);

    public List<T> Query<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] ps)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(ps);
        conn.Open();
        using var reader = cmd.ExecuteReader();
        var list = new List<T>();
        while (reader.Read()) list.Add(map(reader));
        return list;
    }

    public T? QueryOne<T>(string sql, Func<SqlDataReader, T> map, params SqlParameter[] ps) where T : class
        => Query(sql, map, ps).FirstOrDefault();

    public int Execute(string sql, params SqlParameter[] ps)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(ps);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }

    public int Count(string sql, params SqlParameter[] ps)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddRange(ps);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}
