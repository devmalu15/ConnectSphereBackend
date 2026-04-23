using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Contracts.Enums;
using ConnectSphere.Comment.API.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ConnectSphere.Comment.API.Consumers;

public class CommentLikeToggledConsumer : IConsumer<ILikeToggledEvent>
{
    private readonly CommentDbContext _ctx;
    public CommentLikeToggledConsumer(CommentDbContext ctx) => _ctx = ctx;

    public async Task Consume(ConsumeContext<ILikeToggledEvent> context)
    {
        // Only proceed if the like target is a COMMENT
        if (context.Message.TargetType != TargetType.COMMENT) return;

        // Note: Ensure your ILikeToggledEvent interface has the 'IsLiked' property
        int delta = context.Message.IsLiked ? 1 : -1;

        await _ctx.Comments
            .Where(c => c.CommentId == context.Message.TargetId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.LikeCount, c => c.LikeCount + delta));
    }
}