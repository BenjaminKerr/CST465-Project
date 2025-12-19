using CST465_project.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CST465_project.Repositories
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetCommentsByVisualizationIdAsync(int visualizationId);
        Task AddCommentAsync(Comment comment);
    }
}