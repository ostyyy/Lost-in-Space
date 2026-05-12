using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpaceServer.Data;
using SpaceServer.Models;

namespace SpaceServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDB _context;

        public UsersController(AppDB context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register(string login, string password)
        {
            var newUser = new User
            {
                Login = login,
                Password = password
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok($"Added {login}!");
        }
    }
}
