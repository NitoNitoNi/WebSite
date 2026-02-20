using Npgsql;

namespace MyMvcApp.Models.Interfaces
{
    public interface IDbConnection
    {
        NpgsqlConnection CreateConnection();
    }
}
