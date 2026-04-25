using ConnectSphere.Contracts.DTOs;
using ConnectSphere.Contracts.Events.Implementation;
using ConnectSphere.Notif.API.Data;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ConnectSphere.Notif.API.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotifController : ControllerBase
{
    private readonly NotifDbContext _ctx;
    private readonly IPublishEndpoint _bus;
    public NotifController(NotifDbContext ctx, IPublishEndpoint bus)
    { _ctx = ctx; _bus = bus; }
    private int CurrentUserId => int.Parse(
        User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ??
        User.FindFirstValue("sub") ??
        "0"
    );

    [HttpGet("{userId:int}")]
    [Authorize]
    public async Task<IActionResult> GetAll(int userId, [FromQuery] int page = 1,
[FromQuery] int pageSize = 20)
    {
        if (CurrentUserId != userId) return Forbid();
        var query = _ctx.Notifications.Where(n => n.RecipientId ==
userId).OrderByDescending(n => n.CreatedAt);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var dtos = items.Select(n => new NotificationDto(n.NotificationId,
n.RecipientId, n.ActorId, n.Type, n.Message, n.TargetId, n.TargetType, n.IsRead,
n.CreatedAt)).ToList();
        return Ok(ApiResponse<PagedResult<NotificationDto>>.Ok(new
PagedResult<NotificationDto>(dtos, page, pageSize, total)));
    }

    [HttpGet("{userId:int}/unread-count")]
    [Authorize]
    public async Task<IActionResult> UnreadCount(int userId)
    {
        if (CurrentUserId != userId) return Forbid();
        var count = await _ctx.Notifications.CountAsync(n => n.RecipientId ==
userId && !n.IsRead);
        return Ok(ApiResponse<int>.Ok(count));
    }

    [HttpPut("{id:int}/read")]
    [Authorize]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _ctx.Notifications.Where(n => n.NotificationId == id && n.RecipientId
== CurrentUserId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        return Ok(ApiResponse<string>.Ok("Marked as read."));
    }

    [HttpPut("{userId:int}/read-all")]
    [Authorize]
    public async Task<IActionResult> MarkAllRead(int userId)
    {
        if (CurrentUserId != userId) return Forbid();
        await _ctx.Notifications.Where(n => n.RecipientId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        return Ok(ApiResponse<string>.Ok("All marked as read."));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        await _ctx.Notifications.Where(n => n.NotificationId == id && n.RecipientId
== CurrentUserId)
            .ExecuteDeleteAsync();
        return Ok(ApiResponse<string>.Ok("Deleted."));
    }

    [HttpPost("broadcast")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Broadcast([FromBody] BroadcastDto dto)
    {
        await _bus.Publish(new BroadcastNotifEvent(dto.Title, dto.Message,
dto.UserIds, dto.Type));
        return Ok(ApiResponse<string>.Ok("Broadcast queued."));
    }
}

public record BroadcastDto(string Title, string Message, IList<int> UserIds,
ConnectSphere.Contracts.Enums.NotifType Type);