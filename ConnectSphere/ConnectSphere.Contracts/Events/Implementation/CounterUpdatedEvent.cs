using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record CountersUpdatedEvent(int UserId, int FollowerDelta, int 
FollowingDelta, int PostDelta) : ICountersUpdatedEvent; 