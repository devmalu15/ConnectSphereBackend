using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Post.API.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ConnectSphere.Post.API.Consumers;

public class PostCommentAddedConsumer : IConsumer<ICommentAddedEvent>
{
    private readonly PostDbContext _ctx;
    public PostCommentAddedConsumer(PostDbContext ctx) => _ctx = ctx;

    public async Task Consume(ConsumeContext<ICommentAddedEvent> context)
    {
        // Every comment and reply counts toward the post's total CommentCount
        await _ctx.Posts
            .Where(p => p.PostId == context.Message.PostId)
            .ExecuteUpdateAsync(s => s.SetProperty(
                p => p.CommentCount,
                p => p.CommentCount + 1));
    }
}