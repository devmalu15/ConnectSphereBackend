using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events.Interface;
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record CountersUpdatedEvent(int UserId, int FollowerDelta, int 
FollowingDelta, int PostDelta) : ICountersUpdatedEvent; 