namespace ConnectSphere.Feed.API.DTOs; 
  
/// <summary>Query parameters for paginated feed endpoints.</summary> 
public record FeedQueryDto( 
    int Page = 1, 
    int PageSize = 20 
); 