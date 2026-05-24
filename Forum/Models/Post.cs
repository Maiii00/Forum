using System.ComponentModel.DataAnnotations;

namespace Forum.Models;

public class Post
{
    public int Id { get; set; }
    public string? Title { get; set; }
    [Required]
    public string Content { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
}
