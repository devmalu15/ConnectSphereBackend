using ConnectSphere.Contracts.Enums;
using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Contracts.DTOs;
using ConnectSphere.Notif.API.Data;
using ConnectSphere.Notif.API.Entities;
using MassTransit;
using System.Net.Http.Json;

namespace ConnectSphere.Notif.API.Consumers;

public class NotifLikeToggledConsumer : IConsumer<ILikeToggledEvent>
{
    private readonly NotifDbContext _ctx;
    private readonly IHttpClientFactory _httpFactory;

    public NotifLikeToggledConsumer(NotifDbContext ctx, IHttpClientFactory httpFactory)
    {
        _ctx = ctx;
        _httpFactory = httpFactory;
    }

    public async Task Consume(ConsumeContext<ILikeToggledEvent> context)
    {
        var msg = context.Message;
        if (!msg.IsLiked) return; 

        // 1. Resolve the real author ID via HTTP
        int recipientId = await GetTargetAuthorId(msg.TargetId, msg.TargetType);
        
        if (recipientId == 0) return; // Don't save if we can't find the author

        var typeMap = msg.TargetType == TargetType.POST ? NotifType.LIKE_POST : NotifType.LIKE_COMMENT;
        
        _ctx.Notifications.Add(new Notification
        {
            RecipientId = recipientId,
            ActorId = msg.UserId,
            Type = typeMap,
            Message = $"Someone liked your {msg.TargetType.ToString().ToLower()}.",
            TargetId = msg.TargetId,
            TargetType = msg.TargetType
        });
        
        await _ctx.SaveChangesAsync();
    }

    private async Task<int> GetTargetAuthorId(int targetId, TargetType type)
    {
        try 
        {
            // Determine which service to call based on the target type
            string serviceName = type == TargetType.POST ? "PostService" : "CommentService";
            string endpoint = type == TargetType.POST ? $"api/posts/{targetId}" : $"api/comments/{targetId}";
            
            var client = _httpFactory.CreateClient(serviceName);
            var response = await client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                // Note: Ensure your Contracts project is referenced for ApiResponse and DTOs
                if (type == TargetType.POST) {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<PostDto>>();
                    return result?.Data?.UserId ?? 0;
                } else {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<CommentDto>>();
                    return result?.Data?.UserId ?? 0;
                }
            }
        }
        catch { /* Log error */ }
        return 0;
    }
}