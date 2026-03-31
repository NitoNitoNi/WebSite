using Dapper;
using Npgsql;
using MyMvcApp.Models.DataModel;
using MyMvcApp.Models.Interfaces;

namespace MyMvcApp.Models.Implementation
{
    public class ResourcesProvider:IBDResources
    {
        private readonly string _connectionString;

        public ResourcesProvider(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string DefaultConnection not found");
        }

        private NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<List<Resources>> GetResources()
        {
            using (var connection = CreateConnection())
            {
                var result = await connection.QueryAsync<Resources>("SELECT \"ResourcesId\", \"Name\", \"Url\", \"Description\", \"IsActive\" FROM \"Resources\"");
                return result.ToList();
            }
        }

        public async Task<Resources> GetResources(int id)
        {
            using (var connection = CreateConnection())
            {
                var sql = "SELECT \"ResourcesId\", \"Name\", \"Url\", \"Description\", \"IsActive\" FROM \"Resources\" WHERE \"ResourcesId\" = @id";
                return await connection.QueryFirstOrDefaultAsync<Resources>(sql, new { id });
            }
        }

        public async Task<bool> UpdateResources(Resources resources)
        {
            using (var connection = CreateConnection())
            {
                var sql = "UPDATE \"Resources\" SET \"Name\" = @Name, \"Url\" = @Url, \"Description\" = @Description, \"IsActive\" = @IsActive WHERE \"ResourcesId\" = @ResourcesId";
                int rows = await connection.ExecuteAsync(sql,resources);
                return rows > 0;
            }
        }
        public async Task<bool> DeleteResources(int id)
        {
            using (var connection = CreateConnection())
            {
                var check = "SELECT COUNT(*) FROM \"Accounts\" WHERE \"ResourceId\" = @ResourcesId"; 
                int accountCount = await connection.ExecuteScalarAsync<int>(check,new {ResourcesId = id});

                if (accountCount > 0)
                {
                    throw new InvalidOperationException($"Невозмождно удалить отделение, так как в нем есть {accountCount} аккаунтов");
                }

                var sql = "DELETE FROM \"Resources\" WHERE \"ResourcesId\" = @ResourcesId";
                int rows = await connection.ExecuteAsync(sql,new {ResourcesId = id});
                return rows > 0;
            }
        }
        public async Task<bool> AddResources(Resources resources)
        {
            using (var connection = CreateConnection())
            {
                var sql =  "INSERT INTO \"Resources\" (\"Name\", \"Url\", \"Description\", \"IsActive\") VALUES (@Name, @Url, @Description, @IsActive)";
                int rows = await connection.ExecuteAsync(sql,resources);
                return rows > 0;
            }
        }
        public async Task<bool> SaveResources(Resources resources)
        {
            return resources.ResourcesId == 0 ? await AddResources(resources) : await UpdateResources(resources);
        }
    }
}