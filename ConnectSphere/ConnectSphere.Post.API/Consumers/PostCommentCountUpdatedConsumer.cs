using ConnectSphere.Contracts.Events.Interface; 
using ConnectSphere.Post.API.Services; 
using MassTransit; 
  
namespace ConnectSphere.Post.API.Consumers; 
  
public class PostCommentCountUpdatedConsumer : 
IConsumer<ICommentAddedEvent> 
{ 
    private readonly IPostService _service; 
    public PostCommentCountUpdatedConsumer(IPostService service) => _service = 
service; 
  
    public async Task Consume(ConsumeContext<ICommentAddedEvent> context) 
    { 
        await _service.IncrementCommentCountAsync(context.Message.PostId); 
    } 
} 