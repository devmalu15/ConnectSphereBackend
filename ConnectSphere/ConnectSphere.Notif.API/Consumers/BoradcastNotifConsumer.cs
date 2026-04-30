using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events; 
using ConnectSphere.Notif.API.Data; 
using ConnectSphere.Notif.API.Entities; 
using MassTransit; 
  
namespace ConnectSphere.Notif.API.Consumers;



public class BroadcastNotifConsumer : 
IConsumer<ConnectSphere.Contracts.Events.Interface.IBroadcastNotifEvent> 
{ 
    private readonly NotifDbContext _ctx; 
    public BroadcastNotifConsumer(NotifDbContext ctx) => _ctx = ctx; 
  
    public async Task 
Consume(ConsumeContext<ConnectSphere.Contracts.Events.Interface.IBroadcastNotifEvent> 
context) 
    { 
        var msg = context.Message; 
        var notifications = msg.UserIds.Select(uid => new Notification 
        { 
            RecipientId = uid, 
            ActorId = 0, 
            Type = msg.Type, 
            Message = msg.Message 
        }).ToList(); 
        await _ctx.Notifications.AddRangeAsync(notifications); 
        await _ctx.SaveChangesAsync(); 
    } 
} 