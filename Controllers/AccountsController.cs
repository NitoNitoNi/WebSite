using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models.DataModel;
using MyMvcApp.Models.Interfaces;
using MyMvcApp.Models.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace MyMvcApp.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly IDBAccount accountProvider;
        private readonly IDBEmployee employeeProvider;
        private readonly IBDResources resourceProvider;
        private readonly IAuditService auditService;
        public AccountsController(IDBAccount accountProvider, IDBEmployee employeeProvider, IBDResources resourceProvider, IAuditService auditService)
        {
            this.accountProvider = accountProvider;
            this.employeeProvider = employeeProvider;
            this.resourceProvider = resourceProvider;
            this.auditService = auditService;
        }

        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 50;
            var accounts = await accountProvider.GetAccounts();

            if (!string.IsNullOrEmpty(searchString))
            {
                accounts = accounts.Where(a =>
                    (a.Login != null && a.Login.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    (a.EmployeeName != null && a.EmployeeName.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ||
                    (a.ResourceName != null && a.ResourceName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            accounts = accounts.OrderByDescending(a => a.Data).ToList();

            int totalItems = accounts.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var items = accounts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, string? returnUrl = null)
        {
            var account = await accountProvider.GetAccount(id);
            if (account == null) return NotFound();
            ViewBag.ReturnUrl = returnUrl;
            await LoadViewBags();
            return View(account);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(Accounts account, string? returnUrl = null)
        {    
            ModelState.Remove("EmployeeName");
            ModelState.Remove("ResourceName");
            
            if (account.AccountsId == 0)
                account.Data = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                var result = await accountProvider.SaveAccount(account);
                Console.WriteLine($"Результат сохранения: {result}");
                if (result)
                {
                    bool isNew = account.AccountsId == 0;
                    string action = isNew ? "Создание" : "Редактирование";
                    string employeeName = "";
                    if (account.EmployeeId > 0)
                    {
                        var emp = await employeeProvider.GetEmployee(account.EmployeeId);
                        employeeName = emp?.Name ?? "неизвестен";
                    }
                    await auditService.LogAsync(action, "Аккаунты", account.AccountsId, $"Аккаунт пользователя {employeeName}");

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index");                   
                }
                else
                    ModelState.AddModelError("", "Ошибка при сохранении аккаунта");
            }
            else
            {
                // Вывод ошибок валидации
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine($"Ошибка: {error.ErrorMessage}");
            }
            
            ViewBag.Employees = await employeeProvider.GetEmployees();
            ViewBag.Resources = await resourceProvider.GetResources();
            ViewBag.ReturnUrl = returnUrl; 
            return View("Edit", account);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? employeeId, string? returnUrl = null)
        {
            var account = new Accounts();
            if (employeeId.HasValue) account.EmployeeId = employeeId.Value;
            ViewBag.ReturnUrl = returnUrl;
            await LoadViewBags();
            return View("Edit", account);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id, string? returnUrl = null)
        {
            var account = await accountProvider.GetAccount(id);
            string employeeName = "";
            if (account != null && account.EmployeeId > 0)
            {
                var emp = await employeeProvider.GetEmployee(account.EmployeeId);
                employeeName = emp?.Name ?? "неизвестен";
            }
            await auditService.LogAsync("Удаление", "Аккаунта", id, $"Удалён аккаунт пользователя {employeeName}");
            await accountProvider.DeleteAccount(id);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> GetPassword(int id)
        {
            var account = await accountProvider.GetAccount(id);
            if (account == null) return NotFound();
            
            return Ok(new { password = account.Pass });
        }


        private async Task LoadViewBags()
        {
            ViewBag.Employees = await employeeProvider.GetEmployees();
            ViewBag.Resources = await resourceProvider.GetResources();
        }
    }
}