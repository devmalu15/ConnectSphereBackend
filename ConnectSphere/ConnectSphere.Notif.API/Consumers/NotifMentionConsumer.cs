using ConnectSphere.Contracts.Enums;
using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Notif.API.Data;
using ConnectSphere.Notif.API.Entities;
using MassTransit;

namespace ConnectSphere.Notif.API.Consumers;

public class NotifMentionConsumer : IConsumer<IMentionEvent>
{
    private readonly NotifDbContext _ctx;

    public NotifMentionConsumer(NotifDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task Consume(ConsumeContext<IMentionEvent> context)
    {
        var msg = context.Message;

        _ctx.Notifications.Add(new Notification
        {
            RecipientId = msg.MentionedUserId,
            ActorId = msg.ActorId,
            Type = NotifType.MENTION,
            Message = $"Someone mentioned you in a {msg.TargetType.ToString().ToLower()}.",
            TargetId = msg.TargetId,
            TargetType = msg.TargetType,
            CreatedAt = DateTime.UtcNow
        });

        await _ctx.SaveChangesAsync();
    }
}
