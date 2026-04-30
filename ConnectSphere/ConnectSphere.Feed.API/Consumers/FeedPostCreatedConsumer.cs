using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Feed.API.Services;
using MassTransit;

namespace ConnectSphere.Feed.API.Consumers;

public class FeedPostCreatedConsumer : IConsumer<IPostCreatedEvent>
{
    private readonly IFeedService _feedService;
    private readonly IPublishEndpoint _bus;
    private readonly IHttpClientFactory _httpFactory;

    public FeedPostCreatedConsumer(IFeedService feedService, IPublishEndpoint bus,
IHttpClientFactory httpFactory)
    {
        _feedService = feedService; _bus = bus; _httpFactory = httpFactory;
    }

    public async Task Consume(ConsumeContext<IPostCreatedEvent> context)
    {
        var msg = context.Message;
        try
        {
            
            var client = _httpFactory.CreateClient("FollowService");
            var response = await client.GetAsync($"api/follows/internal/{msg.UserId}/following-ids");
            
            if (!response.IsSuccessStatusCode) throw new Exception("Could not fetch followers.");

            var followers = await response.Content.ReadFromJsonAsync<FollowerIdsWrapper>();
            if (followers?.Data == null) return;

            var engagementScore = 0m; 
            foreach (var followerId in followers.Data)
            {
                await _feedService.AddToFeedAsync(followerId, msg.PostId,
msg.UserId, engagementScore);
            }

            await _bus.Publish(new
ConnectSphere.Contracts.Events.Implementation.PostFeedFanoutCompletedEvent(msg.PostId));
        }
        catch (Exception ex)
        {
            await _bus.Publish(new
ConnectSphere.Contracts.Events.Implementation.PostFeedFanoutFailedEvent(
                msg.PostId, ex.Message));
        }
    }

    private record FollowerIdsWrapper(IList<int>? Data);
}