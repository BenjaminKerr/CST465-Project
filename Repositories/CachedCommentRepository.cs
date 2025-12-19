using CST465_project.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CST465_project.Repositories
{
    public class CachedCommentRepository : ICommentRepository
    {
        private readonly CommentRepository _inner;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) };

        public CachedCommentRepository(CommentRepository inner, IMemoryCache cache)
        {
            _inner = inner;
            _cache = cache;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByVisualizationIdAsync(int visualizationId)
        {
            var key = $"comments:v:{visualizationId}";
            if (_cache.TryGetValue<IEnumerable<Comment>>(key, out var cached))
            {
                return cached!;
            }

            var fresh = await _inner.GetCommentsByVisualizationIdAsync(visualizationId);
            _cache.Set(key, fresh.ToList(), _cacheOptions);
            return fresh;
        }

        public async Task AddCommentAsync(Comment comment)
        {
            await _inner.AddCommentAsync(comment);
            var key = $"comments:v:{comment.VisualizationId}";
            _cache.Remove(key);
        }
    }
}
