//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore; 
//using SpaceServer.Data;
//using SpaceServer.Models;

//namespace SpaceServer.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class TopicsController : ControllerBase
//    {
//        private readonly AppDB _context;

//        public TopicsController(AppDB context)
//        {
//            _context = context;
//        }

//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<Topic>>> GetTopics()
//        {
//            return await _context.Topics.Include(t => t.Author).ToListAsync();
//        }

//        [HttpGet("{id}")]
//        public async Task<ActionResult<Topic>> GetTopic(int id)
//        {
//            var topic = await _context.Topics.Include(t => t.Author).FirstOrDefaultAsync(t => t.ID == id);

//            if (topic == null)
//            {
//                return NotFound(); 
//            }

//            return topic;
//        }

//    }
//}
