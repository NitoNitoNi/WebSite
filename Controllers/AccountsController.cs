using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models.DataModel;
using MyMvcApp.Models.Interfaces;
using MyMvcApp.Models.Helpers;

namespace MyMvcApp.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IDBAccount accountProvider;
        private readonly IDBEmployee employeeProvider;
        private readonly IBDResources resourceProvider;

        public AccountsController(IDBAccount accountProvider, IDBEmployee employeeProvider, IBDResources resourceProvider)
        {
            this.accountProvider = accountProvider;
            this.employeeProvider = employeeProvider;
            this.resourceProvider = resourceProvider;
        }

        public async Task<IActionResult> Index()
        {
            var accounts = await accountProvider.GetAccounts();
            return View(accounts);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var account = await accountProvider.GetAccount(id);
            if (account == null) return NotFound();

            ViewBag.Employees = await employeeProvider.GetEmployees();
            ViewBag.Resources = await resourceProvider.GetResources();
            return View(account);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(Accounts account)
        {
            //if (!string.IsNullOrEmpty(account.Pass))
                //account.Pass = EncryptionHelper.Encrypt(account.Pass);
            // Диагностика
            Console.WriteLine($"Сохранение аккаунта: Id={account.AccountsId}, Login={account.Login}, EmpId={account.EmployeeId}, ResId={account.ResourceId}");
            
            ModelState.Remove("EmployeeName");
            ModelState.Remove("ResourceName");
            
            if (account.AccountsId == 0)
                account.Data = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                var result = await accountProvider.SaveAccount(account);
                Console.WriteLine($"Результат сохранения: {result}");
                if (result)
                    return RedirectToAction("Profile", "Employee", new { id = account.EmployeeId });
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
            return View("Edit", account);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? employeeId)
        {
            var account = new Accounts();
            if (employeeId.HasValue)
                account.EmployeeId = employeeId.Value;

            ViewBag.Employees = await employeeProvider.GetEmployees();
            ViewBag.Resources = await resourceProvider.GetResources();
            return View("Edit", account);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            await accountProvider.DeleteAccount(id);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> GetPassword(int id)
        {
            var account = await accountProvider.GetAccount(id);
            if (account == null) return NotFound();
            
            return Ok(new { password = account.Pass });
        }
    }
}