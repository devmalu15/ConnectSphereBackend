using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Feed.API.Services;
using MassTransit;

namespace ConnectSphere.Feed.API.Consumers;




public class FeedPostDeletedFeedConsumer : IConsumer<IPostDeletedEvent> 
{ 
    private readonly IFeedService _feedService; 
    public FeedPostDeletedFeedConsumer(IFeedService feedService) => _feedService = 
feedService; 
  
    public async Task Consume(ConsumeContext<IPostDeletedEvent> context) => 
        await _feedService.RemoveFromFeedsAsync(context.Message.PostId); 
} 