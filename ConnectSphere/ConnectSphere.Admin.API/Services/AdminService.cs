using ConnectSphere.Admin.API.Data;
using ConnectSphere.Admin.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConnectSphere.Admin.API.Services;

public class AdminService : IAdminService
{
    private readonly AdminDbContext _ctx;
    private readonly IHttpClientFactory _httpFactory;

    public AdminService(AdminDbContext ctx, IHttpClientFactory httpFactory)
    {
        _ctx = ctx; _httpFactory = httpFactory;
    }

    public async Task SuspendUserAsync(int adminId, string adminName, int userId, string ip, string token)
{
    var before = await FetchUserJsonAsync(userId); // internal — no auth needed

    var client = _httpFactory.CreateClient("AuthService");
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var response = await client.PutAsync($"api/users/{userId}/suspend", null);
    response.EnsureSuccessStatusCode();

    await WriteAuditLog(adminId, adminName, "SuspendUser", "User",
        userId.ToString(), before, "IsActive: false", ip);
}
    public async Task UnsuspendUserAsync(int adminId, string adminName, int userId, string ip)
    {
        var before = await FetchUserJsonAsync(userId);

        var client = _httpFactory.CreateClient("AuthService");
        // Call the reactivate endpoint that now actually exists
        var response = await client.PutAsync($"api/users/{userId}/reactivate", null);
        response.EnsureSuccessStatusCode();

        var after = await FetchUserJsonAsync(userId);

        await WriteAuditLog(adminId, adminName, "UnsuspendUser", "User",
            userId.ToString(), before, after, ip);
    }

    public async Task DeletePostAsync(int adminId, string adminName, int postId, string ip)
    {
        // Use internal endpoint — no auth needed for service-to-service
        var client = _httpFactory.CreateClient("PostService");
        var before = await client.GetStringAsync($"api/posts/{postId}/internal");

        var response = await client.DeleteAsync($"api/posts/{postId}/internal");
        response.EnsureSuccessStatusCode();

        await WriteAuditLog(adminId, adminName, "DeletePost", "Post",
            postId.ToString(), before, "IsDeleted: true", ip);
    }

    public async Task DeleteCommentAsync(int adminId, string adminName, int commentId, string ip)
    {
        var client = _httpFactory.CreateClient("CommentService");
        var response = await client.DeleteAsync($"api/comments/{commentId}/internal");
        response.EnsureSuccessStatusCode();

        await WriteAuditLog(adminId, adminName, "DeleteComment", "Comment",
            commentId.ToString(), null, "IsDeleted: true", ip);
    }

    public async Task<IList<AuditLog>> GetAuditLogsAsync(
        DateTime? from, DateTime? to, int page, int pageSize)
    {
        var query = _ctx.AuditLogs.AsQueryable();
        if (from.HasValue) query = query.Where(a => a.CreatedAt >= from.Value);
        if (to.HasValue)   query = query.Where(a => a.CreatedAt <= to.Value);

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    private async Task WriteAuditLog(int actorId, string actorName, string action,
        string entityType, string entityId, string? before, string? after, string ip)
    {
        _ctx.AuditLogs.Add(new AuditLog
        {
            ActorId = actorId, ActorUserName = actorName, Action = action,
            EntityType = entityType, EntityId = entityId,
            BeforeValue = before, AfterValue = after, IpAddress = ip,
            CreatedAt = DateTime.UtcNow
        });
        await _ctx.SaveChangesAsync();
    }

    // Uses the /internal endpoint — no auth token needed
    private async Task<string> FetchUserJsonAsync(int userId)
    {
        var client = _httpFactory.CreateClient("AuthService");
        return await client.GetStringAsync($"api/users/{userId}/internal");
    }

    public async Task<object> GetAnalyticsAsync(string token)
{
    var authClient = _httpFactory.CreateClient("AuthService");
    authClient.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    var feedClient = _httpFactory.CreateClient("FeedService");
    // FeedService's trending endpoint — check if it also needs auth

    var userCountStr = await authClient.GetStringAsync("api/users/count");
    var trendingStr = await feedClient.GetStringAsync("api/feed/trending-hashtags?topN=5");

    return new { userCount = userCountStr, trending = trendingStr };
}
}