using Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace Common.Data
{
    public abstract class BaseDbContext :DbContext
    {
        protected BaseDbContext (DbContextOptions options) : base(options) { }

        public override async Task<int> SaveChangesAsync (CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges ()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        private void UpdateTimestamps ()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach(var entry in entries)
            {
                var now = DateTime.UtcNow; // Sử dụng UTC để đồng bộ
                switch(entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = now;
                        entry.Entity.ModifiedDate = now;
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedDate = now;
                        break;
                }
            }
        }

        protected override void OnModelCreating (ModelBuilder modelBuilder)
        {
            // Định nghĩa khóa chính mặc định cho BaseEntity
            modelBuilder.Entity<BaseEntity>().HasKey(e => e.Id);
            // Cho phép các lớp con mở rộng
            OnModelCreatingPartial(modelBuilder);
        }

        protected virtual void OnModelCreatingPartial (ModelBuilder modelBuilder)
        {
            // Phương thức ảo để các lớp con tùy chỉnh
        }
    }
}
