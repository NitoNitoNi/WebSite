using System;
using MyMvcApp.Models.DataModel;


namespace MyMvcApp.Models.Interfaces
{
 public interface IDBEmployee
    {
        Task<List<Employee>> GetEmployees();
        Task<Employee> GetEmployee(int id);
        Task<bool> SaveEmployee(Employee employee);
        Task<bool> DeleteEmployee(int id);
    }
}