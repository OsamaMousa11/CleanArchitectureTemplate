using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CleanArchitectureTemplate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RedisController : ControllerBase
{
    private readonly IDistributedCache _cache;

    public RedisController(IDistributedCache cache)
    {
        _cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> Test()
    {
        var value = await _cache.GetStringAsync("username");

        if (value == null)
        {
            value = $"Osama - {DateTime.Now}";
            await _cache.SetStringAsync("username", value);
        }

        return Ok(value);
    }
}