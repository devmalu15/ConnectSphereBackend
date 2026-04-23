using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Comment.API.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ConnectSphere.Comment.API.Consumers;

public class ComCommentAddedConsumer : IConsumer<ICommentAddedEvent>
{
    private readonly CommentDbContext _ctx;
    public ComCommentAddedConsumer(CommentDbContext ctx) => _ctx = ctx;

    public async Task Consume(ConsumeContext<ICommentAddedEvent> context)
    {
        var message = context.Message;

        // If this comment is a reply, increment the parent's ReplyCount
        if (message.ParentCommentId.HasValue)
        {
            await _ctx.Comments
                .Where(c => c.CommentId == message.ParentCommentId.Value)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.ReplyCount, c => c.ReplyCount + 1));
        }
    }
}