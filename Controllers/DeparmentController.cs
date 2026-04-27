using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models.Interfaces; 
using MyMvcApp.Models.DataModel;
using Microsoft.AspNetCore.Authorization;

namespace MyMvcApp.Controllers
{
    [Authorize]
    public class DeparmentController : Controller
    {
        private readonly IDBDeparment deparmentProvider;
        private readonly IAuditService auditService;
        public DeparmentController(IDBDeparment deparmentProvider, IAuditService auditService)
        {
            this.deparmentProvider = deparmentProvider;
            this.auditService= auditService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await deparmentProvider.GetDeparments();
            result = result.OrderBy(d=>d.Name).ToList();
            return View(result);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dept = await deparmentProvider.GetDeparment(id);
            if (dept == null) return NotFound();
            return View("Edit", dept);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 50;
            var departments = await deparmentProvider.GetDeparments();

            // Фильтрация по названию
            if (!string.IsNullOrEmpty(searchString))
            {
                departments = departments.Where(d => d.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Сортировка
            departments = departments.OrderBy(d => d.Name).ToList();

            // Пагинация
            int totalItems = departments.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var items = departments.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(Deparment deparment)
        {
            if (ModelState.IsValid)
            {
                // Проверка на null для обязательных полей
                if (string.IsNullOrEmpty(deparment.Name))
                {
                    ModelState.AddModelError("", "Не все обязательные поля заполнены");
                    return View("Edit",deparment);
                }
                bool isNew = deparment.DeparmentId == 0;
                string action = isNew ? "Создание" : "Редактирование";
                await auditService.LogAsync(action, "Отделения", deparment.DeparmentId, $"Отделение {deparment.Name}");
                await deparmentProvider.SaveDeparment(deparment);
                return RedirectToAction("Index");
            }
            return View("Edit",deparment);
        }

        [HttpGet]
        public IActionResult Create()
        {
                return View("Edit", new Deparment());    
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var emp = await deparmentProvider.GetDeparment(id);
                await auditService.LogAsync("Удаление", "Отделения", id, $"Удалёно отделение {emp?.Name}");
                await deparmentProvider.DeleteDeparment(id);
                TempData["SuccessMessage"]="Отделение успешно удалено"; 
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