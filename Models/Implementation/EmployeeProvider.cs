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
                var result = await connection.QueryAsync<Employee>("select emp_id, emp_name, position, description, IsActive, dep_id from Employee");
                return result.ToList();
            }
        }

        public async Task<Employee> GetEmployee(int id)
        {
            using (var connection = CreateConnection())
            {
                var sql = "select emp_id, emp_name, position, description, IsActive, dep_id from Employee where emp_id = @id";
                return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { id });
            }
        }

        public async Task<bool> UpdateEmployee(Employee employee)
        {
            using (var connection = CreateConnection())
            {
                var sql = "update Employee set emp_name = @emp_name, position = @position, description = @description, IsActive = @IsActive, dep_id = @Deparment.dep_id where emp_id = @emp_id";
                int rows = await connection.ExecuteAsync(sql,employee);
                return rows > 1;
            }
        }
        public async Task<bool> DeleteEmployee(int id)
        {
            using (var connection = CreateConnection())
            {
                var check = "select count(*) from account where emp_id = @emp_id"; 
                int accountCount = await connection.ExecuteScalarAsync<int>(check,new {emp_id = id});

                if (accountCount > 0)
                {
                    throw new InvalidOperationException($"Невозмождно удалить отделение, так как в нем есть {accountCount} аккаунтов");
                }

                var sql = "delete from Employee where emp_id = @emp_id";
                int rows = await connection.ExecuteAsync(sql,new {emp_id = id});
                return rows > 0;
            }
        }
        public async Task<bool> AddEmployee(Employee employee)
        {
            using (var connection = CreateConnection())
            {
                var sql =  "INSERT INTO Employee (emp_name) values (@emp_name)";
                int rows = await connection.ExecuteAsync(sql,employee);
                return rows > 1;
            }
        }
        public async Task<bool> SaveEmployee(Employee employee)
        {
            return employee.emp_id == 0 ? await AddEmployee(employee) : await UpdateEmployee(employee);
        }
    }
}
