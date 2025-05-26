using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using FileStoringService.Data;

namespace FileStoringService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly string _storagePath;
        private readonly FileStoreContext _context;

        public FileController(IWebHostEnvironment env, FileStoreContext context)
        {
            _context = context;

            _storagePath = Path.Combine(env.ContentRootPath, "UploadedFiles");
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        // POST api/file
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<object>> UploadTxt([FromForm] IFormFile file)
        {
            if (file == null)
                return BadRequest("No file provided.");

            if (file.Length == 0)
                return BadRequest("Empty file.");

            var extension = Path.GetExtension(file.FileName);
            if (!extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .txt files are allowed.");

            try
            {
                var id = Guid.NewGuid();
                var filePath = Path.Combine(_storagePath, id + extension);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                _context.Files.Add(new FileRecord
                {
                    Id = id,
                    Path = filePath,
                    UploadedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();


                return Ok(new { id });
            }
            catch (IOException ioEx)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                "Ошибка при сохранении файла.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                                "Внутренняя ошибка сервиса хранения файлов.");
            }
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetFile(Guid id)
        {
            var record = await _context.Files.FindAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            if (!System.IO.File.Exists(record.Path))
                    return NotFound();

            try
            {
                await using var fs = new FileStream(record.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var memory = new MemoryStream();
                await fs.CopyToAsync(memory);
                memory.Position = 0;
                return File(memory, "text/plain", Path.GetFileName(record.Path));
            }
            catch (IOException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Ошибка при чтении файла");
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервиса");
            }
        }
    }
}