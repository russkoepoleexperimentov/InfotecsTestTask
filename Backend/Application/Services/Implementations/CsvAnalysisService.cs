using System.Globalization;
using Application.Dtos;
using Application.Persistence;
using Domain.Entities;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementations
{
    public class CsvAnalysisService : ICsvAnalysisService
    {
        const char CsvDelimeter = ';';
        const string DateFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFFK";
        const long MaxFileLength = 10 * 1024 * 1024;

        private readonly IAppDbContext _dbContext;

        private readonly string _csvHeader;
        private readonly DateTime _minDate;
        private readonly int _linesLimit;

        public CsvAnalysisService(IAppDbContext dbContext)
        {
            _dbContext = dbContext;

            _csvHeader = string.Join(CsvDelimeter, ["Date", "ExecutionTime", "Value"]);
            _minDate = new DateTime(2000, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            _linesLimit = 10000;
        }

        public async Task<Result<ResultViewDto>> UploadCsv(string fileName, string fileContentType, Stream fileReadStream, long fileLength)
        {
            if (IsCSVContentType(fileContentType) == false)
                return Result.Fail("Wrong content type");

            if (fileReadStream is null)
                return Result.Fail("Error while reading a file");

            if (fileLength > MaxFileLength)
                return Result.Fail($"File is too large (max: {MaxFileLength} bytes)");

            var parseResult = await ParseCsv(fileReadStream);

            if (parseResult.IsFailed)
                return parseResult.ToResult();

            return await SaveResults(fileName, parseResult.Value);
        }

        private async Task<Result<List<ValueRecord>>> ParseCsv(Stream fileReadStream)
        {
            using var reader = new StreamReader(fileReadStream);

            var header = await reader.ReadLineAsync();

            if (header is null || header != _csvHeader)
                return Result.Fail("Wrong CSV header");

            List<ValueRecord> values = new List<ValueRecord>();

            int lineNum = 1; // at header on start

            while (await reader.ReadLineAsync() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                lineNum++;

                if (lineNum > _linesLimit + 1)
                    return Result.Fail($"Invalid lines count (max: {_linesLimit})");

                var tokens = line.Split(CsvDelimeter).Select(token => token.Trim()).ToArray();

                if (tokens.Length != 3)
                    return Result.Fail($"Invalid tokens length @ {lineNum}");

                if (!DateTime.TryParseExact(tokens[0], DateFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var date))
                    return Result.Fail($"Failed parse a {nameof(date)} @ {lineNum}");

                if (!int.TryParse(tokens[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int executionTime))
                    return Result.Fail($"Failed parse an {nameof(executionTime)} @ {lineNum}");

                if (!float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                    return Result.Fail($"Failed parse an {nameof(value)} @ {lineNum}");

                if (date < _minDate || date > DateTime.UtcNow)
                    return Result.Fail($"Invalid date @ {lineNum}");

                if (executionTime < 0)
                    return Result.Fail($"Invalid executionTime @ {lineNum}");

                if (value < 0)
                    return Result.Fail($"Invalid value @ {lineNum}");

                values.Add(new ValueRecord
                {
                    Id = Guid.NewGuid(),
                    Date = date,
                    ExceutionTime = executionTime,
                    Value = value,
                });
            }

            if (values.Count < 1)
                return Result.Fail($"Invalid lines count (min: 1)");

            return Result.Ok(values);
        }

        private async Task<Result<ResultViewDto>> SaveResults(string fileName, List<ValueRecord> values)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var existing = await _dbContext.Results
                    .FirstOrDefaultAsync(fileResult => fileResult.FileName == fileName);

                if (existing is not null)
                    _dbContext.Results.Remove(existing);

                var fileResult = BuildFileResult(fileName, values);

                _dbContext.Results.Add(fileResult);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Result.Ok(ToViewDto(fileResult));
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync();
                return Result.Fail(new Error("Failed to save the file results").CausedBy(exception));
            }
        }

        private static FileResult BuildFileResult(string fileName, List<ValueRecord> values)
        {
            var fileResultId = Guid.NewGuid();

            foreach (var value in values)
                value.FileResultId = fileResultId;

            var minDate = values.Min(value => value.Date);
            var maxDate = values.Max(value => value.Date);

            return new FileResult
            {
                Id = fileResultId,
                FileName = fileName,
                DeltaSeconds = (int)(maxDate - minDate).TotalSeconds,
                FirstExecutionTime = minDate,
                AverageExcecutionTime = (int)Math.Round(values.Average(value => (double)value.ExceutionTime)),
                AverageValue = (float)values.Average(value => (double)value.Value),
                MedianValue = Median(values),
                MinimumValue = values.Min(value => value.Value),
                MaximumValue = values.Max(value => value.Value),
                Values = values,
            };
        }

        private static float Median(List<ValueRecord> values)
        {
            var sorted = values.Select(value => value.Value).Order().ToArray();
            var middle = sorted.Length / 2;

            if (sorted.Length % 2 != 0)
                return sorted[middle];

            return (float)(((double)sorted[middle - 1] + sorted[middle]) / 2);
        }

        private static ResultViewDto ToViewDto(FileResult fileResult) => new ResultViewDto
        {
            Id = fileResult.Id,
            FileName = fileResult.FileName,
            DeltaSeconds = fileResult.DeltaSeconds,
            FirstExcecutionTime = fileResult.FirstExecutionTime,
            AverageExcecutionTime = fileResult.AverageExcecutionTime,
            AverageValue = fileResult.AverageValue,
            MedianValue = fileResult.MedianValue,
            MinimumValue = fileResult.MinimumValue,
            MaximumValue = fileResult.MaximumValue,
        };

        public async Task<Result<List<ResultViewDto>>> GetMostRecentResults()
        {
            throw new NotImplementedException();
        }

        public async Task<Result<PagedListDto<ResultViewDto>>> SearchResults(ResultSearchDto searchDto)
        {
            throw new NotImplementedException();
        }

        // https://github.com/medusajs/medusa/issues/15416
        private static bool IsCSVContentType(string contentType) => contentType is "application/vnd.ms-excel" or "text/csv" or "application/csv";
    }
}
