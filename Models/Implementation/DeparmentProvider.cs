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
                var result = await connection.QueryAsync<Deparment>("select dep_id, dep_name from Deparment");
                return result.ToList();
            }
        }

        public async Task<Deparment> GetDeparment(int id)
        {
            using (var connection = CreateConnection())
            {
                var sql = "select dep_id, dep_name from Deparment where dep_id = @id";
                return await connection.QueryFirstOrDefaultAsync<Deparment>(sql, new { id });
            }
        }

        public async Task<bool> UpdateDeparment(Deparment deparment)
        {
            using (var connection = CreateConnection())
            {
                var sql = "update Deparment set dep_name = @dep_name where dep_id = @dep_id";
                int rows = await connection.ExecuteAsync(sql,deparment);
                return rows > 1;
            }
        }
        public async Task<bool> DeleteDeparment(int id)
        {
            using (var connection = CreateConnection())
            {
                var check = "select count(*) from employee where dep_id = @dep_id"; 
                int employeeCount = await connection.ExecuteScalarAsync<int>(check,new {dep_id = id});

                if (employeeCount > 0)
                {
                    throw new InvalidOperationException($"Невозмождно удалить отделение, так как в нем есть {employeeCount} сотрудников");
                }

                var sql = "delete from Deparment where dep_id = @dep_id";
                int rows = await connection.ExecuteAsync(sql,new {dep_id = id});
                return rows > 0;
            }
        }
        public async Task<bool> AddDeparment(Deparment deparment)
        {
            using (var connection = CreateConnection())
            {
                var sql =  "INSERT INTO Deparment (dep_name) values (@dep_name)";
                int rows = await connection.ExecuteAsync(sql,deparment);
                return rows > 1;
            }
        }
        public async Task<bool> SaveDeparment(Deparment deparment)
        {
            return deparment.dep_id == 0 ? await AddDeparment(deparment) : await UpdateDeparment(deparment);
        }
    }
}
