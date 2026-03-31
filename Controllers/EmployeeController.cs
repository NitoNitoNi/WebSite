using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models.Interfaces; 
using MyMvcApp.Models.DataModel;

namespace MyMvcApp.Controllers
{
    public class EmployeeController : Controller
    {
        IDBEmployee employeeProvider;
        IDBDeparment deparmentProvider;

        public EmployeeController(IDBEmployee employeeProvider, IDBDeparment deparmentProvider)
        {
            this.employeeProvider = employeeProvider;
            this.deparmentProvider = deparmentProvider;
        }

        public async Task<IActionResult> Index()
        {
            var result = await employeeProvider.GetEmployees();
            result = result.OrderBy(d=>d.Name).ToList();
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            Console.WriteLine($"=== Метод Edit вызван с id={id} ===");
            
            var result = await employeeProvider.GetEmployee(id);
            
            if (result == null)
            {
                Console.WriteLine($"Сотрудник с id={id} не найден");
                // Если сотрудник не найден, создаем нового
                result = new Employee();
            }
            
            var departments = await deparmentProvider.GetDeparments();
            ViewBag.Departments = departments ?? new List<Deparment>();
            
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(Employee employee)
        {
            // Удаляем ошибку валидации для свойства Deparment, если она есть
            ModelState.Remove("Deparment");
            
            var departments = await deparmentProvider.GetDeparments();
            ViewBag.Departments = departments;
            
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(employee.Name) || string.IsNullOrEmpty(employee.Position))
                {
                    ModelState.AddModelError("", "Не все обязательные поля заполнены");
                    return View("Edit", employee);
                }
                
                var result = await employeeProvider.SaveEmployee(employee);
                
                if (result)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Ошибка при сохранении сотрудника");
                    return View("Edit", employee);
                }
            }
            
            return View("Edit", employee);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var departments = await deparmentProvider.GetDeparments(); // Добавьте это
            ViewBag.Departments = departments;
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

            return RedirectToAction("Index");
        }
    }
}