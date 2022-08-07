using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FileIO = System.IO.File;

namespace Medior.Web.Server.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        private readonly string _downloadsDir;

        public VersionController(IWebHostEnvironment hostEnv)
        {
            _downloadsDir = Directory.CreateDirectory(Path.Combine(hostEnv.WebRootPath, "downloads")).FullName;
        }

        [HttpGet("desktop")]
        public ActionResult<string> GetDesktopVersion()
        {
            var filePath = Path.Combine(_downloadsDir, "Medior.exe");
            if (!FileIO.Exists(filePath))
            {
                return NotFound();
            }

            var version = FileVersionInfo.GetVersionInfo(Path.Combine(_downloadsDir, "Medior.exe"));

            if (version.FileVersion is null)
            {
                return NotFound();
            }

            return version.FileVersion.ToString();
        }
    }
}
