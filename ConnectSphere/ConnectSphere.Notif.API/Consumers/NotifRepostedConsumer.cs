using ConnectSphere.Contracts.Enums;
using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Notif.API.Data;
using ConnectSphere.Notif.API.Entities;
using MassTransit;

namespace ConnectSphere.Notif.API.Consumers;

public class NotifRepostedConsumer : IConsumer<IPostRepostedEvent>
{
    private readonly NotifDbContext _ctx;

    public NotifRepostedConsumer(NotifDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Consume(ConsumeContext<IPostRepostedEvent> context)
    {
        var msg = context.Message;

        // Don't notify if the user reposts their own post
        if (msg.ReposterId == msg.OriginalAuthorId) return;

        _ctx.Notifications.Add(new Notification
        {
            RecipientId = msg.OriginalAuthorId,
            ActorId = msg.ReposterId,
            Type = NotifType.REPOST,
            Message = "Someone reposted your post.",
            TargetId = msg.NewPostId,
            TargetType = TargetType.POST,
            CreatedAt = DateTime.UtcNow
        });

        await _ctx.SaveChangesAsync();
    }
}
