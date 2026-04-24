using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Contracts.Enums;
using ConnectSphere.Post.API.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ConnectSphere.Post.API.Consumers;

public class PostLikeToggledConsumer : IConsumer<ILikeToggledEvent>
{
    private readonly PostDbContext _ctx;
    public PostLikeToggledConsumer(PostDbContext ctx) => _ctx = ctx;

    public async Task Consume(ConsumeContext<ILikeToggledEvent> context)
{

    Console.WriteLine("reeached the consumer");
    if (context.Message.TargetType != TargetType.POST) return;

    Console.WriteLine("Passedtypecheck");

    var postId = context.Message.TargetId;
    bool isLiked = context.Message.IsLiked;

    Console.WriteLine($"[CONSUMER] Post: {postId}, Action: {(isLiked ? "LIKE" : "UNLIKE")}");

    
    await _ctx.Posts
        .Where(p => p.PostId == postId)
        .ExecuteUpdateAsync(s => s.SetProperty(
            p => p.LikeCount,
            p => isLiked 
                ? p.LikeCount + 1 
                : (p.LikeCount > 0 ? p.LikeCount - 1 : 0)
        ));
}
}