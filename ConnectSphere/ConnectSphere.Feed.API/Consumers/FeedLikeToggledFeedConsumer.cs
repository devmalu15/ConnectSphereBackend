using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Feed.API.Services;
using MassTransit;

namespace ConnectSphere.Feed.API.Consumers;




public class FeedLikeToggledFeedConsumer : IConsumer<ILikeToggledEvent> 
{ 
    private readonly IFeedService _feedService; 
    public FeedLikeToggledFeedConsumer(IFeedService feedService) => _feedService = 
feedService; 
  
    public async Task Consume(ConsumeContext<ILikeToggledEvent> context) 
    { 
        var msg = context.Message; 
        if (!msg.IsLiked) return; // Only learn on like, not unlike 
        if (msg.TargetType != ConnectSphere.Contracts.Enums.TargetType.POST) 
return; 
        await _feedService.UpdateTagPreferencesOnLikeAsync(msg.UserId, 
msg.TargetId); 
    } 
}