namespace ConnectSphere.Feed.API.DTOs; 

public record ScoredFeedItemDto( 
    int FeedItemId, 
    int UserId, 
    int PostId, 
    int ActorId, 
    decimal EngagementScore, 
    double FinalScore, 
    DateTime CreatedAt 
);