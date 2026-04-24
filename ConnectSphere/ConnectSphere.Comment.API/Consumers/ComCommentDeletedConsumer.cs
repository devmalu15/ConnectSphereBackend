using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Comment.API.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ConnectSphere.Comment.API.Consumers;

public class ComCommentDeletedConsumer : IConsumer<ICommentDeletedEvent>
{
    private readonly CommentDbContext _ctx;
    public ComCommentDeletedConsumer(CommentDbContext ctx) => _ctx = ctx;

    public async Task Consume(ConsumeContext<ICommentDeletedEvent> context)
    {
        var message = context.Message;

    
        var comment = await _ctx.Comments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.CommentId == message.CommentId);

        if (comment != null && comment.ParentCommentId.HasValue)
        {
            await _ctx.Comments
                .Where(c => c.CommentId == comment.ParentCommentId.Value)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    c => c.ReplyCount,
                    c => c.ReplyCount > 0 ? c.ReplyCount - 1 : 0));
        }
    }
}