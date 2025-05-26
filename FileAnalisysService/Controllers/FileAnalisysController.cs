using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FileAnalisysService.Data;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FileAnalisysService.Controllers
{
    [ApiController]
    [Route("api/fileinfo")]
    public class AnalisysController : ControllerBase
    {
        private readonly Db _db;
        private readonly HttpClient _fileStoringService;

        public AnalisysController(Db db, IHttpClientFactory httpFactory)
        {
            _db = db;
            _fileStoringService = httpFactory.CreateClient("FileService");
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetFileInfo(Guid id)
        {
            try
            {
                if (_db.TryGetInfo(id, out var existing))
                    return Ok(existing);

                var response = await _fileStoringService.GetAsync($"file/{id}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return NotFound();

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, err);
                }

                var content = await response.Content.ReadAsStringAsync();
                var paragraphCount = CountParagraphs(content);
                var wordCount = CountWords(content);
                var charCount = content.Length;
                var hash = ComputeHash(content);
                var isPlagiarized = _db.HasHash(hash);

                var info = new FileAnalysisInfo(paragraphCount, wordCount, charCount, isPlagiarized, hash);
                _db.AddFile(id, info);

                return Ok(info);
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status502BadGateway, "Ошибка связи сервисом хранения файлов.");
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервиса анализа файлов.");
            }
        }

        private int CountParagraphs(string text) =>
            text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries).Length;

        private int CountWords(string text) =>
            text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;

        private string ComputeHash(string text)
        {
            using var sha = SHA256.Create();
            var data = Encoding.UTF8.GetBytes(text);
            var hash = sha.ComputeHash(data);
            var sb  = new StringBuilder();
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}