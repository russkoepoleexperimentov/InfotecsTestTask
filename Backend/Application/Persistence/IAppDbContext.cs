using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Persistence
{
    public interface IAppDbContext
    {
        DbSet<ValueRecord> Values { get; }
        DbSet<FileResult> Results { get; }

        DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
