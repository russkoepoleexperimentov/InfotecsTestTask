using Application.Dtos;
using Application.Services;
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
        [ProducesResponseType<ResponseDto<ResultViewDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResponseDto>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile formFile)
        {
            return await _csvAnalysisService
                .UploadCsv(formFile.FileName, formFile.ContentType, formFile.OpenReadStream(), formFile.Length)
                .ToActionResult();
        }

        [HttpGet("search")]
        [ProducesResponseType<ResponseDto<PagedListDto<ResultViewDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResponseDto>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Search([FromQuery] ResultSearchDto searchDto)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [ProducesResponseType<List<ResultViewDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResponseDto>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get()
        {
            throw new NotImplementedException();
        }
    }
}
