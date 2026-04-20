using ConnectSphere.Auth.API.Services; 
using ConnectSphere.Contracts.Events; 
using MassTransit; 
  
namespace ConnectSphere.Auth.API.Consumers; 
  
public class CountersUpdatedConsumer : IConsumer<ICountersUpdatedEvent> 
{ 
    private readonly IUserService _service; 
    public CountersUpdatedConsumer(IUserService service) => _service = service; 
  
    public async Task Consume(ConsumeContext<ICountersUpdatedEvent> context) 
    { 
        var msg = context.Message; 
        await _service.UpdateCountersAsync(msg.UserId, msg.FollowerDelta, 
msg.FollowingDelta, msg.PostDelta); 
    } 
} 