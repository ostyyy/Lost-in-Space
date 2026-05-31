using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpaceServer.Data;
using SpaceServer.Models;
using Microsoft.EntityFrameworkCore;

namespace SpaceServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APODController : ControllerBase
    {
        private readonly AppDB _context;

        public APODController(AppDB context)
        {
            _context = context;
        }

        public class AddImageRequest
        {
            public int UserId { get; set; }
            public DateTime Date { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Explanation { get; set; } = string.Empty;
            public string ImageURL { get; set; } = string.Empty;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddImage([FromBody] AddImageRequest request)
        {
            bool userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
            if (!userExists)
            {
                return BadRequest("User access denied.");
            }

            bool alreadyExists = await _context.ImagesOfTheDay
                 .AnyAsync(i => i.Date.Date == request.Date.Date && i.UserId == request.UserId);

            if (alreadyExists)
            {
                return Conflict("This image is already saved in your personal archive.");
            }

            var newImage = new APOD
            {
                UserId = request.UserId,
                Date = request.Date.ToUniversalTime(),
                Title = request.Title,
                Explanation = request.Explanation,
                ImageURL = request.ImageURL
            };

            _context.ImagesOfTheDay.Add(newImage);
            await _context.SaveChangesAsync();

            return Ok("Saved to DB");

        }


        [HttpGet("my-archive/{userId}")]
        public async Task<IActionResult> GetUserArchive(int userId)
        {
            var userImages = await _context.ImagesOfTheDay
                .Where(i => i.UserId == userId) 
                .OrderByDescending(i => i.Date)
                .ToListAsync();

            return Ok(userImages);
        }
        [HttpDelete("delete/{id}/{userId}")]
        public async Task<IActionResult> DeleteImage(int id, int userId)
        {
            var image = await _context.ImagesOfTheDay.FindAsync(id);

            if (image == null)
            {
                return NotFound("Record not found in the archive.");
            }

            if (image.UserId != userId)
            {
                return Forbid("Access denied. You can only delete your own archived images.");
            }

            _context.ImagesOfTheDay.Remove(image);

            await _context.SaveChangesAsync();

            return Ok("Successfully removed from archive");
        }
    }
}
