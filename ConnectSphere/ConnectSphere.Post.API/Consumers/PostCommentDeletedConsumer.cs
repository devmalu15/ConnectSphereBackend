using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Post.API.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ConnectSphere.Post.API.Consumers;

public class PostCommentDeletedConsumer : IConsumer<ICommentDeletedEvent>
{
    private readonly PostDbContext _ctx;
    public PostCommentDeletedConsumer(PostDbContext ctx) => _ctx = ctx;

    public async Task Consume(ConsumeContext<ICommentDeletedEvent> context)
    {
        var deletedCount = context.Message.DeletedCount;

        await _ctx.Posts
            .Where(p => p.PostId == context.Message.PostId)
            .ExecuteUpdateAsync(s => s.SetProperty(
                p => p.CommentCount,
                p => p.CommentCount >= deletedCount
                    ? p.CommentCount - deletedCount
                    : 0));
    }
}