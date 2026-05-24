namespace Forum.Models;

public class PostRequest
{
    public int? Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }

    public int? ParentId { get; set; }
}
