using Microsoft.EntityFrameworkCore;
using API.Entities;
namespace API.Data
{
    public class AppDBContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<AppUser> Users { get; set;}
        public DbSet<Member> Members { get; set; }
        public DbSet<Photo> Photos { get; set; }

    }
}
