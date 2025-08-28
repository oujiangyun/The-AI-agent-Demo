using Microsoft.EntityFrameworkCore;

namespace AIChat.Models
{

    public partial class ChatDbContext : DbContext
    {
        public ChatDbContext()
        {
        }

        public ChatDbContext(DbContextOptions<ChatDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<EfmigrationsLock> EfmigrationsLocks { get; set; }

        public virtual DbSet<Message> Messages { get; set; }

        public virtual DbSet<Session> Sessions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite("Data Source=chat.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EfmigrationsLock>(entity =>
            {
                entity.ToTable("__EFMigrationsLock");
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasIndex(e => e.Created, "IX_Messages_Created");
                entity.HasIndex(e => e.SessionId, "IX_Messages_SessionId");
                entity.Property(e => e.Created).HasDefaultValueSql("datetime('now')");
                entity.HasOne(d => d.Session).WithMany(p => p.Messages).HasForeignKey(d => d.SessionId);
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.Property(e => e.Created).HasDefaultValueSql("datetime('now')");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}