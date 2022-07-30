using Medior.Shared.Entities;
using Medior.Web.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Medior.Web.Server.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IUploadedFileManager _fileManager;

        public FileController(IUploadedFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            var result = await _fileManager.Load(id);

            if (!result.Found)
            {
                return NotFound("The file does not exist.");
            }

            var mimeType = "application/octet-stream";

            var contentProvider = new FileExtensionContentTypeProvider();
            if (contentProvider.TryGetContentType(result.UploadedFile!.FileName, out var resolvedType))
            {
                mimeType = resolvedType;
            }

            return File(result.FileStream!, mimeType, result.UploadedFile!.FileName);
        }

        [IgnoreAntiforgeryToken]
        [RequestSizeLimit(100_000_000)]
        [HttpPost]
        public async Task<ActionResult<UploadedFile>> Post(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest();
            }

            return await _fileManager.Save(file);
        }

        [IgnoreAntiforgeryToken]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _fileManager.Delete(id);
            return NoContent();
        }
    }
}
