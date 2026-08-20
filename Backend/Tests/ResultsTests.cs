using System.Text;
using Application.Services.Implementations;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class ResultsTests
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

        [Fact]
        public async Task UploadCsv_CountsAllValues()
        {
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n" +
                      "2024-03-10T08:01:00.0000Z;20;10\n" +
                      "2024-03-10T08:00:00.0000Z;10;30\n" +
                      "2024-03-10T08:03:00.0000Z;30;20";

            var result = await service.UploadCsv("calc.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsSuccess);

            Assert.Equal(180, result.Value.DeltaSeconds);
            Assert.Equal(new DateTime(2024, 3, 10, 8, 0, 0, DateTimeKind.Utc), result.Value.FirstExcecutionTime);
            Assert.Equal(20, result.Value.AverageExcecutionTime);
            Assert.Equal(20, result.Value.AverageValue);
            Assert.Equal(20, result.Value.MedianValue);
            Assert.Equal(10, result.Value.MinimumValue);
            Assert.Equal(30, result.Value.MaximumValue);
        }

        [Fact]
        public async Task UploadCsv_SavesResultToDatabase()
        {
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n" +
                      "2024-03-10T08:00:00.0000Z;10;1\n" +
                      "2024-03-10T08:00:30.0000Z;30;3";

            await service.UploadCsv("saved.csv", "text/csv", MakeFile(csv), csv.Length);

            var fileResult = db.Results.AsNoTracking().First();

            Assert.Equal("saved.csv", fileResult.FileName);
            Assert.Equal(30, fileResult.DeltaSeconds);
            Assert.Equal(20, fileResult.AverageExcecutionTime);
            Assert.Equal(2, fileResult.AverageValue);
            Assert.Equal(2, fileResult.MedianValue);
            Assert.Equal(1, fileResult.MinimumValue);
            Assert.Equal(3, fileResult.MaximumValue);
        }

        [Fact]
        public async Task UploadCsv_MedianForOddCount()
        {
            var service = CreateService(out var db);

            // значения 5 1 3, медиана 3
            var csv = "Date;ExecutionTime;Value\n" +
                      "2024-03-10T08:00:00.0000Z;1;5\n" +
                      "2024-03-10T08:00:01.0000Z;1;1\n" +
                      "2024-03-10T08:00:02.0000Z;1;3";

            var result = await service.UploadCsv("odd.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.Equal(3, result.Value.MedianValue);
        }

        [Fact]
        public async Task UploadCsv_MedianForEvenCount()
        {
            var service = CreateService(out var db);

            // значения 5 1 3 9, медиана (3 + 5) / 2 = 4
            var csv = "Date;ExecutionTime;Value\n" +
                      "2024-03-10T08:00:00.0000Z;1;5\n" +
                      "2024-03-10T08:00:01.0000Z;1;1\n" +
                      "2024-03-10T08:00:02.0000Z;1;3\n" +
                      "2024-03-10T08:00:03.0000Z;1;9";

            var result = await service.UploadCsv("even.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.Equal(4, result.Value.MedianValue);
        }

        [Fact]
        public async Task UploadCsv_OneRow_DeltaIsZero()
        {
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n2024-03-10T08:00:00.0000Z;7;42";

            var result = await service.UploadCsv("one.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Value.DeltaSeconds);
            Assert.Equal(7, result.Value.AverageExcecutionTime);
            Assert.Equal(42, result.Value.AverageValue);
            Assert.Equal(42, result.Value.MedianValue);
            Assert.Equal(42, result.Value.MinimumValue);
            Assert.Equal(42, result.Value.MaximumValue);
        }

        [Fact]
        public async Task UploadCsv_ZeroValues_Ok()
        {
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n2024-03-10T08:00:00.0000Z;0;0";

            var result = await service.UploadCsv("zero.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Value.AverageExcecutionTime);
            Assert.Equal(0, result.Value.AverageValue);
        }
    }
}
