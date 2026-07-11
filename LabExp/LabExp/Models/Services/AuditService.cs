using LabExp.Data;
using LabExp.Models.Entities;
using LabExp.Models.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace LabExp.Models.Services
{

    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(
            string action,
            string entityName,
            Guid entityId)
        {
            var userName =
                _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
                ?? "Unknown";

            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                TimeStamp = DateTime.Now
            };

            _context.AuditLogs.Add(log);

            await _context.SaveChangesAsync();
        }
    }
}
