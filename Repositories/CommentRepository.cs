using CST465_project.Data;
using CST465_project.Models;
using Microsoft.EntityFrameworkCore;

namespace CST465_project.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _db;
        private ILogger<CommentRepository> _logger;

        public CommentRepository(ApplicationDbContext db, ILogger<CommentRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByVisualizationIdAsync(int visualizationId)
        {
            var comments = await _db.Comments
                .Where(c => c.VisualizationId == visualizationId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return comments;
        }

        public async Task AddCommentAsync(Comment comment)
        {
            try
            {
                _db.Comments.Add(comment);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                _logger.LogError(ex, "Error adding comment to visualization {Id}", comment.VisualizationId);
                throw;
            }
        }
    }
}
