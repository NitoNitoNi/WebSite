using MyMvcApp.Models.DataModel;

namespace MyMvcApp.Models.DataModel
{
    public interface IDBAccount
    {
        Task<List<Accounts>> GetAccounts();
        Task<Accounts?> GetAccount(int id);
        Task<List<Accounts>> GetAccountsByEmployeeId(int employeeId);
        Task<bool> SaveAccount(Accounts account);
        Task<bool> DeleteAccount(int id);
    }
}