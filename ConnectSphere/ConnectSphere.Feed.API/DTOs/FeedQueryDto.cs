namespace ConnectSphere.Feed.API.DTOs; 
  
public record FeedQueryDto( 
    int Page = 1, 
    int PageSize = 20 
); 