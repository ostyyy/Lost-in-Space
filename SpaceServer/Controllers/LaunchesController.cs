using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceServer.Data;
using SpaceServer.Models;

namespace SpaceServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LaunchesController : ControllerBase
    {
        private readonly AppDB _context;

        public LaunchesController(AppDB context)
        {
            _context = context;
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

            // Повертає статус 201 Created і посилання на новий об'єкт
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