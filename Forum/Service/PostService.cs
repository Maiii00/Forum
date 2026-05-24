using Forum.DbContext;
using Forum.Models;
using Microsoft.EntityFrameworkCore;

namespace Forum.Service;

public class PostService : IPostService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public PostService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
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
        return post;
    }

    public async Task<PostNode?> GetThreadAsync(PostRequest postReq)
    {
        var allPosts = await _context.Posts.ToListAsync();
        var rootPost = allPosts.FirstOrDefault(p => p.Id == postReq.Id);
        if (rootPost == null) return null;
        return MapToTree(rootPost, allPosts);
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
        var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);

        return await _context.Posts
            .AnyAsync(p => p.IpAddress == ipAddress && p.CreatedAt > oneMinuteAgo);
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
