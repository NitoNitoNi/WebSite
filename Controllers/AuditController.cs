using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Data;
using MyMvcApp.Models.DataModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyMvcApp.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search, DateTime? startDate, DateTime? endDate, int page = 1)
        {
            int pageSize = 50;
            var logs = _context.AuditLogs.AsNoTracking();

            // Фильтр по тексту
            if (!string.IsNullOrEmpty(search))
            {
                logs = logs.Where(l => l.UserName.Contains(search) ||
                                    l.Action.Contains(search) ||
                                    l.EntityType.Contains(search) ||
                                    (l.Details != null && l.Details.Contains(search)));
            }

            // Преобразуем даты в UTC
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                logs = logs.Where(l => l.Timestamp >= start);
            }

            if (endDate.HasValue)
            {
                // Добавляем один день и преобразуем в UTC
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                logs = logs.Where(l => l.Timestamp < end);
            }

            logs = logs.OrderByDescending(l => l.Timestamp);

            var totalItems = await logs.CountAsync();
            var items = await logs.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(items);
        }
    }
}