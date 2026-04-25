using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Feed.API.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ConnectSphere.Feed.API.Consumers;

public class FeedUnfollowedConsumer : IConsumer<IUnfollowedEvent>
{
    private readonly FeedDbContext _ctx;
    private readonly ILogger<FeedUnfollowedConsumer> _logger;

    public FeedUnfollowedConsumer(FeedDbContext ctx, ILogger<FeedUnfollowedConsumer> logger)
    {
        _ctx = ctx; _logger = logger;
    }

    public async Task Consume(ConsumeContext<IUnfollowedEvent> context)
    {
        var followerId = context.Message.FollowerId;  // the person who unfollowed
        var followeeId = context.Message.FolloweeId;  // the person being unfollowed

        // Remove all posts authored by followeeId from followerId's feed
        var deleted = await _ctx.FeedItems
            .Where(f => f.UserId == followerId && f.ActorId == followeeId)
            .ExecuteDeleteAsync();

        _logger.LogInformation(
            "Unfollow cleanup: removed {Count} feed items from user {FollowerId}'s feed (unfollowed {FolloweeId})",
            deleted, followerId, followeeId);
    }
}