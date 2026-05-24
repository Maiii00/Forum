using Forum.Models;
using Forum.Service;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    public readonly IPostService _postService;

    public PostController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost(PostRequest postReq)
    {
        var post = await _postService.CreatePostAsync(postReq);
        if (post == null)
        {
            return BadRequest("發文速度太快囉！請稍候一分鐘再試。");
        }

        return Ok(post);
    }

    [HttpPut]
    public async Task<IActionResult> Update(PostRequest postReq)
    {
        var success = await _postService.UpdatePostAsync(postReq);
        return Ok();
    }

    [HttpPost("getPosts")]
    public async Task<IActionResult> GetThread(PostRequest postReq)
    {
        var thread = await _postService.GetThreadAsync(postReq);
        return Ok(thread);
    }

    [HttpPost("mainList")]
    public async Task<IActionResult> GetMain()
    {
        var mainList = await _postService.GetMainListAsync();
        return Ok(mainList);
    }
}
