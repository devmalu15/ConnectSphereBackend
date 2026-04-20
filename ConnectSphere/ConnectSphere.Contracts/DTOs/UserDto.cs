namespace ConnectSphere.Contracts.DTOs; 
  
public record UserDto( 
    int UserId, string UserName, string FullName, string? AvatarUrl, 
    string? Bio, bool IsPrivate, int FollowerCount, int FollowingCount, int 
PostCount);