using Microsoft.AspNetCore.Mvc;

namespace TutorialService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TutorialController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public TutorialController(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        // GET /api/tutorial/tmt  -> returns metadata (title, video URL, poster URL)
        [HttpGet("{key}")]
        public IActionResult GetTutorial(string key)
        {
            var section = _config.GetSection($"Tutorials:{key}");
            if (!section.Exists())
                return NotFound(new { message = "No tutorial found." });

            var poster = section["Poster"];
            return Ok(new
            {
                title = section["Title"],
                videoUrl = $"/api/tutorial/{key}/video",
                posterUrl = string.IsNullOrEmpty(poster) ? null : $"/videos/tutorials/{poster}"
            });
        }

        // GET /api/tutorial/tmt/video -> streams the actual file, resolved server-side only
        [HttpGet("{key}/video")]
        public IActionResult GetVideo(string key)
        {
            var fileName = _config[$"Tutorials:{key}:VideoFile"];
            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            // fileName comes ONLY from config, never from the request — no path traversal possible
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var fullPath = Path.Combine(webRoot, "videos", "tutorials", fileName);
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            return PhysicalFile(fullPath, "video/mp4", enableRangeProcessing: true);
        }
    }
}
