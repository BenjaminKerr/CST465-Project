using CST465_project.Data;
using CST465_project.Models;
using Microsoft.EntityFrameworkCore;

namespace CST465_project.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _db;

        public CommentRepository(ApplicationDbContext db)
        {
            _db = db;
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
                var entity = new Comment
                {
                    VisualizationId = comment.VisualizationId,
                    Author = comment.Author,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt
                };

                try { _db.Comments.Add(entity); }
                catch (Exception ex) { throw new Exception("Error adding comment entity to DbSet.", ex); }
                
                await _db.SaveChangesAsync();
                comment.Id = entity.Id;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new Exception("An error occurred while adding the comment.", ex);
            }
        }
    }
}
