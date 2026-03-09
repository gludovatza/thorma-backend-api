using System.Text.Json;
using ThormaBackendAPI.Data;
using ThormaBackendAPI.Models;

namespace ThormaBackendAPI.Services
{
    public class AuditLogger : IAuditLogger
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogger(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogChangeAsync<T>(string userId, string userEmail, string entityType, 
            string entityId, string action, T? oldValue, T? newValue)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();
            var path = httpContext?.Request.Path.Value ?? "";
            var method = httpContext?.Request.Method ?? "";

            var log = new Log
            {
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                UserEmail = userEmail,
                HttpMethod = method,
                Path = path,
                StatusCode = 200,
                Message = $"{action} {entityType} {entityId} by {userEmail}",
                LogLevel = "Information",
                IsAuthFailure = false,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                OldValue = oldValue != null ? JsonSerializer.Serialize(oldValue) : null,
                NewValue = newValue != null ? JsonSerializer.Serialize(newValue) : null
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
