using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events; 
using ConnectSphere.Notif.API.Data; 
using ConnectSphere.Notif.API.Entities; 
using MassTransit; 
  
namespace ConnectSphere.Notif.API.Consumers; 
public class NotifFollowRequestedConsumer : 
IConsumer<ConnectSphere.Contracts.Events.Interface.IFollowRequestedEvent> 
{ 
    private readonly NotifDbContext _ctx; 
    public NotifFollowRequestedConsumer(NotifDbContext ctx) => _ctx = ctx; 
  
    public async Task 
Consume(ConsumeContext<ConnectSphere.Contracts.Events.Interface.IFollowRequestedEvent> 
context) 
    { 
        var msg = context.Message; 
        _ctx.Notifications.Add(new Notification 
        { 
            RecipientId = msg.FolloweeId, 
            ActorId = msg.FollowerId, 
            Type = NotifType.FOLLOW_REQUEST, 
            Message = "Someone requested to follow you.", 
            TargetId = msg.FollowerId, 
            TargetType = TargetType.USER 
        }); 
        await _ctx.SaveChangesAsync(); 
    } 
} 