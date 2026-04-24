using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Comment.API.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ConnectSphere.Comment.API.Consumers;

public class PostDeletedConsumer : IConsumer<IPostDeletedEvent>
{
    private readonly CommentDbContext _ctx;
    public PostDeletedConsumer(CommentDbContext ctx) => _ctx = ctx;

    public async Task Consume(ConsumeContext<IPostDeletedEvent> context)
    {
        
        await _ctx.Comments
            .Where(c => c.PostId == context.Message.PostId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsDeleted, true));
    }
}