using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WallpaperPortal.Models
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public DbSet<File> Files { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
