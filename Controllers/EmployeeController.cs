using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models.Interfaces; 
using MyMvcApp.Models.DataModel;
using Microsoft.AspNetCore.Authorization;

namespace MyMvcApp.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IDBEmployee employeeProvider;
        private readonly IDBDeparment deparmentProvider;
        private readonly IDBAccount accountProvider;
        private readonly IBDResources resourcesProvider;

        private readonly IAuditService auditService;

        public EmployeeController(IDBEmployee employeeProvider, IDBDeparment deparmentProvider, IDBAccount accountProvider,IBDResources resourcesProvider, IAuditService auditService)
        {
            this.employeeProvider = employeeProvider;
            this.deparmentProvider = deparmentProvider;
            this.accountProvider = accountProvider;
            this.resourcesProvider = resourcesProvider;
            this.auditService = auditService;
        }

        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 50;
            var employees = await employeeProvider.GetEmployees();
            if (!string.IsNullOrEmpty(searchString))
                employees = employees.Where(e => e.Name != null && e.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            employees = employees.OrderBy(e => e.Name).ToList();
            int totalItems = employees.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var items = employees.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            return View(items);
        }



        /*public async Task<IActionResult> Index()
        {
            var result = await employeeProvider.GetEmployees();
            result = result.OrderBy(d=>d.Name).ToList();
            return View(result);
        }*/

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
                    bool isNew = employee.EmployeeId == 0;
                    string action = isNew ? "Создание" : "Редактирование";
                    await auditService.LogAsync(action, "Сотрудники", employee.EmployeeId, $"Сотрудник {employee.Name}, должность {employee.Position}");

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
                var emp = await employeeProvider.GetEmployee(id);
                await employeeProvider.DeleteEmployee(id);
                TempData["SuccessMessage"]="Сотрудник успешно удален";
                await auditService.LogAsync("Удаление", "Employee", id, $"Удалён сотрудник {emp?.Name}");
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

        [HttpGet]
        public async Task<IActionResult> Profile(int id)
        {
            var employee = await employeeProvider.GetEmployee(id);
            if (employee == null) return NotFound();

            // Загружаем аккаунты
            var accounts = await accountProvider.GetAccountsByEmployeeId(id);
            employee.Accounts = accounts ?? new List<Accounts>();
            var allResources = await resourcesProvider.GetResources();
            var resourceDict = allResources.ToDictionary(r => r.ResourcesId, r => r);
            foreach (var acc in employee.Accounts)
            {
                if (resourceDict.TryGetValue(acc.ResourceId, out var res))
                {
                    acc.ResourceName = res.Name;
                    acc.ResourceUrl = res.Url;
                }
                else
                {
                    acc.ResourceName = "Неизвестно";
                    acc.ResourceUrl = "#";
                }
            }
            // Загружаем отдел
            if (employee.DeparmentId > 0)
            {
                var dept = await deparmentProvider.GetDeparment(employee.DeparmentId);
                employee.Deparment = dept ?? new Deparment { Name = "Не указано" };
            }
            else
            {
                employee.Deparment = new Deparment { Name = "Не указано" };
            }

            return View(employee);
        }



    }
}