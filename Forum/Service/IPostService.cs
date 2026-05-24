using Forum.Models;

namespace Forum.Service;

public interface IPostService
{
    Task<Post> CreatePostAsync(PostRequest post);
    Task<PostNode?> GetThreadAsync(PostRequest post);
    Task<List<Post>> GetMainListAsync();
    Task<bool> UpdatePostAsync(PostRequest post);
}
