namespace fase_01.infrastructure.data
{
    using Microsoft.EntityFrameworkCore;
    using fase_01.domain.entities;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<UserPhoto> UserPhotos => Set<UserPhoto>();

        public DbSet<Game> Games => Set<Game>();
        public DbSet<GamePhoto> GamePhotos => Set<GamePhoto>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.NickName).HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Admin).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.ValidatedAt);

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Users_ValidatedAt_GreaterThan_CreatedAt",
                    "[ValidatedAt] IS NULL OR [ValidatedAt] >= [CreatedAt]"
                ));
            });

            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Manufacturer).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ReleasedAt);
                entity.Property(e => e.Description);
                entity.Property(e => e.Online).IsRequired();
                entity.Property(e => e.Multiplayer).IsRequired();
                entity.Property(e => e.CategoryId).IsRequired();
                entity.Property(e => e.UrlGame).HasMaxLength(255);
                entity.Property(e => e.UrlVideo).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Ignore(e => e.Category);
            });

            // Relacionamento User 1 <-> 0..1 UserPhoto
            modelBuilder.Entity<UserPhoto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Image).IsRequired();
                entity.Property(e => e.Thumbnail);
                entity.HasOne(e => e.User)
                      .WithOne()
                      .HasForeignKey<UserPhoto>(e => e.Id)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Relacionamento Game 1 <-> 0..1 GamePhoto
            modelBuilder.Entity<GamePhoto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Image).IsRequired();
                entity.Property(e => e.Thumbnail);
                entity.HasOne(e => e.Game)
                      .WithOne(u => u.Photo)
                      .HasForeignKey<GamePhoto>(e => e.Id)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Accounts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Approved).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.FailedCounter).IsRequired().HasDefaultValue(0);
                entity.HasOne(e => e.User)
                      .WithOne(u => u.Account)
                      .HasForeignKey<Account>(e => e.Id)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}