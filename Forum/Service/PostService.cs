using Forum.DbContext;
using Forum.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Forum.Service;

public class PostService : IPostService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDistributedCache _cache;
    public PostService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IDistributedCache cache)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
    }
    public async Task<Post> CreatePostAsync(PostRequest postReq)
    {
        var ipAddress = GetClientIpAddress();
        if (await IsInCooldownAsync(ipAddress))
        {
            return null;
        }

        Post post = new Post
        {
            Content = postReq.Content,
            Title = postReq.Title,
            ParentId = postReq.ParentId,
            CreatedAt = DateTime.UtcNow,
            IpAddress = ipAddress
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        await SetCooldownAsync(ipAddress);
        return post;
    }

    public async Task<PostNode?> GetThreadAsync(PostRequest postReq)
    {
        var rootPost = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postReq.Id);
        if (rootPost == null) return null;

        var rawSql = @"
            WITH PostTree AS (
                SELECT * FROM [Posts] WHERE [Id] = {0}
                UNION ALL
                SELECT p.* FROM [Posts] p
                INNER JOIN PostTree pt ON p.[ParentId] = pt.[Id]
            )
            SELECT * FROM PostTree ORDER BY [CreatedAt] ASC;";

        var relatedPosts = await _context.Posts
            .FromSqlRaw(rawSql, postReq.Id)
            .AsNoTracking()
            .ToListAsync();

        return MapToTree(rootPost, relatedPosts);
    }

    public async Task<List<Post>> GetMainListAsync()
    {
        return await _context.Posts
            .Where(p => p.ParentId == null)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> UpdatePostAsync(PostRequest postReq)
    {
        Post post = await _context.Posts.FindAsync(postReq.Id);
        post.Content = postReq.Content;
        post.Title = postReq.Title;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> IsInCooldownAsync(string ipAddress)
    {
        string cacheKey = $"cooldown:{ipAddress}";
        var cooldownData = await _cache.GetStringAsync(cacheKey);

        return cooldownData != null;
    }

    private async Task SetCooldownAsync(string ipAddress)
    {
        string cacheKey = $"cooldown:{ipAddress}";

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
        };

        await _cache.SetStringAsync(cacheKey, "blocked", cacheOptions);
    }

    private string GetClientIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private PostNode MapToTree(Post current, List<Post> allPosts)
    {
        return new PostNode
        {
            Id = current.Id,
            Title = current.Title,
            Content = current.Content,
            ParentId = current.ParentId,
            CreatedAt = current.CreatedAt,
            Replies = allPosts
            .Where(p => p.ParentId == current.Id)
            .Select(p => MapToTree(p, allPosts))
            .ToList()
        };
    }
}
