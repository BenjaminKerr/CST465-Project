using CST465_project.Data;
using CST465_project.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CST465_project.Repositories
{
    public class VisualizationRepository : IVisualizationRepository
    {
        private readonly ApplicationDbContext _db;

        public VisualizationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Visualization>> GetAllAsync()
        {
            return await _db.Visualizations.OrderByDescending(v => v.CreatedAt).ToListAsync();
        }

        public async Task<Visualization?> GetByIdAsync(int id)
        {
            return await _db.Visualizations.FindAsync(id);
        }

        public async Task AddAsync(Visualization v)
        {
            v.CreatedAt = DateTime.UtcNow;
            _db.Visualizations.Add(v);
            await _db.SaveChangesAsync();
        }
    }
}
