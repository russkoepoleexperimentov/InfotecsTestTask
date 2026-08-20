using System.Text;
using Application.Dtos;
using Application.Services.Implementations;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class SearchTests
    {
        private SqliteConnection _connection = null!;

        private CsvAnalysisService CreateService(out AppDbContext db)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            db = new AppDbContext(options);
            db.Database.EnsureCreated();

            return new CsvAnalysisService(db);
        }

        private Stream MakeFile(string text)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(text));
        }

        private async Task AddFiles(CsvAnalysisService service)
        {
            var alpha = "Date;ExecutionTime;Value\n2024-01-01T00:00:00.0000Z;10;10";
            await service.UploadCsv("alpha.csv", "text/csv", MakeFile(alpha), alpha.Length);

            var beta = "Date;ExecutionTime;Value\n2024-02-01T00:00:00.0000Z;20;20";
            await service.UploadCsv("beta.csv", "text/csv", MakeFile(beta), beta.Length);

            var gamma = "Date;ExecutionTime;Value\n2024-03-01T00:00:00.0000Z;30;30";
            await service.UploadCsv("gamma.csv", "text/csv", MakeFile(gamma), gamma.Length);
        }

        [Fact]
        public async Task SearchResults_WithoutFilters_ReturnsAll()
        {
            var service = CreateService(out var db);
            await AddFiles(service);

            var result = await service.SearchResults(new ResultSearchDto());

            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Value.TotalCount);
            Assert.Equal(3, result.Value.Values.Count);
        }

        [Fact]
        public async Task SearchResults_ByName()
        {
            var service = CreateService(out var db);
            await AddFiles(service);

            var result = await service.SearchResults(new ResultSearchDto { NameQuery = "bet" });

            Assert.Single(result.Value.Values);
            Assert.Equal("beta.csv", result.Value.Values[0].FileName);
        }

        [Fact]
        public async Task SearchResults_ByNameInUpperCase()
        {
            var service = CreateService(out var db);
            await AddFiles(service);

            var result = await service.SearchResults(new ResultSearchDto { NameQuery = "GAMMA" });

            Assert.Single(result.Value.Values);
            Assert.Equal("gamma.csv", result.Value.Values[0].FileName);
        }

        [Fact]
        public async Task SearchResults_ByNameNotFound_ReturnsEmpty()
        {
            var service = CreateService(out var db);
            await AddFiles(service);

            var result = await service.SearchResults(new ResultSearchDto { NameQuery = "qwerty" });

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value.Values);
            Assert.Equal(0, result.Value.TotalCount);
        }

        [Fact]
        public async Task SearchResults_ByDateRange()
        {
            var service = CreateService(out var db);
            await AddFiles(service);

            var result = await service.SearchResults(new ResultSearchDto
            {
                FirstExecutionRange = new RangeDto<DateTime>
                {
                    Min = new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                    Max = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            });

            Assert.Equal(2, result.Value.Values.Count);
            Assert.Equal("beta.csv", result.Value.Values[0].FileName);
            Assert.Equal("gamma.csv", result.Value.Values[1].FileName);
        }

        [Fact]
        public async Task SearchResults_ByAverageValueRange()
        {
            var service = CreateService(out var db);
            await AddFiles(service);

            var result = await service.SearchResults(new ResultSearchDto
            {
                AverageValueRange = new RangeDto<float> { Min = 10, Max = 20 }
            });

            Assert.Equal(2, result.Value.Values.Count);
            Assert.Equal("alpha.csv", result.Value.Values[0].FileName);
            Assert.Equal("beta.csv", result.Value.Values[1].FileName);
        }

        [Fact]
        public async Task SearchResults_ByAverageTimeRange_OnlyMin()
        {
            var service = CreateService(out var db);
            await AddFiles(service);

            var result = await service.SearchResults(new ResultSearchDto
            {
                AverageExcecutionTimeRange = new RangeDto<int> { Min = 25 }
            });

            Assert.Single(result.Value.Values);
            Assert.Equal("gamma.csv", result.Value.Values[0].FileName);
        }

        [Fact]
        public async Task SearchResults_ManyFiltersTogether()
        {
            var service = CreateService(out var db);
            await AddFiles(service);

            // буква a есть во всех, среднее значение от 15 это beta и gamma,
            // среднее время до 25 это alpha и beta, значит остается beta
            var result = await service.SearchResults(new ResultSearchDto
            {
                NameQuery = "a",
                AverageValueRange = new RangeDto<float> { Min = 15 },
                AverageExcecutionTimeRange = new RangeDto<int> { Max = 25 }
            });

            Assert.Single(result.Value.Values);
            Assert.Equal("beta.csv", result.Value.Values[0].FileName);
        }

        [Fact]
        public async Task SearchResults_Paging()
        {
            var service = CreateService(out var db);
            await AddFiles(service);

            var result = await service.SearchResults(new ResultSearchDto { Skip = 1, Take = 1 });

            Assert.Single(result.Value.Values);
            Assert.Equal("beta.csv", result.Value.Values[0].FileName);
            Assert.Equal(1, result.Value.Skipped);
            Assert.Equal(1, result.Value.Taken);
            Assert.Equal(3, result.Value.TotalCount);
        }

        [Fact]
        public async Task SearchResults_NegativeSkip_Error()
        {
            var service = CreateService(out var db);

            var result = await service.SearchResults(new ResultSearchDto { Skip = -1 });

            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task SearchResults_ZeroTake_Error()
        {
            var service = CreateService(out var db);

            var result = await service.SearchResults(new ResultSearchDto { Take = 0 });

            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task SearchResults_TooBigTake_Error()
        {
            var service = CreateService(out var db);

            var result = await service.SearchResults(new ResultSearchDto { Take = 101 });

            Assert.True(result.IsFailed);
        }
    }
}
