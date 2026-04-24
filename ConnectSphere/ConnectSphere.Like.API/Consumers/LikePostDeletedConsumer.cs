using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Like.API.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ConnectSphere.Like.API.Consumers;

// When a post is deleted, cascade-delete all likes on that post
// so orphaned Like rows don't accumulate.
public class LikePostDeletedConsumer : IConsumer<IPostDeletedEvent>
{
    private readonly LikeDbContext _ctx;
    public LikePostDeletedConsumer(LikeDbContext ctx) => _ctx = ctx;

    public async Task Consume(ConsumeContext<IPostDeletedEvent> context)
    {
        var postId = context.Message.PostId;

        await _ctx.Likes
            .Where(l => l.TargetId == postId &&
                        l.TargetType == ConnectSphere.Contracts.Enums.TargetType.POST)
            .ExecuteDeleteAsync();
    }
}