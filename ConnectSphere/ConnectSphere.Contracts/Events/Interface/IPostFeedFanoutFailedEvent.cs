namespace ConnectSphere.Contracts.Events.Interface; 
  
public interface IPostFeedFanoutFailedEvent 
{ 
    int PostId { get; } 
    string Reason { get; } 
} 