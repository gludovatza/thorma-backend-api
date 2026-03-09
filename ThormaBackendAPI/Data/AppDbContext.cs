using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using ThormaBackendAPI.Models;

namespace ThormaBackendAPI.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null) 
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Festo> Festok { get; set; }
        public DbSet<Kep> Kepek { get; set; }
        public DbSet<Log> Logs { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditEntries = CreateAuditLogs();
            var result = await base.SaveChangesAsync(cancellationToken);

            // Audit log-ok mentése (külön SaveChanges, hogy ne legyen végtelen ciklus)
            if (auditEntries.Any())
            {
                Logs.AddRange(auditEntries);
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        private List<Log> CreateAuditLogs()
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            var userId = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = httpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous";
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();
            var path = httpContext?.Request.Path.Value ?? "";
            var method = httpContext?.Request.Method ?? "";

            var auditLogs = new List<Log>();

            foreach (var entry in ChangeTracker.Entries())
            {
                // Kihagyjuk a Log entitást (végtelen ciklus elkerülése)
                if (entry.Entity is Log || entry.State == EntityState.Unchanged || entry.State == EntityState.Detached)
                    continue;

                var entityType = entry.Entity.GetType().Name;
                var entityId = GetEntityId(entry);
                var action = entry.State.ToString();

                string? oldValue = null;
                string? newValue = null;

                if (entry.State == EntityState.Added)
                {
                    newValue = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
                }
                else if (entry.State == EntityState.Deleted)
                {
                    oldValue = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                }
                else if (entry.State == EntityState.Modified)
                {
                    var modifiedProperties = entry.Properties
                        .Where(p => p.IsModified)
                        .ToDictionary(p => p.Metadata.Name, p => new { Old = p.OriginalValue, New = p.CurrentValue });

                    oldValue = JsonSerializer.Serialize(modifiedProperties.ToDictionary(x => x.Key, x => x.Value.Old));
                    newValue = JsonSerializer.Serialize(modifiedProperties.ToDictionary(x => x.Key, x => x.Value.New));
                }

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
                    OldValue = oldValue,
                    NewValue = newValue
                };

                auditLogs.Add(log);
            }

            return auditLogs;
        }

        private string GetEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            var keyProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            return keyProperty?.CurrentValue?.ToString() ?? "Unknown";
        }
    }
}
