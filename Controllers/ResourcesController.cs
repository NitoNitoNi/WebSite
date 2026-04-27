using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models.DataModel;
using MyMvcApp.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace MyMvcApp.Controllers
{
    [Authorize]
    public class ResourcesController : Controller
    {
        private readonly IBDResources _resourcesProvider;
        private readonly IAuditService _auditService;

        public ResourcesController(IBDResources resourcesProvider, IAuditService auditService)
        {
            _resourcesProvider = resourcesProvider;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 50;
            var resources = await _resourcesProvider.GetResources();

            // Фильтрация по названию
            if (!string.IsNullOrEmpty(searchString))
            {
                resources = resources.Where(r => r.Name != null && r.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Сортировка
            resources = resources.OrderBy(r => r.Name).ToList();

            // Пагинация
            int totalItems = resources.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var items = resources.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _resourcesProvider.GetResources(id);
            if (result == null)
            {
                return NotFound();
            }
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(Resources resources)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(resources.Name))
                {
                    ModelState.AddModelError("", "Не все обязательные поля заполнены");
                    return View("Edit", resources);
                }
                bool isNew = resources.ResourcesId == 0;
                string action = isNew ? "Создание" : "Редактирование";
                await _auditService.LogAsync(action, "Ресурсы", resources.ResourcesId, $"Ресурс {resources.Name}");

                await _resourcesProvider.SaveResources(resources);
                return RedirectToAction("Index");
            }
            return View("Edit", resources);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("Edit", new Resources());
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var emp = await _resourcesProvider.GetResources(id);
                await _resourcesProvider.DeleteResources(id);
                TempData["SuccessMessage"] = "Ресурс успешно удален";
                await _auditService.LogAsync("Удаление", "Ресурсы", id, $"Удалён ресурс {emp?.Name}");

            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Произошла ошибка при удалении: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}