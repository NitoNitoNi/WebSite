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
                var result = await connection.QueryAsync<Employee>("select EmployeeId, Name, Position, Description, IsActive, Deparment from Employee");
                return result.ToList();
            }
        }

        public async Task<Employee> GetEmployee(int id)
        {
            using (var connection = CreateConnection())
            {
                var sql = "select EmployeeId, Name, Position, Description, IsActive, Deparment from Employee where EmployeeId = @id";
                return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { id });
            }
        }

        public async Task<bool> UpdateEmployee(Employee employee)
        {
            using (var connection = CreateConnection())
            {
                var sql = "update Employee set Name = @Name, Position = @Position, Description = @Description, IsActive = @IsActive, Deparment = @Deparment.DeparmentId where EmployeeId = @EmployeeId";
                int rows = await connection.ExecuteAsync(sql,employee);
                return rows > 1;
            }
        }
        public async Task<bool> DeleteEmployee(int id)
        {
            using (var connection = CreateConnection())
            {
                var check = "select count(*) from Account where EmployeeId = @EmployeeId"; 
                int accountCount = await connection.ExecuteScalarAsync<int>(check,new {emp_id = id});

                if (accountCount > 0)
                {
                    throw new InvalidOperationException($"Невозмождно удалить отделение, так как в нем есть {accountCount} аккаунтов");
                }

                var sql = "delete from Employee where EmployeeId = @EmployeeId";
                int rows = await connection.ExecuteAsync(sql,new {EmployeeId = id});
                return rows > 0;
            }
        }
        public async Task<bool> AddEmployee(Employee employee)
        {
            using (var connection = CreateConnection())
            {
                var sql =  "INSERT INTO Employee (Name) values (@Name)";
                int rows = await connection.ExecuteAsync(sql,employee);
                return rows > 1;
            }
        }
        public async Task<bool> SaveEmployee(Employee employee)
        {
            return employee.EmployeeId == 0 ? await AddEmployee(employee) : await UpdateEmployee(employee);
        }
    }
}
