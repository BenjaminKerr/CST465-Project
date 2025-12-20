using CST465_project.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CST465_project.Repositories
{
    public interface IVisualizationRepository
    {
        Task<IEnumerable<Visualization>> GetAllAsync();
        Task<Visualization?> GetByIdAsync(int id);
        Task AddAsync(Visualization v);
        Task DeleteAsync(int id);
    }
}
