using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models.Interfaces; 
using MyMvcApp.Models.DataModel;
using Humanizer;

namespace MyMvcApp.Controllers
{
    public class EmployeeController : Controller
    {
        IDBEmployee employeeProvider;

        public EmployeeController(IDBEmployee employeeProvider)
        {
            this.employeeProvider = employeeProvider;
        }

        public async Task<IActionResult> ListEmployee()
        {
            var result = await employeeProvider.GetEmployees();
            result = result.OrderBy(d=>d.emp_name).ToList();
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await employeeProvider.GetEmployee(id);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(Employee employee)
        {
            if (ModelState.IsValid)
            {
                // Проверка на null для обязательных полей emp_id, emp_name, position, description, IsActive, dep_id
                if (string.IsNullOrEmpty(employee.emp_name)|| string.IsNullOrEmpty(employee.position))
                {
                    ModelState.AddModelError("", "Не все обязательные поля заполнены");
                    return View("Edit",employee);
                }
                await employeeProvider.SaveEmployee(employee);
                return RedirectToAction("ListEmployee");
            }
            return View("Edit",employee);
        }

        [HttpGet]
        public IActionResult Create()
        {
                return View("Edit", new Employee());    
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await employeeProvider.DeleteEmployee(id);
                TempData["SuccessMessage"]="Сотрудник успешно удален";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"]=ex.Message;           
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"]="Произошла ошибка при удалении:" + ex.Message;
            }

            return RedirectToAction("ListEmployee");
        }
    }
}