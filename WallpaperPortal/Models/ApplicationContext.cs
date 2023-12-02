using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WallpaperPortal.Models
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public DbSet<File> Files { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<Color> Colors { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);;

            modelBuilder.Entity<Color>()
                .HasKey(c => new { c.A, c.R, c.G, c.B });

            modelBuilder.Entity<UserLikedFile>()
               .HasKey(ulf => new { ulf.UserId, ulf.FileId });

            modelBuilder.Entity<UserLikedFile>()
                .HasOne(ulf => ulf.User)
                .WithMany(u => u.LikedFiles)
                .HasForeignKey(ulf => ulf.UserId);

            modelBuilder.Entity<UserLikedFile>()
                .HasOne(ulf => ulf.File)
                .WithMany(f => f.LikedByUsers)
                .HasForeignKey(ulf => ulf.FileId);
        }
    }
}
