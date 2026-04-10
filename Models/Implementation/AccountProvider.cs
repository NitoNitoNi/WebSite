using Dapper;
using Npgsql;
using MyMvcApp.Models.DataModel;
using MyMvcApp.Models.Interfaces;
using MyMvcApp.Models.Helpers;


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
            var account = await conn.QueryFirstOrDefaultAsync<Accounts>(sql, new { id });
            if (account != null && !string.IsNullOrEmpty(account.Pass))
            {
                Console.WriteLine($"Before decrypt: {account.Pass}");
                var decrypted = EncryptionHelper.Decrypt(account.Pass);
                Console.WriteLine($"After decrypt: {decrypted}");
                account.Pass = decrypted;
            }
            return account;
        }

        public async Task<List<Accounts>> GetAccountsByEmployeeId(int employeeId)
        {
            using var conn = CreateConnection();
            const string sql = "SELECT * FROM \"Accounts\" WHERE \"EmployeeId\" = @EmployeeId ORDER BY \"Data\" DESC";
            var accounts = await conn.QueryAsync<Accounts>(sql, new { EmployeeId = employeeId });
            var list = accounts.ToList();
            foreach (var acc in list)
            {
                if (!string.IsNullOrEmpty(acc.Pass))
                    acc.Pass = EncryptionHelper.Decrypt(acc.Pass);
            }
            return list;
        }

        public async Task<bool> SaveAccount(Accounts account)
        {
            using var conn = CreateConnection();

            if (account.AccountsId == 0)
            {
                // Новый аккаунт
                if (string.IsNullOrEmpty(account.Pass))
                    throw new ArgumentException("Пароль обязателен");
                account.Pass = EncryptionHelper.Encrypt(account.Pass);
                account.Data = DateTime.UtcNow;
                const string sql = @"
                    INSERT INTO ""Accounts"" (""Login"", ""Pass"", ""Data"", ""IsActive"", ""ResourceId"", ""EmployeeId"")
                    VALUES (@Login, @Pass, @Data, @IsActive, @ResourceId, @EmployeeId)";
                var rows = await conn.ExecuteAsync(sql, account);
                return rows > 0;
            }
            else
            {
                // Обновление
                string? finalPass;
                if (string.IsNullOrEmpty(account.Pass))
                {
                    // Пароль не меняем – берём существующий из БД
                    finalPass = await conn.QueryFirstOrDefaultAsync<string>(
                        "SELECT \"Pass\" FROM \"Accounts\" WHERE \"AccountsId\" = @id",
                        new { id = account.AccountsId });
                }
                else
                {
                    // Новый пароль – шифруем
                    finalPass = EncryptionHelper.Encrypt(account.Pass);
                }

                const string sql = @"
                    UPDATE ""Accounts"" 
                    SET ""Login"" = @Login,
                        ""Pass"" = @Pass,
                        ""IsActive"" = @IsActive,
                        ""ResourceId"" = @ResourceId,
                        ""EmployeeId"" = @EmployeeId
                    WHERE ""AccountsId"" = @AccountsId";
                var rows = await conn.ExecuteAsync(sql, new
                {
                    account.AccountsId,
                    account.Login,
                    Pass = finalPass,
                    account.IsActive,
                    account.ResourceId,
                    account.EmployeeId
                });
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