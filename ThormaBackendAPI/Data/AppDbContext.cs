using Microsoft.EntityFrameworkCore;
using ThormaBackendAPI.Models;

namespace ThormaBackendAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Festo> Festok { get; set; }
        public DbSet<Kep> Kepek { get; set; }
    }
}
