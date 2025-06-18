using Common.Data;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data
{
    public class UserDbContext :BaseDbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreatingPartial (ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users"); // Tùy chỉnh nếu cần
        }
    }
}
