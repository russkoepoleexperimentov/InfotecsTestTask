using Application.Dtos;
using Application.Services;
using FluentResults;
using FluentResults.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("/analysis/")]
    public class CsvAnalysisController : ControllerBase
    {
        private readonly ICsvAnalysisService _csvAnalysisService;

        public CsvAnalysisController(ICsvAnalysisService csvAnalysisService)
        {
            _csvAnalysisService = csvAnalysisService;
        }

        [HttpPost]
        [ProducesResponseType<ResultViewDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<List<ErrorDto>>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile? formFile)
        {
            if (formFile is null || formFile.Length == 0)
                return await Task.FromResult(Result.Fail<ResultViewDto>("File is required")).ToActionResult();

            return await _csvAnalysisService
                .UploadCsv(formFile.FileName, formFile.ContentType, formFile.OpenReadStream(), formFile.Length)
                .ToActionResult();
        }

        [HttpGet("search")]
        [ProducesResponseType<PagedListDto<ResultViewDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<List<ErrorDto>>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery] ResultSearchDto searchDto)
        {
            return await _csvAnalysisService
                .SearchResults(searchDto)
                .ToActionResult();
        }

        /// <summary>
        /// Последние 10 значений заданного файла, отсортированные по времени запуска (Date).
        /// </summary>
        [HttpGet("values")]
        [ProducesResponseType<List<ValueViewDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<List<ErrorDto>>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMostRecentValues([FromQuery] string fileName)
        {
            return await _csvAnalysisService
                .GetMostRecentValues(fileName)
                .ToActionResult();
        }
    }
}
