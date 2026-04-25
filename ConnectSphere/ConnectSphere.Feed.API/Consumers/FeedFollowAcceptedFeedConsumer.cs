using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Feed.API.Services;
using MassTransit;

namespace ConnectSphere.Feed.API.Consumers;




public class FeedFollowAcceptedFeedConsumer : IConsumer<IFollowAcceptedEvent> 
{ 
    private readonly IFeedService _feedService; 
    public FeedFollowAcceptedFeedConsumer(IFeedService feedService) => _feedService = 
feedService; 
  
    public async Task Consume(ConsumeContext<IFollowAcceptedEvent> context) 
    { 
        var msg = context.Message; 
        await _feedService.BackfillFeedAsync(msg.FollowerId, msg.FolloweeId); 
    } 
} 