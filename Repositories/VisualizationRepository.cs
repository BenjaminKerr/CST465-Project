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
        private readonly ILogger<VisualizationRepository> _logger;

        public VisualizationRepository(ApplicationDbContext db, ILogger<VisualizationRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<IEnumerable<Visualization>> GetAllAsync()
        {
            return await _db.Visualizations.OrderByDescending(v => v.CreatedAt).ToListAsync();
        }

        public async Task<Visualization?> GetByIdAsync(int id)
        {
            return await _db.Visualizations.FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task AddAsync(Visualization v)
        {
            try
            {
                v.CreatedAt = DateTime.UtcNow;
                _db.Visualizations.Add(v);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding visualization {Id}", v.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var viz = await _db.Visualizations.FindAsync(id);
                if (viz != null)
                {
                    _db.Visualizations.Remove(viz);
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting visualization {Id}", id);
                throw;
            }
        }
    }
}