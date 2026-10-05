using Rooms.Api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Rooms.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<Room> Rooms { get; set; }
    }
}
