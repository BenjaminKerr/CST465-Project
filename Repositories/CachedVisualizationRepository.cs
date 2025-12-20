using CST465_project.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CST465_project.Repositories
{
    public class CachedVisualizationRepository : IVisualizationRepository
    {
        private readonly VisualizationRepository _inner;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) };

        public CachedVisualizationRepository(VisualizationRepository inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public async Task<IEnumerable<Visualization>> GetAllAsync()
        {
            const string key = "visualizations:all";
            if (_cache.TryGetValue<IEnumerable<Visualization>>(key, out var cached)) return cached!;
            var fresh = await _inner.GetAllAsync();
            _cache.Set(key, fresh.ToList(), _options);
            return fresh;
        }

        public Task<Visualization?> GetByIdAsync(int id)
        {
            return _inner.GetByIdAsync(id);
        }

        public async Task AddAsync(Visualization v)
        {
            await _inner.AddAsync(v);
            _cache.Remove("visualizations:all");
        }

        public Task DeleteAsync(int id)
        {
            return _inner.DeleteAsync(id);
        }
    }
}
