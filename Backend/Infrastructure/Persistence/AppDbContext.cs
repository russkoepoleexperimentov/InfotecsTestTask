using Application.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ValueRecord> Values => Set<ValueRecord>();
        public DbSet<FileResult> Results => Set<FileResult>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileResult>(entity =>
            {
                entity.ToTable("Results");
                entity.HasKey(fileResult => fileResult.Id);

                // имя файла — ключ перезаписи, поиск по нему идёт на каждой загрузке
                entity.HasIndex(fileResult => fileResult.FileName).IsUnique();
                entity.Property(fileResult => fileResult.FileName).HasMaxLength(255);

                entity
                    .HasMany(fileResult => fileResult.Values)
                    .WithOne(value => value.FileResult)
                    .HasForeignKey(value => value.FileResultId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ValueRecord>(entity =>
            {
                entity.ToTable("Values");
                entity.HasKey(value => value.Id);
                entity.HasIndex(value => value.Date);
            });
        }
    }
}
