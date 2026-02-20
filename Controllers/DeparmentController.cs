using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models.Interfaces; 
using MyMvcApp.Models.DataModel;
using Humanizer;

namespace MyMvcApp.Controllers
{
    public class DeparmentController : Controller
    {
        IDBDeparment deparmentProvider;

        public DeparmentController(IDBDeparment deparmentProvider)
        {
            this.deparmentProvider = deparmentProvider;
        }

        public async Task<IActionResult> ListDeparment()
        {
            var result = await deparmentProvider.GetDeparments();
            result = result.OrderBy(d=>d.dep_name).ToList();
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await deparmentProvider.GetDeparment(id);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(Deparment deparment)
        {
            if (ModelState.IsValid)
            {
                // Проверка на null для обязательных полей
                if (string.IsNullOrEmpty(deparment.dep_name))
                {
                    ModelState.AddModelError("", "Не все обязательные поля заполнены");
                    return View("Edit",deparment);
                }
                await deparmentProvider.SaveDeparment(deparment);
                return RedirectToAction("ListDeparment");
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

            return RedirectToAction("ListDeparment");
        }
    }
}