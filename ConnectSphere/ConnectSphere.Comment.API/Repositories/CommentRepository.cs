using ConnectSphere.Comment.API.Data;
using CommentEntity = ConnectSphere.Comment.API.Entities.Comment;
using Microsoft.EntityFrameworkCore;

namespace ConnectSphere.Comment.API.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly CommentDbContext _ctx;
    public CommentRepository(CommentDbContext ctx) => _ctx = ctx;

    public Task<CommentEntity?> GetByIdAsync(int commentId) =>
        _ctx.Comments.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.CommentId == commentId);

    public Task<IList<CommentEntity>> GetTopLevelByPostAsync(int postId) =>
        _ctx.Comments
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync()
            .ContinueWith<IList<CommentEntity>>(t => t.Result);

    public Task<IList<CommentEntity>> GetRepliesAsync(int commentId) =>
        _ctx.Comments
            .Where(c => c.ParentCommentId == commentId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync()
            .ContinueWith<IList<CommentEntity>>(t => t.Result);

    public Task<IList<CommentEntity>> GetByUserAsync(int userId) =>
        _ctx.Comments
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync()
            .ContinueWith<IList<CommentEntity>>(t => t.Result);

    public Task<int> CountByPostAsync(int postId) =>
        _ctx.Comments.CountAsync(c => c.PostId == postId);

    public async Task AddAsync(CommentEntity comment) => await
_ctx.Comments.AddAsync(comment);

    public Task SaveChangesAsync() => _ctx.SaveChangesAsync();
}