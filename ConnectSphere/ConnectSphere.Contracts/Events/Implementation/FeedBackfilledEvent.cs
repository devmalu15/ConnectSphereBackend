using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record FeedBackfilledEvent(int RecipientUserId, int FolloweeId) : 
IFeedBackfilledEvent;