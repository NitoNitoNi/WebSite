using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyMvcApp.Data;
using MyMvcApp.Models.DataModel;
using MyMvcApp.Models.Interfaces;
using System;
using System.Threading.Tasks;

namespace MyMvcApp.Models.Implementation
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task LogAsync(string action, string entityType, int? entityId = null, string? details = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userId = _userManager.GetUserId(httpContext?.User);
            var userName = httpContext?.User?.Identity?.Name;

            var log = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                Timestamp = DateTime.UtcNow,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString()
            };

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}