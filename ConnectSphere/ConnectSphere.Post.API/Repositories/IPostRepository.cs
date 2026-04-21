using PostEntity = ConnectSphere.Post.API.Entities.Post; 
using ConnectSphere.Contracts.DTOs; 
  
namespace ConnectSphere.Post.API.Repositories; 
  
public interface IPostRepository 
{ 
    Task<PostEntity?> GetByIdAsync(int postId); 
    Task<PagedResult<PostEntity>> GetByUserIdAsync(int userId, int page, int pageSize); 
    Task<PagedResult<PostEntity>> GetPublicAsync(int page, int pageSize); 
    Task<IList<PostEntity>> GetByHashtagAsync(string tag); 
    Task<IList<PostEntity>> SearchAsync(string query); 
    Task<IList<PostEntity>> GetTrendingAsync(int topN); 
    Task AddAsync(PostEntity post); 
    Task SaveChangesAsync(); 
} 