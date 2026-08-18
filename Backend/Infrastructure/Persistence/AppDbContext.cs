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

                // поля, по которым идёт фильтрация диапазонами в поиске
                entity.HasIndex(fileResult => fileResult.FirstExecutionTime);
                entity.HasIndex(fileResult => fileResult.AverageValue);
                entity.HasIndex(fileResult => fileResult.AverageExcecutionTime);

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
                // выборка последних значений одного файла: фильтр по FileResultId + сортировка по Date
                entity.HasIndex(value => new { value.FileResultId, value.Date })
                    .IsDescending(false, true);
            });
        }
    }
}
