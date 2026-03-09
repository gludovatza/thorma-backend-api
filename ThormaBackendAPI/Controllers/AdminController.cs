using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThormaBackendAPI.Data;
using ThormaBackendAPI.DTOs;

namespace ThormaBackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;

        public AdminController(UserManager<IdentityUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: api/admin/users - Összes felhasználó + utolsó aktivitás
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserActivityDto>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userActivities = new List<UserActivityDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                // Utolsó aktivitás a logs táblából
                var lastLog = await _context.Logs
                    .Where(l => l.UserId == user.Id)
                    .OrderByDescending(l => l.Timestamp)
                    .FirstOrDefaultAsync();

                // Összes akció száma
                var totalActions = await _context.Logs
                    .CountAsync(l => l.UserId == user.Id);

                // Sikertelen login kísérletek
                var failedLogins = await _context.Logs
                    .CountAsync(l => l.UserId == user.Id && l.IsAuthFailure);

                userActivities.Add(new UserActivityDto
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    Roles = roles.ToList(),
                    LastActivity = lastLog?.Timestamp,
                    LastActivityDescription = lastLog != null 
                        ? $"{lastLog.HttpMethod} {lastLog.Path}" 
                        : "Még nincs aktivitás",
                    TotalActions = totalActions,
                    FailedLoginAttempts = failedLogins
                });
            }

            return Ok(userActivities.OrderByDescending(u => u.LastActivity));
        }

        // GET: api/admin/users/{id} - Egy felhasználó részletes története
        [HttpGet("users/{id}")]
        public async Task<ActionResult<object>> GetUserDetails(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Felhasználó nem található" });

            var roles = await _userManager.GetRolesAsync(user);

            var logs = await _context.Logs
                .Where(l => l.UserId == id)
                .OrderByDescending(l => l.Timestamp)
                .Take(100)
                .Select(l => new LogDto
                {
                    Id = l.Id,
                    Timestamp = l.Timestamp,
                    UserEmail = l.UserEmail,
                    HttpMethod = l.HttpMethod,
                    Path = l.Path,
                    StatusCode = l.StatusCode,
                    Message = l.Message,
                    LogLevel = l.LogLevel,
                    IsAuthFailure = l.IsAuthFailure,
                    IpAddress = l.IpAddress,
                    EntityType = l.EntityType,
                    EntityId = l.EntityId,
                    Action = l.Action
                })
                .ToListAsync();

            var totalActions = await _context.Logs.CountAsync(l => l.UserId == id);
            var failedLogins = await _context.Logs.CountAsync(l => l.UserId == id && l.IsAuthFailure);

            return Ok(new
            {
                user = new
                {
                    userId = user.Id,
                    email = user.Email,
                    userName = user.UserName,
                    roles
                },
                stats = new
                {
                    totalActions,
                    failedLogins,
                    lastActivity = logs.FirstOrDefault()?.Timestamp
                },
                recentLogs = logs
            });
        }

        // GET: api/admin/logs - Összes log (szűrhető, lapozható)
        [HttpGet("logs")]
        public async Task<ActionResult<object>> GetLogs(
            [FromQuery] string? userEmail = null,
            [FromQuery] string? entityType = null,
            [FromQuery] bool? isAuthFailure = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var query = _context.Logs.AsQueryable();

            // Szűrők
            if (!string.IsNullOrEmpty(userEmail))
                query = query.Where(l => l.UserEmail != null && l.UserEmail.Contains(userEmail));

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(l => l.EntityType == entityType);

            if (isAuthFailure.HasValue)
                query = query.Where(l => l.IsAuthFailure == isAuthFailure.Value);

            var totalCount = await query.CountAsync();

            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LogDto
                {
                    Id = l.Id,
                    Timestamp = l.Timestamp,
                    UserEmail = l.UserEmail,
                    HttpMethod = l.HttpMethod,
                    Path = l.Path,
                    StatusCode = l.StatusCode,
                    Message = l.Message,
                    LogLevel = l.LogLevel,
                    IsAuthFailure = l.IsAuthFailure,
                    IpAddress = l.IpAddress,
                    EntityType = l.EntityType,
                    EntityId = l.EntityId,
                    Action = l.Action
                })
                .ToListAsync();

            return Ok(new
            {
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                logs
            });
        }

        // GET: api/admin/stats - Statisztikák
        [HttpGet("stats")]
        public async Task<ActionResult<AdminStatsDto>> GetStats()
        {
            var totalUsers = await _userManager.Users.CountAsync();

            var today = DateTime.UtcNow.Date;

            var activeUsersToday = await _context.Logs
                .Where(l => l.Timestamp >= today && l.UserId != null)
                .Select(l => l.UserId)
                .Distinct()
                .CountAsync();

            var totalLogs = await _context.Logs.CountAsync();

            var failedLoginsToday = await _context.Logs
                .CountAsync(l => l.IsAuthFailure && l.Timestamp >= today);

            var topActions = await _context.Logs
                .Where(l => l.EntityType != null && l.Action != null)
                .GroupBy(l => new { l.EntityType, l.Action })
                .Select(g => new EntityActionCount
                {
                    EntityType = g.Key.EntityType ?? "",
                    Action = g.Key.Action ?? "",
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            return Ok(new AdminStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsersToday = activeUsersToday,
                TotalLogs = totalLogs,
                FailedLoginAttemptsToday = failedLoginsToday,
                TopActions = topActions
            });
        }

        // GET: api/admin/logs/failed-logins - Sikertelen bejelentkezések
        [HttpGet("logs/failed-logins")]
        public async Task<ActionResult<IEnumerable<LogDto>>> GetFailedLogins([FromQuery] int days = 7)
        {
            var since = DateTime.UtcNow.AddDays(-days);

            var failedLogins = await _context.Logs
                .Where(l => l.IsAuthFailure && l.Timestamp >= since)
                .OrderByDescending(l => l.Timestamp)
                .Select(l => new LogDto
                {
                    Id = l.Id,
                    Timestamp = l.Timestamp,
                    UserEmail = l.UserEmail,
                    HttpMethod = l.HttpMethod,
                    Path = l.Path,
                    StatusCode = l.StatusCode,
                    Message = l.Message,
                    LogLevel = l.LogLevel,
                    IsAuthFailure = l.IsAuthFailure,
                    IpAddress = l.IpAddress,
                    EntityType = l.EntityType,
                    EntityId = l.EntityId,
                    Action = l.Action
                })
                .ToListAsync();

            return Ok(failedLogins);
        }

        // GET: api/admin/logs/by-ip - IP címenkénti statisztika
        [HttpGet("logs/by-ip")]
        public async Task<ActionResult<object>> GetLogsByIp([FromQuery] int days = 7)
        {
            var since = DateTime.UtcNow.AddDays(-days);

            var ipStats = await _context.Logs
                .Where(l => l.IpAddress != null && l.Timestamp >= since)
                .GroupBy(l => l.IpAddress)
                .Select(g => new
                {
                    ipAddress = g.Key,
                    totalRequests = g.Count(),
                    failedLogins = g.Count(l => l.IsAuthFailure),
                    uniqueUsers = g.Where(l => l.UserEmail != null).Select(l => l.UserEmail).Distinct().Count(),
                    lastActivity = g.Max(l => l.Timestamp)
                })
                .OrderByDescending(x => x.totalRequests)
                .ToListAsync();

            return Ok(ipStats);
        }
    }
}
