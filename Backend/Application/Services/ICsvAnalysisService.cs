using Application.Dtos;
using Domain.Entities;
using FluentResults;

namespace Application.Services
{
    public interface ICsvAnalysisService
    {
        Task<Result<ResultViewDto>> UploadCsv(string fileName, string fileContentType, Stream fileReadStream, long fileLength);
        Task<Result<PagedListDto<ResultViewDto>>> SearchResults(ResultSearchDto searchDto);
        Task<Result<List<ValueViewDto>>> GetMostRecentValues(string fileName);
    }
}
