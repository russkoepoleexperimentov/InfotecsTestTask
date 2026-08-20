using System.Text;
using Application.Services.Implementations;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests
{
    public class UploadTests
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
        public async Task UploadCsv_GoodFile_Ok()
        {
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n" +
                      "2024-01-01T12:00:00.0000Z;10;1.5\n" +
                      "2024-01-01T12:01:00.0000Z;20;2.5";

            var result = await service.UploadCsv("good.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsSuccess);
            Assert.Equal("good.csv", result.Value.FileName);
            Assert.Equal(1, db.Results.Count());
            Assert.Equal(2, db.Values.Count());
        }

        [Fact]
        public async Task UploadCsv_BadContentType_Error()
        {
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n2024-01-01T12:00:00.0000Z;10;1.5";

            var result = await service.UploadCsv("pic.png", "image/png", MakeFile(csv), csv.Length);

            Assert.True(result.IsFailed);
            Assert.Equal(0, db.Results.Count());
        }

        [Fact]
        public async Task UploadCsv_BadHeader_Error()
        {
            var service = CreateService(out var db);

            var csv = "Date;Time;Value\n2024-01-01T12:00:00.0000Z;10;1.5";

            var result = await service.UploadCsv("bad.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsFailed);
            Assert.Equal(0, db.Results.Count());
        }

        [Fact]
        public async Task UploadCsv_NoRows_Error()
        {
            // по заданию строк должно быть минимум 1
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value";

            var result = await service.UploadCsv("empty.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsFailed);
            Assert.Equal(0, db.Results.Count());
        }

        [Fact]
        public async Task UploadCsv_TooManyRows_Error()
        {
            var service = CreateService(out var db);

            var text = new StringBuilder();
            text.Append("Date;ExecutionTime;Value");

            for (int i = 0; i < 10001; i++)
                text.Append("\n2024-01-01T12:00:00.0000Z;10;1.5");

            var csv = text.ToString();

            var result = await service.UploadCsv("big.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsFailed);
            Assert.Equal(0, db.Results.Count());
        }

        [Fact]
        public async Task UploadCsv_10000Rows_Ok()
        {
            var service = CreateService(out var db);

            var text = new StringBuilder();
            text.Append("Date;ExecutionTime;Value");

            for (int i = 0; i < 10000; i++)
                text.Append("\n2024-01-01T12:00:00.0000Z;10;1.5");

            var csv = text.ToString();

            var result = await service.UploadCsv("max.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsSuccess);
            Assert.Equal(10000, db.Values.Count());
        }

        [Fact]
        public async Task UploadCsv_DateTooOld_Error()
        {
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n1999-12-31T23:59:59.0000Z;10;1.5";

            var result = await service.UploadCsv("old.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task UploadCsv_DateFromFuture_Error()
        {
            var service = CreateService(out var db);

            var future = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss.0000") + "Z";
            var csv = "Date;ExecutionTime;Value\n" + future + ";10;1.5";

            var result = await service.UploadCsv("future.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task UploadCsv_NegativeExecutionTime_Error()
        {
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n2024-01-01T12:00:00.0000Z;-10;1.5";

            var result = await service.UploadCsv("neg.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task UploadCsv_NegativeValue_Error()
        {
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n2024-01-01T12:00:00.0000Z;10;-1.5";

            var result = await service.UploadCsv("neg2.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task UploadCsv_NotEnoughColumns_Error()
        {
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n2024-01-01T12:00:00.0000Z;10";

            var result = await service.UploadCsv("cols.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task UploadCsv_EmptyValueInRow_Error()
        {
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n2024-01-01T12:00:00.0000Z;;1.5";

            var result = await service.UploadCsv("empty2.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task UploadCsv_TextInsteadOfNumber_Error()
        {
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n2024-01-01T12:00:00.0000Z;abc;1.5";

            var result = await service.UploadCsv("text.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task UploadCsv_WrongDateFormat_Error()
        {
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n01.01.2024;10;1.5";

            var result = await service.UploadCsv("date.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsFailed);
        }

        [Fact]
        public async Task UploadCsv_OneBadRow_NothingSaved()
        {
            // если хоть одна строка плохая, то не сохраняем весь файл
            var service = CreateService(out var db);

            var csv = "Date;ExecutionTime;Value\n" +
                      "2024-01-01T12:00:00.0000Z;10;1.5\n" +
                      "2024-01-01T12:01:00.0000Z;-5;2.5\n" +
                      "2024-01-01T12:02:00.0000Z;30;3.5";

            var result = await service.UploadCsv("mix.csv", "text/csv", MakeFile(csv), csv.Length);

            Assert.True(result.IsFailed);
            Assert.Equal(0, db.Results.Count());
            Assert.Equal(0, db.Values.Count());
        }

        [Fact]
        public async Task UploadCsv_BadFileAfterGood_OldDataStays()
        {
            var service = CreateService(out var db);

            var good = "Date;ExecutionTime;Value\n2024-01-01T12:00:00.0000Z;10;1.5";
            await service.UploadCsv("file.csv", "text/csv", MakeFile(good), good.Length);

            var bad = "Date;ExecutionTime;Value\n2024-01-01T12:00:00.0000Z;10;-1.5";
            var result = await service.UploadCsv("file.csv", "text/csv", MakeFile(bad), bad.Length);

            Assert.True(result.IsFailed);
            Assert.Equal(1, db.Results.Count());
            Assert.Equal(1, db.Values.Count());
        }

        [Fact]
        public async Task UploadCsv_SameFileName_Overwrite()
        {
            var service = CreateService(out var db);

            var first = "Date;ExecutionTime;Value\n" +
                        "2024-01-01T12:00:00.0000Z;10;1\n" +
                        "2024-01-01T12:01:00.0000Z;20;2";
            await service.UploadCsv("file.csv", "text/csv", MakeFile(first), first.Length);

            var second = "Date;ExecutionTime;Value\n2024-01-01T13:00:00.0000Z;99;9";
            var result = await service.UploadCsv("file.csv", "text/csv", MakeFile(second), second.Length);

            Assert.True(result.IsSuccess);
            Assert.Equal(1, db.Results.Count());

            var values = db.Values.AsNoTracking().ToList();
            Assert.Single(values);
            Assert.Equal(9, values[0].Value);
        }
    }
}
