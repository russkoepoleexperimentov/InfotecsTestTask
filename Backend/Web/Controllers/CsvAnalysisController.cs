using Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("/")]
    public class CsvAnalysisController : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType<ResponseDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResponseDto>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile formFile)
        {
            throw new NotImplementedException();
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
