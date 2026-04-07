using Dapper;
using Npgsql;
using MyMvcApp.Models.DataModel;
using MyMvcApp.Models.Interfaces;
using WebSite.Models.DataModel;

namespace MyMvcApp.Models.Implementation
{
    public class AccountProvider : IDBAccount
    {
        private readonly string _connectionString;

        public AccountProvider(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        private NpgsqlConnection CreateConnection() => new(_connectionString);

        public async Task<List<Accounts>> GetAccounts()
        {
            using var conn = CreateConnection();
            const string sql = "SELECT * FROM \"Accounts\" ORDER BY \"Data\" DESC";
            var result = await conn.QueryAsync<Accounts>(sql);
            return result.ToList();
        }

        public async Task<Accounts?> GetAccount(int id)
        {
            using var conn = CreateConnection();
            const string sql = "SELECT * FROM \"Accounts\" WHERE \"AccountsId\" = @id";
            return await conn.QueryFirstOrDefaultAsync<Accounts>(sql, new { id });
        }

        public async Task<List<Accounts>> GetAccountsByEmployeeId(int employeeId)
        {
            using var conn = CreateConnection();
            const string sql = "SELECT * FROM \"Accounts\" WHERE \"EmployeeId\" = @EmployeeId ORDER BY \"Data\" DESC";
            var result = await conn.QueryAsync<Accounts>(sql, new { EmployeeId = employeeId });
            return result.ToList();
        }

        public async Task<bool> SaveAccount(Accounts account)
        {
            using var conn = CreateConnection();
            if (account.AccountsId == 0)
            {
                // Принудительно устанавливаем текущую дату
                account.Data = DateTime.UtcNow;
                Console.WriteLine($"Inserting account: Login={account.Login}, Data={account.Data}");

                const string sql = @"
                    INSERT INTO ""Accounts"" (""Login"", ""Pass"", ""Data"", ""IsActive"", ""ResourceId"", ""EmployeeId"")
                    VALUES (@Login, @Pass, @Data, @IsActive, @ResourceId, @EmployeeId)";
                var rows = await conn.ExecuteAsync(sql, account);
                return rows > 0;
            }
            else
            {
                const string sql = @"
                    UPDATE ""Accounts"" 
                    SET ""Login"" = @Login,
                        ""Pass"" = @Pass,
                        ""IsActive"" = @IsActive,
                        ""ResourceId"" = @ResourceId,
                        ""EmployeeId"" = @EmployeeId
                    WHERE ""AccountsId"" = @AccountsId";
                var rows = await conn.ExecuteAsync(sql, account);
                return rows > 0;
            }
        }

        public async Task<bool> DeleteAccount(int id)
        {
            using var conn = CreateConnection();
            const string sql = "DELETE FROM \"Accounts\" WHERE \"AccountsId\" = @id";
            var rows = await conn.ExecuteAsync(sql, new { id });
            return rows > 0;
        }
    }
}