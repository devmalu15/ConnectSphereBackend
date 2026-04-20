using ConnectSphere.Auth.API.Entities; 
  
namespace ConnectSphere.Auth.API.Repositories; 
  
public interface IUserRepository 
{ 
    Task<User?> GetByIdAsync(int userId); 
    Task<User?> GetByEmailAsync(string email); 
    Task<User?> GetByUserNameAsync(string userName); 
    Task<User?> GetByOAuthAsync(string provider, string providerId); 
    Task<IList<User>> SearchAsync(string query); 
    Task<IList<User>> GetSuggestedUsersAsync(int userId, IList<int> followingIds); 
    Task AddAsync(User user); 
    Task UpdateAsync(User user); 
    Task<bool> ExistsByEmailAsync(string email); 
    Task<bool> ExistsByUserNameAsync(string userName); 
    Task SaveChangesAsync(); 
} 