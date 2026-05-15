using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpaceServer.Data;
using SpaceServer.Models;
using Microsoft.EntityFrameworkCore;

namespace SpaceServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IOTDController : ControllerBase
    {
        private readonly AppDB _context;

        public IOTDController(AppDB context)
        {
            _context = context;
        }

        public class AddImageRequest
        {
            public DateTime Date { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Explanation { get; set; } = string.Empty;
            public string ImageURL { get; set; } = string.Empty;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddImage([FromBody] AddImageRequest request)
        {
            var newImage = new ImageOfTheDay
            {
                Date = request.Date.ToUniversalTime(),
                Title = request.Title,
                Explanation = request.Explanation,
                ImageURL = request.ImageURL
            };

            _context.ImagesOfTheDay.Add(newImage);
            await _context.SaveChangesAsync();

            return Ok("Saved to DB");
        
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestImage()
        {
            var latestImage = await _context.ImagesOfTheDay
                .OrderByDescending(i => i.Date) 
                .FirstOrDefaultAsync();         

            if (latestImage == null)
            {
                return NotFound("Not found(");
            }

            return Ok(latestImage);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllImages()
        {
            var images = await _context.ImagesOfTheDay
                .OrderByDescending(i => i.Date)
                .ToListAsync();

            return Ok(images);
        }
    }
}
