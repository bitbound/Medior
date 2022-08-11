using Medior.Web.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Medior.Web.Server.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClipboardController : ControllerBase
    {
        private readonly IClipboardSyncService _clipboardSync;

        public ClipboardController(IClipboardSyncService clipboardSync)
        {
            _clipboardSync = clipboardSync;
        }

        [RequestSizeLimit(20_000_000)]
        [HttpPost]
        public async Task<ActionResult<string>> Post()
        {
            using var ms = new MemoryStream();
            await Request.Body.CopyToAsync(ms);
            return _clipboardSync.GetAccessKey(ms.ToArray());
        }

        [HttpGet("{accessToken}")]
        public async Task Get(string accessToken)
        {
            if (_clipboardSync.TryGetContent(accessToken, out var content))
            {
                await Response.BodyWriter.WriteAsync(content);
                return;
            }
            Response.StatusCode = 404;
        }
    }
}
