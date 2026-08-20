using System.Text;
using Application.Services.Implementations;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class ValuesTests
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
        public async Task GetMostRecentValues_Returns10LastValues()
        {
            var service = CreateService(out var db);

            // делаем 25 строк с шагом в одну минуту
            var text = new StringBuilder();
            text.Append("Date;ExecutionTime;Value");

            for (int i = 0; i < 25; i++)
                text.Append("\n2024-05-01T00:" + i.ToString("00") + ":00.0000Z;" + i + ";" + i);

            var csv = text.ToString();
            await service.UploadCsv("many.csv", "text/csv", MakeFile(csv), csv.Length);

            var result = await service.GetMostRecentValues("many.csv");

            Assert.True(result.IsSuccess);
            Assert.Equal(10, result.Value.Count);

            // первым идет самое позднее время, последним - самое раннее из десятки
            Assert.Equal(new DateTime(2024, 5, 1, 0, 24, 0, DateTimeKind.Utc), result.Value[0].Date);
            Assert.Equal(new DateTime(2024, 5, 1, 0, 15, 0, DateTimeKind.Utc), result.Value[9].Date);
        }

        [Fact]
        public async Task GetMostRecentValues_SortedByDateDesc()
        {
            var service = CreateService(out var db);

            var text = new StringBuilder();
            text.Append("Date;ExecutionTime;Value");

            for (int i = 0; i < 15; i++)
                text.Append("\n2024-05-01T00:" + i.ToString("00") + ":00.0000Z;" + i + ";" + i);

            var csv = text.ToString();
            await service.UploadCsv("sorted.csv", "text/csv", MakeFile(csv), csv.Length);

            var result = await service.GetMostRecentValues("sorted.csv");

            for (int i = 0; i < result.Value.Count - 1; i++)
                Assert.True(result.Value[i].Date > result.Value[i + 1].Date);
        }

        [Fact]
        public async Task GetMostRecentValues_LessThan10_ReturnsAll()
        {
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n" +
                      "2024-05-01T00:00:00.0000Z;1;1\n" +
                      "2024-05-01T00:01:00.0000Z;2;2";

            await service.UploadCsv("few.csv", "text/csv", MakeFile(csv), csv.Length);

            var result = await service.GetMostRecentValues("few.csv");

            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public async Task GetMostRecentValues_ReturnsOnlyOwnValues()
        {
            var service = CreateService(out var db);

            var mine = "Date;ExecutionTime;Value\n2024-05-01T00:00:00.0000Z;1;1";
            await service.UploadCsv("mine.csv", "text/csv", MakeFile(mine), mine.Length);

            var other = "Date;ExecutionTime;Value\n2024-05-01T00:05:00.0000Z;2;2";
            await service.UploadCsv("other.csv", "text/csv", MakeFile(other), other.Length);

            var result = await service.GetMostRecentValues("mine.csv");

            Assert.Single(result.Value);
            Assert.Equal(1, result.Value[0].Value);
        }

        [Fact]
        public async Task GetMostRecentValues_UnknownFile_Error()
        {
            var service = CreateService(out var db);

            var result = await service.GetMostRecentValues("nofile.csv");

            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task GetMostRecentValues_EmptyName_Error()
        {
            var service = CreateService(out var db);

            var result = await service.GetMostRecentValues("");

            Assert.True(result.IsFailed);
        }
    }
}
