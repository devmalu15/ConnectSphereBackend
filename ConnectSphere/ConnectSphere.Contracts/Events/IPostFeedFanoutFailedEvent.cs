namespace ConnectSphere.Contracts.Events; 
  
public interface IPostFeedFanoutFailedEvent 
{ 
    int PostId { get; } 
    string Reason { get; } 
} 