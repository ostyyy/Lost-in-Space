using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpaceServer.Data;
using SpaceServer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaceServer.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDB _context;

        public CommentsController(AppDB context)
        {
            _context = context;
        }

        public class AddCommentRequest
        {
            public string Content { get; set; } = string.Empty;
            public int PostID { get; set; }
            public int? ParentCommentID { get; set; }
            public int? AuthorID { get; set; }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateComment([FromBody] AddCommentRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Comment content cannot be empty.");
            }

            bool postExists = await _context.Posts.AnyAsync(p => p.ID == request.PostID);
            if (!postExists)
            {
                return NotFound($"Post with ID {request.PostID} not found.");
            }

            if (request.AuthorID.HasValue)
            {
                bool userExists = await _context.Users.AnyAsync(u => u.Id == request.AuthorID.Value);
                if (!userExists)
                {
                    return BadRequest("User access denied.");
                }
            }

            var newComment = new Comment
            {
                Content = request.Content,
                PostID = request.PostID,
                ParentCommentID = request.ParentCommentID,
                AuthorID = request.AuthorID,
                CreatedDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)
            };

            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            return Ok("Saved to DB");
        }


        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetCommentsForPost(int postId)
        {
            var comments = await _context.Comments
                .Include(c => c.Author)
                .Where(c => c.PostID == postId)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();

            var clientResult = comments.Select(c => new
            {
                id = c.ID,
                content = c.Content,
                postID = c.PostID,
                parentCommentID = c.ParentCommentID,
                authorID = c.AuthorID,
                authorName = c.Author != null ? c.Author.Login : $"User #{c.AuthorID}",
                createdDate = c.CreatedDate
            }).ToList();

            return Ok(clientResult);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteComment(int id, [FromQuery] int userId)
        {
            try
            {
                var comment = await _context.Comments.FindAsync(id);

                if (comment == null)
                {
                    return NotFound("Comment not found.");
                }

                if (comment.AuthorID != userId)
                {
                    return StatusCode(403, "You can only delete your own comments.");
                }

                comment.Content = "This comment has been deleted by the user.";
                comment.AuthorID = null;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Comment deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error during delete: {ex.Message}");
            }
        }
    }
}