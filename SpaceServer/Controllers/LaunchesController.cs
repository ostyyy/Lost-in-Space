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
    }
}