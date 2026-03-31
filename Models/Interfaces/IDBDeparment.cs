using MyMvcApp.Models.DataModel;

namespace MyMvcApp.Models.Interfaces
{
 public interface IDBDeparment
    {
        Task<List<Deparment>> GetDeparments();
        Task<Deparment> GetDeparment(int id);
        Task<bool> SaveDeparment(Deparment deparment);
        Task<bool> DeleteDeparment(int id);
    }
}