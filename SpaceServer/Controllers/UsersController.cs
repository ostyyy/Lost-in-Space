using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpaceServer.Data;
using SpaceServer.Models;
using Microsoft.EntityFrameworkCore;

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
        public class RegisterRequest
        {
            public string Login { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var newUser = new User
            {
                Login = request.Login,
                Password = request.Password
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok($"Added {request.Login}!");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] RegisterRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Login == request.Login &&
                u.Password == request.Password);

            if (user != null)
            {
                return Ok(new
                {
                    id = user.Id,
                    login = user.Login
                });
            }
            else
            {
                return Unauthorized(new { message = "Incorrect login or password!" });
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteUser([FromBody] RegisterRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Login == request.Login &&
                u.Password == request.Password);


            if (user == null)
            {
                return Unauthorized("Invalid login or password.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok($"Deleted! {request.Login}!");
        }
    }
}
