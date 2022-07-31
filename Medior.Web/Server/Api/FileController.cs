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
        private readonly FileExtensionContentTypeProvider _contentTypeProvider = new ();

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

            if (_contentTypeProvider.TryGetContentType(result.UploadedFile!.FileName, out var resolvedType))
            {
                mimeType = resolvedType;
            }

            return File(result.FileStream!, mimeType, result.UploadedFile!.FileName);
        }

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _fileManager.Delete(id);
            return NoContent();
        }

        [HttpHead("{id}")]
        public async Task<IActionResult> Head(Guid id)
        {
            var result = await _fileManager.GetData(id);

            if (!result.IsSuccess)
            {
                return NotFound("The file does not exist.");
            }

            var mimeType = "application/octet-stream";
            if (_contentTypeProvider.TryGetContentType(result.Value!.FileName, out var resolvedType))
            {
                mimeType = resolvedType;
            }

            return File(Array.Empty<byte>(), mimeType, result.Value!.FileName);
        }
    }
}
