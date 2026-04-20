namespace ConnectSphere.Contracts.Events; 
  
public interface IFeedBackfilledEvent 
{ 
    int RecipientUserId { get; } 
    int FolloweeId { get; } 
} 