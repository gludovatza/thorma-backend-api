using System.Security.Claims;
using ThormaBackendAPI.Data;
using ThormaBackendAPI.Models;

namespace ThormaBackendAPI.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            var path = context.Request.Path.Value ?? "";

            // Kizárjuk a swagger és health endpoint-okat
            if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var method = context.Request.Method;
            var timestamp = DateTime.UtcNow;

            // User információk (ha be van jelentkezve)
            string? userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string? userEmail = context.User?.FindFirst(ClaimTypes.Email)?.Value;

            // IP cím és User Agent
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.Request.Headers["User-Agent"].ToString();

            // Entity és Action felismerés az URL-ből
            var (entityType, entityId, action) = ParsePathInfo(method, path);

            // Response elkapása
            await _next(context);

            var statusCode = context.Response.StatusCode;

            // Log szint és üzenet meghatározása
            var (logLevel, message, isAuthFailure) = DetermineLogDetails(method, path, statusCode, userEmail);

            // Konzolra írás
            _logger.Log(GetLogLevel(logLevel), "{Method} {Path} - {StatusCode} - User: {UserEmail} - IP: {IpAddress}", 
                method, path, statusCode, userEmail ?? "Anonymous", ipAddress);

            // Adatbázisba írás
            var logEntry = new Log
            {
                Timestamp = timestamp,
                UserId = userId,
                UserEmail = userEmail,
                HttpMethod = method,
                Path = path,
                StatusCode = statusCode,
                Message = message,
                LogLevel = logLevel,
                IsAuthFailure = isAuthFailure,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                EntityType = entityType,
                EntityId = entityId,
                Action = action
            };

            dbContext.Logs.Add(logEntry);
            await dbContext.SaveChangesAsync();
        }

        private (string logLevel, string message, bool isAuthFailure) DetermineLogDetails(
            string method, string path, int statusCode, string? userEmail)
        {
            var user = userEmail ?? "Anonymous";

            return statusCode switch
            {
                401 => ("Warning", $"Unauthorized access attempt by {user} to {method} {path}", true),
                403 => ("Warning", $"Forbidden access attempt by {user} to {method} {path}", true),
                >= 400 and < 500 => ("Warning", $"{user} - {method} {path} failed with {statusCode}", false),
                >= 500 => ("Error", $"Server error: {method} {path} - {statusCode}", false),
                _ => ("Information", $"{user} - {method} {path} - {statusCode}", false)
            };
        }

        private (string? entityType, string? entityId, string? action) ParsePathInfo(string method, string path)
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            string? entityType = null;
            string? entityId = null;
            string? action = null;

            // /api/festok/5 -> EntityType: Festo, EntityId: 5
            // /api/kepek/LT001 -> EntityType: Kep, EntityId: LT001
            // /api/auth/login -> EntityType: Auth, Action: Login

            if (parts.Length >= 2 && parts[0].Equals("api", StringComparison.OrdinalIgnoreCase))
            {
                entityType = parts[1] switch
                {
                    "festok" => "Festo",
                    "kepek" => "Kep",
                    "auth" => "Auth",
                    "feladatok" => "Feladat",
                    _ => parts[1]
                };

                if (parts.Length >= 3)
                {
                    // Ha szám vagy kód (entityId) vagy action név
                    if (int.TryParse(parts[2], out _) || parts[2].Length <= 20)
                    {
                        entityId = parts[2];
                    }
                    else
                    {
                        action = parts[2];
                    }
                }

                // Action meghatározása HTTP method alapján
                action ??= method switch
                {
                    "GET" => entityId != null ? "View" : "List",
                    "POST" => entityType == "Auth" ? "Login/Register" : "Create",
                    "PUT" => "Update",
                    "DELETE" => "Delete",
                    _ => method
                };
            }

            return (entityType, entityId, action);
        }

        private LogLevel GetLogLevel(string level) => level switch
        {
            "Error" => LogLevel.Error,
            "Warning" => LogLevel.Warning,
            _ => LogLevel.Information
        };
    }
}
