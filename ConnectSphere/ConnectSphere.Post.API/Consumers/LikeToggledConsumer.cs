using ConnectSphere.Contracts.Events.Interface; // Use your interface from the screenshot
using ConnectSphere.Contracts.Enums;
using ConnectSphere.Post.API.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ConnectSphere.Post.API.Consumers;

public class LikeToggledConsumer : IConsumer<ILikeToggledEvent>
{
    private readonly PostDbContext _ctx;
    public LikeToggledConsumer(PostDbContext ctx) => _ctx = ctx;

    public async Task Consume(ConsumeContext<ILikeToggledEvent> context)
    {
        // We only care about likes for POSTS in this service
        if (context.Message.TargetType != TargetType.POST) return;

        // Note: You might need to add 'IsLiked' to your ILikeToggledEvent 
        // to know if we are adding (+1) or removing (-1) a like.
        int delta = 1; // For now, let's assume it's a new like

        await _ctx.Posts
            .Where(p => p.PostId == context.Message.TargetId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.LikeCount, p => p.LikeCount + delta));
    }
}