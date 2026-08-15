using System.Globalization;
using System.Text.RegularExpressions;
using Application.Dtos;
using Domain.Entities;
using FluentResults;

namespace Application.Services.Implementations
{
    public class CsvAnalysisService : ICsvAnalysisService
    {
        const char CsvDelimeter = ',';


        private readonly string _csvHeader;

        public CsvAnalysisService() 
        {
            _csvHeader = string.Join(CsvDelimeter, ["Date", "ExecutionTime", "Value"]);
        }

        public async Task<Result<List<ValueRecord>>> UploadCsv(string fileName, string fileContentType, Stream fileReadStream, long fileLength)
        {
            if (IsCSVContentType(fileContentType) == false)
                return Result.Fail("Wrong content type");

            if (fileReadStream is null)
                return Result.Fail("Error while reading a file");

            var reader = new StreamReader(fileReadStream);

            var header = await reader.ReadLineAsync();

            if(header is null || header == _csvHeader)
                return Result.Fail("Wrong CSV header");

            List<ValueRecord> testOutput = new List<ValueRecord>();

            int lineNum = 1;
            while (!reader.EndOfStream) 
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var tokens = line.Split(CsvDelimeter).Select(token => token.Trim()).ToArray();

                if (tokens == null || tokens.Length == 0)
                    continue;

                var valueRecord = new ValueRecord();


                if (!DateTime.TryParse(tokens[0], null, DateTimeStyles.RoundtripKind, out DateTime date))
                {
                    return Result.Fail($"Failed parse a {nameof(date)} @ {lineNum}");
                }

                if(!int.TryParse(tokens[1], out int executionTime))
                {
                    return Result.Fail($"Failed parse an {nameof(executionTime)} @ {lineNum}");
                }

                if (!float.TryParse(tokens[2], out float value))
                {
                    return Result.Fail($"Failed parse an {nameof(value)} @ {lineNum}");
                }


                valueRecord.Date = date;
                valueRecord.Value = value;
                valueRecord.ExceutionTime = executionTime;

                testOutput.Add(valueRecord);

                lineNum++;
            }

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
