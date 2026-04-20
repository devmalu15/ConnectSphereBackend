namespace ConnectSphere.Contracts.Events; 
  
public interface IFeedUpdatedEvent 
{ 
    int RecipientUserId { get; } 
    int PostId { get; } 
} 