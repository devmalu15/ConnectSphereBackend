namespace ConnectSphere.Contracts.Events.Interface; 
  
public interface IFeedBackfilledEvent 
{ 
    int RecipientUserId { get; } 
    int FolloweeId { get; } 
} 