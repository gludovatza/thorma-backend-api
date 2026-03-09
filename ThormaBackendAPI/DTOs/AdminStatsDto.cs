namespace ThormaBackendAPI.DTOs
{
    public class AdminStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsersToday { get; set; }
        public int TotalLogs { get; set; }
        public int FailedLoginAttemptsToday { get; set; }
        public List<EntityActionCount> TopActions { get; set; } = new();
    }

    public class EntityActionCount
    {
        public string EntityType { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
