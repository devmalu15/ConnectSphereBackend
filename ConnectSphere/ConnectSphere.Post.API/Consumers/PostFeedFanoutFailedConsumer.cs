using ConnectSphere.Contracts.Events.Interface; 
using ConnectSphere.Post.API.Services; 
using MassTransit; 
  
namespace ConnectSphere.Post.API.Consumers; 
  
public class PostFeedFanoutFailedConsumer : IConsumer<IPostFeedFanoutFailedEvent> 
{ 
    private readonly IPostService _service; 
    public PostFeedFanoutFailedConsumer(IPostService service) => _service = 
service; 
  
    public async Task Consume(ConsumeContext<IPostFeedFanoutFailedEvent> context) 
    { 
        await _service.UpdateDistributionStatusAsync(context.Message.PostId, 
"FAILED"); 
    } 
} 
  
