namespace ConnectSphere.Contracts.Events.Interface; 
  
public interface IFeedUpdatedEvent 
{ 
    int RecipientUserId { get; } 
    int PostId { get; } 
} 