using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace APIGatewayController
{
    [ApiController]
    [Route("api/file")]
    public class ApiController : ControllerBase
    {
        private readonly HttpClient _fileStoringService;
        private readonly HttpClient _fileAnalisysService;

        public ApiController(IHttpClientFactory httpClientFactory)
        {
            _fileStoringService = httpClientFactory.CreateClient("FileService");
            _fileAnalisysService = httpClientFactory.CreateClient("AnalisysService");
        }


        [HttpPost("upload")]
        public async Task<IActionResult> UploadTxt([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided or empty.");

            var extension = Path.GetExtension(file.FileName);
            if (!extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .txt files are allowed.");

            try
            {
                using var content = new MultipartFormDataContent();
                await using var stream = file.OpenReadStream();
                var streamContent = new StreamContent(stream);
                content.Add(streamContent, "file", file.FileName);

                var response = await _fileStoringService.PostAsync("file", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, error);
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                return Content(resultJson, "application/json");
            }
            catch (IOException ioEx)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error reading the uploaded file.");
            }
            catch (HttpRequestException httpEx)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    "Error communicating with file storage service.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
            }
        }

        // GET /api/{id}
        [HttpGet("upload/{id:guid}")]
        public async Task<IActionResult> GetFile(Guid id)
        {
            var response = await _fileStoringService.GetAsync($"file/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound();

            var stream = await response.Content.ReadAsStreamAsync();
            var fileName = response.Content.Headers.ContentDisposition?.FileName ?? "file.txt";

            return File(stream, "text/plain", fileName);
        }
        
        [HttpGet("analysis/{id:guid}")]
        public async Task<IActionResult> GetFileAnalysis(Guid id)
        {
            try
            {
                var response = await _fileAnalisysService.GetAsync($"fileinfo/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return NotFound();

                response.EnsureSuccessStatusCode();
                var resultJson = await response.Content.ReadAsStringAsync();

                return Content(resultJson, "application/json");
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    "Error communicating with file analysis service.");
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while fetching analysis.");
            }
        }
    }
}