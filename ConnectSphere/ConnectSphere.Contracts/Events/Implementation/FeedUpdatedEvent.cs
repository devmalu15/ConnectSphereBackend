using ConnectSphere.Contracts.Enums; 
  
namespace ConnectSphere.Contracts.Events.Implementation; 

public record FeedUpdatedEvent(int RecipientUserId, int PostId) : 
IFeedUpdatedEvent;