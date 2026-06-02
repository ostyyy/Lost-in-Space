using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpaceServer.Data;
using SpaceServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SpaceServer.Controllers
{
    [ApiController]
    [Route("api/APOD")]
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
            DateTime pureUtcDate = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Utc);

            bool userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
            if (!userExists)
            {
                return BadRequest("User access denied.");
            }

            bool alreadyExists = await _context.ImagesOfTheDay
                 .AnyAsync(i => i.Date == pureUtcDate && i.UserId == request.UserId);

            if (alreadyExists)
            {
                return Conflict("This image is already saved in your personal archive.");
            }

            var newImage = new APOD
            {
                UserId = request.UserId,
                Date = pureUtcDate, 
                Title = request.Title,
                Explanation = request.Explanation,
                ImageURL = request.ImageURL
            };

            _context.ImagesOfTheDay.Add(newImage);
            await _context.SaveChangesAsync();

            return Ok("Saved to DB");
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserArchive(int userId)
        {
            var archive = await _context.ImagesOfTheDay
                .Where(i => i.UserId == userId) 
                .OrderByDescending(i => i.Date)
                .ToListAsync();

            var clientResult = archive.Select(i => new
            {
                id = i.Id,
                userId = i.UserId,
                date = i.Date,
                title = i.Title,
                explanation = i.Explanation,
                imageURL = i.ImageURL
            }).ToList();

            return Ok(clientResult);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteArchiveItem(int id)
        {
            var item = await _context.ImagesOfTheDay.FindAsync(id);

            if (item == null)
            {
                return NotFound("Record not found");
            }

            _context.ImagesOfTheDay.Remove(item);
            await _context.SaveChangesAsync();

            return Ok("Record deleted successfully");
        }
    }
}
