namespace Forum.Client.Models;

public class PostNode
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PostNode> Replies { get; set; } = new();
}
