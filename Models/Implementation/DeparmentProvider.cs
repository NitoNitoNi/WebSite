using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using MyMvcApp.Models.DataModel;
using MyMvcApp.Models.Interfaces;

namespace MyMvcApp.Models.Implementation
{
    public class DeparmentProvider:IDBDeparment
    {
        private readonly string _connectionString;

        public DeparmentProvider(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string DefaultConnection not found");
        }

        private NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<List<Deparment>> GetDeparments()
        {
            using (var connection = CreateConnection())
            {
                var result = await connection.QueryAsync<Deparment>("SELECT \"DeparmentId\", \"Name\" FROM \"Deparment\"");
                Console.WriteLine($"GetDeparments вернул {result.ToList().Count} отделов");
                foreach (var dept in result.ToList())
                {
                    Console.WriteLine($"  Отдел: {dept.DeparmentId} - {dept.Name}");
                }
                
                return result.ToList();
            }
        }

        public async Task<Deparment> GetDeparment(int id)
        {
            using (var connection = CreateConnection())
            {
                var sql = "SELECT \"DeparmentId\", \"Name\" FROM \"Deparment\" WHERE \"DeparmentId\" = @id";
                return await connection.QueryFirstOrDefaultAsync<Deparment>(sql, new { id });
            }
        }

        public async Task<bool> UpdateDeparment(Deparment deparment)
        {
            using (var connection = CreateConnection())
            {
                var sql = "UPDATE \"Deparment\" SET \"Name\" = @Name WHERE \"DeparmentId\" = @DeparmentId";
                int rows = await connection.ExecuteAsync(sql,deparment);
                return rows > 1;
            }
        }
        public async Task<bool> DeleteDeparment(int id)
        {
            using (var connection = CreateConnection())
            {
                var check = "SELECT COUNT(*) FROM \"Employees\" WHERE \"Deparment\" = @DeparmentId";
                int employeeCount = await connection.ExecuteScalarAsync<int>(check,new {DeparmentId = id});

                if (employeeCount > 0)
                {
                    throw new InvalidOperationException($"Невозмождно удалить отделение, так как в нем есть {employeeCount} сотрудников");
                }

                var sql = "DELETE FROM \"Deparment\" WHERE \"DeparmentId\" = @DeparmentId";
                int rows = await connection.ExecuteAsync(sql,new {DeparmentId = id});
                return rows > 0;
            }
        }
        public async Task<bool> AddDeparment(Deparment deparment)
        {
            using (var connection = CreateConnection())
            {
                var sql =  "INSERT INTO \"Deparment\" (\"Name\") VALUES (@Name)";
                int rows = await connection.ExecuteAsync(sql,deparment);
                return rows > 1;
            }
        }
        public async Task<bool> SaveDeparment(Deparment deparment)
        {
            return deparment.DeparmentId == 0 ? await AddDeparment(deparment) : await UpdateDeparment(deparment);
        }
    }
}
