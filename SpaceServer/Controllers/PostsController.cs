using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceServer.Data;
using SpaceServer.Models;

namespace SpaceServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly AppDB _context;

        public PostsController(AppDB context)
        {
            _context = context;
        }

        public class CreatePostRequest
        {
            public string Title { get; set; } =  string.Empty;
            public string Content { get; set; } = string.Empty;
            public int TopicID { get; set; }
            public int AuthorID { get; set; }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            var newPost = new Post
            {
                Title = request.Title,
                Content = request.Content,
                AuthorID = request.AuthorID,
                TopicID = request.TopicID,
                CreatedDate = DateTime.UtcNow
            };

            _context.Posts.Add(newPost);
            await _context.SaveChangesAsync();

            return Ok("Added!");
        }

        [HttpGet("topic/{topicId}")]
        public async Task<IActionResult> GetPostsForTopic(int topicId)
        {
            var posts = await _context.Posts
                .Where(p => p.TopicID == topicId)
                .ToListAsync();

            return Ok(posts);
        }

        [HttpDelete("delete/{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound("Not found.");
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok("Deleted!");
        }
    }
}
