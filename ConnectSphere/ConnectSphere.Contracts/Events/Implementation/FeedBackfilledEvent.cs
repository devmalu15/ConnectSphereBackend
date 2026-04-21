using ConnectSphere.Contracts.Enums; 
using ConnectSphere.Contracts.Events.Interface;
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record FeedBackfilledEvent(int RecipientUserId, int FolloweeId) : 
IFeedBackfilledEvent;