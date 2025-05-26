using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.IO;

namespace APIGatewayController
{
    [ApiController]
    [Route("api/file")]
    public class ApiController : ControllerBase
    {
        private readonly HttpClient _fileStoringService;
        private readonly HttpClient _fileAnalysisService;

        public ApiController(IHttpClientFactory httpClientFactory)
        {
            _fileStoringService = httpClientFactory.CreateClient("FileService");
            _fileAnalysisService = httpClientFactory.CreateClient("AnalisysService");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadTxt([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не предоставлен или пустой.");

            var extension = Path.GetExtension(file.FileName);
            if (!extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Разрешены только файлы .txt.");

            try
            {
                using var content = new MultipartFormDataContent();
                await using var stream = file.OpenReadStream();
                content.Add(new StreamContent(stream), "file", file.FileName);

                var response = await _fileStoringService.PostAsync("file", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, error);
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                return Content(resultJson, "application/json");
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    "Ошибка связи с сервисом хранения файлов.");
            }
            catch (IOException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ошибка чтения загруженного файла.");
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Произошла непредвиденная ошибка.");
            }
        }

        [HttpGet("upload/{id:guid}")]
        public async Task<IActionResult> GetFile(Guid id)
        {
            try
            {
                var response = await _fileStoringService.GetAsync($"file/{id}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return NotFound();

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, error);
                }

                var stream = await response.Content.ReadAsStreamAsync();
                var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "file.txt";
                return File(stream, "text/plain", fileName);
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    "Ошибка связи с сервисом хранения файлов.");
            }
            catch (IOException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Ошибка чтения файла.");
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Произошла непредвиденная ошибка.");
            }
        }

        [HttpGet("analysis/{id:guid}")]
        public async Task<IActionResult> GetFileAnalysis(Guid id)
        {
            try
            {
                var response = await _fileAnalysisService.GetAsync($"fileinfo/{id}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return NotFound();

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, error);
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                return Content(resultJson, "application/json");
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    "Ошибка связи с сервисом анализа файлов.");
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Произошла непредвиденная ошибка при получении анализа.");
            }
        }
    }
}