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
    
    // Call Post API to find out who owns the post
    var client = _httpFactory.CreateClient("PostService");
    var response = await client.GetAsync($"api/posts/{msg.PostId}");
    
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PostDto>>();
        var postAuthorId = result?.Data?.UserId ?? 0;

        _ctx.Notifications.Add(new Notification 
        { 
            RecipientId = postAuthorId, // Now you have the real ID!
            ActorId = msg.UserId,
            // ... rest of the code
        });
        await _ctx.SaveChangesAsync();
    }
}
} 