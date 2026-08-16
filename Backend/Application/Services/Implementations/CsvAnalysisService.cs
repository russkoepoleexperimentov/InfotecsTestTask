using System.Globalization;
using System.Text.RegularExpressions;
using Application.Dtos;
using Domain.Entities;
using FluentResults;

namespace Application.Services.Implementations
{
    public class CsvAnalysisService : ICsvAnalysisService
    {
        const char CsvDelimeter = ';';


        private readonly string _csvHeader;
        private readonly DateTime _minDate;
        private readonly int _linesLimit;

        public CsvAnalysisService() 
        {
            _csvHeader = string.Join(CsvDelimeter, ["Date", "ExecutionTime", "Value"]);
            _minDate = new DateTime(2000, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            _linesLimit = 10000;
        }

        public async Task<Result<List<ValueRecord>>> UploadCsv(string fileName, string fileContentType, Stream fileReadStream, long fileLength)
        {
            if (IsCSVContentType(fileContentType) == false)
                return Result.Fail("Wrong content type");

            if (fileReadStream is null)
                return Result.Fail("Error while reading a file");

            using var reader = new StreamReader(fileReadStream);

            var header = await reader.ReadLineAsync();

            if(header is null || header != _csvHeader)
                return Result.Fail("Wrong CSV header");

            List<ValueRecord> testOutput = new List<ValueRecord>();

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

                var valueRecord = new ValueRecord();

                if (!DateTime.TryParseExact(tokens[0], "yyyy-MM-ddTHH:mm:ss.ffffZ", CultureInfo.InvariantCulture, 
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var date))
                    return Result.Fail($"Failed parse a {nameof(date)} @ {lineNum}");

                if (!int.TryParse(tokens[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int executionTime))
                    return Result.Fail($"Failed parse an {nameof(executionTime)} @ {lineNum}");

                if (!float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                    return Result.Fail($"Failed parse an {nameof(value)} @ {lineNum}");

                if(date < _minDate || date > DateTime.UtcNow)
                    return Result.Fail($"Invalid date @ {lineNum}");

                if (executionTime < 0)
                    return Result.Fail($"Invalid executionTime @ {lineNum}");

                if (value < 0)
                    return Result.Fail($"Invalid value @ {lineNum}");

                valueRecord.Date = date;
                valueRecord.Value = value;
                valueRecord.ExceutionTime = executionTime;

                testOutput.Add(valueRecord);
            }

            if (lineNum < 2)
                return Result.Fail($"Invalid lines count (min: 1)");

            return Result.Ok(testOutput);
        }

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
