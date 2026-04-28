using ConnectSphere.Contracts.Events.Interface;
namespace ConnectSphere.Contracts.Events.Implementation; 
public record PostFeedFanoutCompletedEvent(int PostId) : IPostFeedFanoutCompletedEvent; 
