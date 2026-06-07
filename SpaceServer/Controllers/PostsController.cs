using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceServer.Data;
using SpaceServer.Models;
using System;
using System.Threading.Tasks;

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
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public string TopicName { get; set; } = string.Empty;
            public int AuthorID { get; set; }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Title.ToLower() == request.TopicName.ToLower());
            if (topic == null)
            {
                topic = new Topic 
                { 
                    Title = request.TopicName,
                    AuthorID = request.AuthorID
                };
                _context.Topics.Add(topic);
                await _context.SaveChangesAsync();
            }
            var newPost = new Post
            {
                Title = request.Title,
                Content = request.Content,
                TopicID = topic.ID,
                AuthorID = request.AuthorID, 
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

        [HttpGet("all")]
        public async Task<IActionResult> GetAllPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.Topic)
                .Include(p => p.Author)
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => new
                {
                    Id = p.ID, 
                    Title = p.Title,
                    Content = p.Content,
                    CreatedDate = p.CreatedDate,
                    Topic = new { Title = p.Topic.Title }, 
                    Author = new { Login = p.Author.Login}
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostById(int postId)
        {
            var post = await _context.Posts
                .Include(p => p.Topic)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.ID == postId);

            if (post == null)
            {
                return NotFound("Post not found.");
            }

            return Ok(new
            {
                Id = post.ID,
                Title = post.Title,
                Content = post.Content,
                CreatedDate = post.CreatedDate,
                Topic = post.Topic != null ? new { Title = post.Topic.Title } : null,
                Author = post.Author != null ? new { Login = post.Author.Login } : null
            });
        }

        [HttpDelete("delete/{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound("Not found.");
            }

            //UPD: delete related comments
            var relatedComments = _context.Comments.Where(c => c.PostID == postId);
            _context.Comments.RemoveRange(relatedComments);

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok("Deleted!");
        }
    }
}