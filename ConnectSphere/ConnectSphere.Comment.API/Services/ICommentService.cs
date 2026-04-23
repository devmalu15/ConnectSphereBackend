using ConnectSphere.Contracts.DTOs; 
using ConnectSphere.Comment.API.DTOs;
  
namespace ConnectSphere.Comment.API.Services; 
  
public interface ICommentService 
{ 
    Task<CommentDto> AddAsync(int userId, CreateCommentDto dto); 
    Task<CommentDto?> GetByIdAsync(int commentId); 
    Task<IList<CommentDto>> GetTopLevelByPostAsync(int postId); 
    Task<IList<CommentDto>> GetRepliesAsync(int commentId); 
    Task<IList<CommentDto>> GetByUserAsync(int userId); 
    Task<int> GetCountByPostAsync(int postId); 
    Task<CommentDto> EditAsync(int commentId, int userId, string newContent); 
    Task SoftDeleteAsync(int commentId, int userId); 
    Task IncrementLikeCountAsync(int commentId, int delta); 
}