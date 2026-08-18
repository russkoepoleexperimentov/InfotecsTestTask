using Application.Dtos;
using Application.Services;
using FluentResults;
using FluentResults.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    /// <summary>
    /// Работа с результатами обработки, загружаемыми в виде CSV-файлов.
    /// </summary>
    [ApiController]
    [Route("/analysis/")]
    public class CsvAnalysisController : ControllerBase
    {
        private readonly ICsvAnalysisService _csvAnalysisService;

        public CsvAnalysisController(ICsvAnalysisService csvAnalysisService)
        {
            _csvAnalysisService = csvAnalysisService;
        }

        /// <summary>
        /// Загружает CSV-файл, валидирует его и сохраняет значения вместе с интегральными результатами.
        /// </summary>
        /// <remarks>
        /// Ожидается файл с заголовком <c>Date;ExecutionTime;Value</c> и от 1 до 10 000 строк значений.
        ///
        /// Правила валидации: дата не раньше 01.01.2000 и не позже текущего момента, время выполнения
        /// и показатель не меньше нуля, все три значения строки обязательны и должны соответствовать своим типам.
        /// Если нарушено хотя бы одно правило, файл считается невалидным и в базу не попадает ничего.
        ///
        /// Если результат для файла с таким именем уже есть, он перезаписывается.
        /// </remarks>
        /// <param name="formFile">CSV-файл в составе multipart/form-data.</param>
        /// <response code="200">Файл принят; возвращаются интегральные результаты по нему.</response>
        /// <response code="400">Файл отсутствует, имеет неверный формат или не прошёл валидацию.</response>
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

        /// <summary>
        /// Возвращает постранично интегральные результаты, подходящие под заданные фильтры.
        /// </summary>
        /// <remarks>
        /// Все фильтры необязательны и комбинируются друг с другом: часть имени файла,
        /// а также диапазоны по времени запуска первой операции, среднему показателю
        /// и среднему времени выполнения. Без фильтров возвращается вся таблица результатов.
        /// </remarks>
        /// <param name="searchDto">Фильтры и параметры постраничной выборки.</param>
        /// <response code="200">Страница результатов и общее количество подходящих записей.</response>
        /// <response code="400">Некорректные параметры выборки.</response>
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
        /// Возвращает последние 10 значений заданного файла, отсортированные по времени запуска (Date).
        /// </summary>
        /// <param name="fileName">Имя ранее загруженного файла (точное совпадение).</param>
        /// <response code="200">Значения файла, от самого позднего к более раннему.</response>
        /// <response code="400">Имя файла не задано или файл с таким именем не загружался.</response>
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
