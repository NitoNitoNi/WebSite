using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Models.DataModel;
using MyMvcApp.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace MyMvcApp.Controllers;
[Authorize]
public class HomeController : Controller
{
    private readonly IDBEmployee _employeeProvider;
    private readonly IDBDeparment _deparmentProvider;
    private readonly IBDResources _resourceProvider;
    private readonly IDBAccount _accountProvider;

    public HomeController(
        IDBEmployee employeeProvider,
        IDBDeparment deparmentProvider,
        IBDResources resourceProvider,
        IDBAccount accountProvider)
    {
        _employeeProvider = employeeProvider;
        _deparmentProvider = deparmentProvider;
        _resourceProvider = resourceProvider;
        _accountProvider = accountProvider;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _employeeProvider.GetEmployees();
        var departments = await _deparmentProvider.GetDeparments();
        var resources = await _resourceProvider.GetResources();
        var accounts = await _accountProvider.GetAccounts();

        ViewBag.EmployeeCount = employees.Count;
        ViewBag.DepartmentCount = departments.Count;
        ViewBag.ResourceCount = resources.Count;
        ViewBag.AccountCount = accounts.Count;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize]
    public IActionResult Instructions()
    {
        return View();
    }
}