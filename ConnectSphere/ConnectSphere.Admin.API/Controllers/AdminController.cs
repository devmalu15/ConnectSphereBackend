using ConnectSphere.Admin.API.Services; 
using ConnectSphere.Contracts.DTOs; 
using ConnectSphere.Contracts.Events.Implementation; 
using ConnectSphere.Contracts.Enums; 
using MassTransit; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc; 
using System.Security.Claims; 
  
namespace ConnectSphere.Admin.API.Controllers; 
  
[ApiController] 
[Route("api/admin")] 
[Authorize(Roles = "Admin")] 
public class AdminController : ControllerBase 
{ 
    private readonly IAdminService _service; 
    private readonly IHttpClientFactory _httpFactory; 
    private readonly IPublishEndpoint _bus; 
  
    
private readonly IHttpContextAccessor _httpContextAccessor;

public AdminController(IAdminService service, IHttpClientFactory httpFactory,
    IPublishEndpoint bus, IHttpContextAccessor httpContextAccessor)
{
    _service = service; _httpFactory = httpFactory;
    _bus = bus; _httpContextAccessor = httpContextAccessor;
}


private string? BearerToken => _httpContextAccessor.HttpContext?
    .Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
  
    private int CurrentUserId => int.Parse(
    User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? 
    User.FindFirstValue("sub") ?? 
    "0"
);
    private string CurrentUserName => User.FindFirstValue("username") ?? "Admin"; 
    private string ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString() 
?? "unknown"; 
  
    [HttpGet("users")] 
public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20) 
{ 
    var client = _httpFactory.CreateClient("AuthService"); 
    
    
    
    var response = await client.GetAsync("api/users/search?q="); 

    if (!response.IsSuccessStatusCode)
    {
        var error = await response.Content.ReadAsStringAsync();
        return StatusCode((int)response.StatusCode, error);
    }

    var result = await response.Content.ReadAsStringAsync();
    
    
    
    
    
    return Content(result, "application/json"); 
}
  
    [HttpPut("users/{id:int}/suspend")] 
    public async Task<IActionResult> Suspend(int id) 
    { 
        await _service.SuspendUserAsync(CurrentUserId, CurrentUserName, id, 
ClientIp, BearerToken); 
        return Ok(ApiResponse<string>.Ok("User suspended.")); 
    } 
  
    [HttpDelete("posts/{id:int}")] 
    public async Task<IActionResult> DeletePost(int id) 
    { 
        await _service.DeletePostAsync(CurrentUserId, CurrentUserName, id, 
ClientIp); 
        return Ok(ApiResponse<string>.Ok("Post deleted.")); 
    } 
  
    [HttpDelete("comments/{id:int}")] 
    public async Task<IActionResult> DeleteComment(int id) 
    { 
        await _service.DeleteCommentAsync(CurrentUserId, CurrentUserName, id, 
ClientIp); 
        return Ok(ApiResponse<string>.Ok("Comment deleted.")); 
    } 
  
    [HttpGet("analytics")]
public async Task<IActionResult> GetAnalytics()
{
    
    var result = await _service.GetAnalyticsAsync(BearerToken!);
    return Ok(ApiResponse<object>.Ok(result));
}
  
    [HttpPost("notifications/broadcast")] 
    public async Task<IActionResult> Broadcast([FromBody] BroadcastRequestDto dto) 
    { 
        if (!Enum.TryParse<NotifType>(dto.Type, true, out var notifType))
        {
            return BadRequest(ApiResponse<string>.Fail($"Invalid type: {dto.Type}"));
        }

        await _bus.Publish(new BroadcastNotifEvent(dto.Title, dto.Message, 
dto.UserIds, notifType)); 
        return Ok(ApiResponse<string>.Ok("Broadcast queued.")); 
    } 
  
    [HttpGet("audit-logs")] 
    public async Task<IActionResult> GetAuditLogs([FromQuery] DateTime? from, 
[FromQuery] DateTime? to, 
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) 
    { 
        var logs = await _service.GetAuditLogsAsync(from, to, page, pageSize); 
        return Ok(ApiResponse<object>.Ok(logs)); 
    } 
} 
  
public record BroadcastRequestDto(string Title, string Message, IList<int> UserIds, 
string Type); 