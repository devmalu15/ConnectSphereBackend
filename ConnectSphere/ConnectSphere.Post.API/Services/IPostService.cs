using ConnectSphere.Contracts.DTOs; 
using ConnectSphere.Post.API.DTOs; 
  
namespace ConnectSphere.Post.API.Services; 
  
public interface IPostService 
{ 
    Task<PostDto> CreateAsync(int userId, CreatePostDto dto); 
    Task<PostDto?> GetByIdAsync(int postId, int? requestingUserId); 
    Task<PagedResult<PostDto>> GetByUserIdAsync(int userId, int page, int 
pageSize); 
    Task<PagedResult<PostDto>> GetPublicAsync(int page, int pageSize); 
    Task<IList<PostDto>> GetByHashtagAsync(string tag); 
    Task<IList<PostDto>> SearchAsync(string query); 
    Task<IList<PostDto>> GetTrendingAsync(int topN = 20); 
    Task<PostDto> UpdateAsync(int postId, int userId, UpdatePostDto dto); 
    Task SoftDeleteAsync(int postId, int userId); 
    Task<PostDto> RepostAsync(int postId, int userId); 
    Task UpdateDistributionStatusAsync(int postId, string status); 
    Task IncrementCommentCountAsync(int postId); 
    Task DecrementCommentCountAsync(int postId); 
} 