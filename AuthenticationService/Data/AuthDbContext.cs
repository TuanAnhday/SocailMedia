using Microsoft.AspNetCore.Authentication;
using Common.Data;
using Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Data
{
    public class AuthDbContext : BaseDbContext
    {
        public AuthDbContext (DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> AuthUsers { get; set; }

        protected override void OnModelCreatingPartial (ModelBuilder modelBuilder)
        {
        }
    }
}
