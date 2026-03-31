using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models.DataModel;
using MyMvcApp.Models.Interfaces;

namespace MyMvcApp.Controllers
{
    public class ResourcesController : Controller
    {
        private readonly IBDResources _resourcesProvider;

        public ResourcesController(IBDResources resourcesProvider)
        {
            _resourcesProvider = resourcesProvider;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _resourcesProvider.GetResources();
            result = result.OrderBy(d => d.Name).ToList();
            return View(result);
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
                await _resourcesProvider.DeleteResources(id);
                TempData["SuccessMessage"] = "Ресурс успешно удален";
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