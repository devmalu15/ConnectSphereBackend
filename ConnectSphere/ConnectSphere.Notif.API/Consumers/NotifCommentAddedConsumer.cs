using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events; 
using ConnectSphere.Notif.API.Data; 
using ConnectSphere.Notif.API.Entities; 
using MassTransit; 
using ConnectSphere.Contracts.DTOs; 
using System.Net.Http.Json;         
  
namespace ConnectSphere.Notif.API.Consumers; 



public class NotifCommentAddedConsumer : 
IConsumer<ConnectSphere.Contracts.Events.Interface.ICommentAddedEvent> 
{ 
    private readonly NotifDbContext _ctx; 
    private readonly IHttpClientFactory _httpFactory;
    public NotifCommentAddedConsumer(NotifDbContext ctx, IHttpClientFactory httpFactory) 
{ 
    _ctx = ctx; 
    _httpFactory = httpFactory; 
}
  
    public async Task Consume(ConsumeContext<ConnectSphere.Contracts.Events.Interface.ICommentAddedEvent> context)
    {
        var msg = context.Message;
        int recipientId = 0;
        NotifType type = NotifType.NEW_COMMENT;
        string message = "";

        try
        {
            if (msg.ParentCommentId == null || msg.ParentCommentId == 0)
            {
                
                var client = _httpFactory.CreateClient("PostService");
                var response = await client.GetAsync($"api/posts/{msg.PostId}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<PostDto>>();
                    recipientId = result?.Data?.UserId ?? 0;
                }
                type = NotifType.NEW_COMMENT;
                message = "Someone commented on your post.";
            }
            else
            {
                
                var client = _httpFactory.CreateClient("CommentService");
                var response = await client.GetAsync($"api/comments/{msg.ParentCommentId}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<CommentDto>>();
                    recipientId = result?.Data?.UserId ?? 0;
                }
                type = NotifType.NEW_REPLY;
                message = "Someone replied to your comment.";
            }

            if (recipientId != 0 && recipientId != msg.UserId)
            {
                _ctx.Notifications.Add(new Notification
                {
                    RecipientId = recipientId,
                    ActorId = msg.UserId,
                    Type = type,
                    Message = message,
                    TargetId = msg.CommentId,
                    TargetType = TargetType.COMMENT,
                    CreatedAt = DateTime.UtcNow
                });
                await _ctx.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            
        }
    }
}