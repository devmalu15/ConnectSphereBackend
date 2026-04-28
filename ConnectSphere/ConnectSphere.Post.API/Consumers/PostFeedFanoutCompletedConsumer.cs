using ConnectSphere.Contracts.Events.Interface; 
using ConnectSphere.Post.API.Services; 
using MassTransit; 
  
namespace ConnectSphere.Post.API.Consumers; 
  
public class PostFeedFanoutCompletedConsumer : IConsumer<IPostFeedFanoutCompletedEvent> 
{ 
    private readonly IPostService _service; 
    public PostFeedFanoutCompletedConsumer(IPostService service) => _service = service; 
  
    public async Task Consume(ConsumeContext<IPostFeedFanoutCompletedEvent> context) 
    { 
        await _service.UpdateDistributionStatusAsync(context.Message.PostId, "COMPLETE"); 
    } 
} 
