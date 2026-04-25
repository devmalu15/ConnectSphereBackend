using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events; 
using ConnectSphere.Notif.API.Data; 
using ConnectSphere.Notif.API.Entities; 
using MassTransit; 
  
namespace ConnectSphere.Notif.API.Consumers;



public class NotifFollowAcceptedConsumer : 
IConsumer<ConnectSphere.Contracts.Events.Interface.IFollowAcceptedEvent> 
{ 
    private readonly NotifDbContext _ctx; 
    public NotifFollowAcceptedConsumer(NotifDbContext ctx) => _ctx = ctx; 
  
    public async Task 
Consume(ConsumeContext<ConnectSphere.Contracts.Events.Interface.IFollowAcceptedEvent> 
context) 
    { 
        var msg = context.Message; 
        _ctx.Notifications.Add(new Notification 
        { 
            RecipientId = msg.FollowerId, 
            ActorId = msg.FolloweeId, 
            Type = NotifType.FOLLOW_ACCEPTED, 
            Message = "Your follow request was accepted.", 
            TargetId = msg.FolloweeId, 
            TargetType = TargetType.USER 
        }); 
        _ctx.Notifications.Add(new Notification 
        { 
            RecipientId = msg.FolloweeId, 
            ActorId = msg.FollowerId, 
            Type = NotifType.NEW_FOLLOWER, 
            Message = "You have a new follower.", 
            TargetId = msg.FollowerId, 
            TargetType = TargetType.USER 
        }); 
        await _ctx.SaveChangesAsync(); 
    } 
} 
  