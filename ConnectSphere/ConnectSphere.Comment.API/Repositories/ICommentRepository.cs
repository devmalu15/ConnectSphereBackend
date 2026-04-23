using CommentEntity = ConnectSphere.Comment.API.Entities.Comment; 
  
namespace ConnectSphere.Comment.API.Repositories; 
  
public interface ICommentRepository 
{ 
    Task<CommentEntity?> GetByIdAsync(int commentId); 
    Task<IList<CommentEntity>> GetTopLevelByPostAsync(int postId); 
    Task<IList<CommentEntity>> GetRepliesAsync(int commentId); 
    Task<IList<CommentEntity>> GetByUserAsync(int userId); 
    Task<int> CountByPostAsync(int postId); 
    Task AddAsync(CommentEntity comment); 
    Task SaveChangesAsync(); 
} 