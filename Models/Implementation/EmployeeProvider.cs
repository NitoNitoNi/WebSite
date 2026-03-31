using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using MyMvcApp.Models.DataModel;
using MyMvcApp.Models.Interfaces;

namespace MyMvcApp.Models.Implementation
{
    public class EmployeeProvider:IDBEmployee
    {
        private readonly string _connectionString;

        public EmployeeProvider(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string DefaultConnection not found");
        }

        private NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<List<Employee>> GetEmployees()
        {
            using (var connection = CreateConnection())
            {
                var result = await connection.QueryAsync<Employee>("SELECT \"EmployeeId\", \"Name\", \"Position\", \"Description\", \"IsActive\", \"DeparmentId\" FROM \"Employees\"");
                return result.ToList();
            }
        }

        public async Task<Employee> GetEmployee(int id)
        {
            using (var connection = CreateConnection())
            {
                var sql = "SELECT \"EmployeeId\", \"Name\", \"Position\", \"Description\", \"IsActive\", \"DeparmentId\" FROM \"Employees\" WHERE \"EmployeeId\" = @id";
                return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { id });
            }
        }

        public async Task<bool> UpdateEmployee(Employee employee)
        {
            using (var connection = CreateConnection())
            {
                var sql = "UPDATE \"Employees\" SET \"Name\" = @Name, \"Position\" = @Position, \"Description\" = @Description, \"IsActive\" = @IsActive, \"DeparmentId\" = @DeparmentId WHERE \"EmployeeId\" = @EmployeeId";
                int rows = await connection.ExecuteAsync(sql,employee);
                return rows > 0;
            }
        }
        public async Task<bool> DeleteEmployee(int id)
        {
            using (var connection = CreateConnection())
            {
                var check = "SELECT COUNT(*) FROM \"Accounts\" WHERE \"EmployeeId\" = @EmployeeId"; 
                int accountCount = await connection.ExecuteScalarAsync<int>(check,new {EmployeeId = id});

                if (accountCount > 0)
                {
                    throw new InvalidOperationException($"Невозмождно удалить отделение, так как в нем есть {accountCount} аккаунтов");
                }

                var sql = "DELETE FROM \"Employees\" WHERE \"EmployeeId\" = @EmployeeId";
                int rows = await connection.ExecuteAsync(sql,new {EmployeeId = id});
                return rows > 0;
            }
        }
        public async Task<bool> AddEmployee(Employee employee)
        {
            using (var connection = CreateConnection())
            {
                var sql =  "INSERT INTO \"Employees\" (\"Name\", \"Position\", \"Description\", \"IsActive\", \"DeparmentId\") VALUES (@Name, @Position, @Description, @IsActive, @DeparmentId)";
                int rows = await connection.ExecuteAsync(sql,employee);
                return rows > 0;
            }
        }
        public async Task<bool> SaveEmployee(Employee employee)
        {
            return employee.EmployeeId == 0 ? await AddEmployee(employee) : await UpdateEmployee(employee);
        }
    }
}
