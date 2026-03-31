using MyMvcApp.Models.DataModel;

namespace MyMvcApp.Models.Interfaces
{
    public interface IBDResources
    {
        Task<List<Resources>> GetResources();
        Task<Resources> GetResources(int id);
        Task<bool> SaveResources(Resources resources);
        Task<bool> DeleteResources(int id);
    }
}