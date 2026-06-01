using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceServer.Data;
using SpaceServer.Models;

namespace SpaceServer.Controllers
{

    public class FavoritesRequest
    {
        public string UserLogin { get; set; } = string.Empty;
        public Launches LaunchData { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class LaunchesController : ControllerBase
    {
        private readonly AppDB _context;

        public LaunchesController(AppDB context)
        {
            _context = context;
        }

        [HttpPost("favorites")]
        public async Task<IActionResult> FavotiteLaunch([FromBody] FavoritesRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.UserLogin);

            if (user == null)
            {
                return BadRequest("Error no such user!");
            }

            var newLaunch = request.LaunchData;
            newLaunch.UserId = user.Id;

            _context.Launches.Add(newLaunch);

            if (request.LaunchData.LaunchDate.HasValue)
            {
                request.LaunchData.LaunchDate = request.LaunchData.LaunchDate.Value.ToUniversalTime();
            }

            _context.Launches.Add(request.LaunchData);
            await _context.SaveChangesAsync();

            

            return Ok("Added!");
        }

        [HttpGet("my/{login}")]
        public async Task<ActionResult<IEnumerable<Launches>>> GetMyLaunches(string login)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);
            if (user == null)
            {
                return BadRequest("Error no such user!");
            }

            var myLaunches = await _context.Launches.Where(l => l.UserId == user.Id).ToListAsync();

            return myLaunches;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Launches>>> GetLaunches()
        {
            return await _context.Launches.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Launches>> GetLaunch(int id)
        {
            var launch = await _context.Launches.FindAsync(id);

            if (launch == null)
            {
                return NotFound();
            }

            return launch;
        }

        // POST: api/Launches
        [HttpPost]
        public async Task<ActionResult<Launches>> PostLaunch(Launches launch)
        {
            _context.Launches.Add(launch);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLaunch), new { id = launch.Id }, launch);
        }

        // PUT: api/Launches/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLaunch(int id, Launches launch)
        {
            if (id != launch.Id) return BadRequest();

            _context.Entry(launch).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Launches.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/Launches/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLaunch(int id)
        {
            var launch = await _context.Launches.FindAsync(id);
            if (launch == null) return NotFound();

            _context.Launches.Remove(launch);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}