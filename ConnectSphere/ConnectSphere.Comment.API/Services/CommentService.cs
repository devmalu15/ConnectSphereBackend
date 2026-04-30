using ConnectSphere.Comment.API.Data;
using ConnectSphere.Comment.API.DTOs;
using CommentEntity = ConnectSphere.Comment.API.Entities.Comment;
using ConnectSphere.Contracts.DTOs;
using ConnectSphere.Contracts.Enums;
using ConnectSphere.Contracts.Events.Implementation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ConnectSphere.Comment.API.Services;

public class CommentService : ICommentService
{
    private readonly CommentDbContext _ctx;
    private readonly IPublishEndpoint _bus;
    private readonly IHttpClientFactory _httpFactory;

    public CommentService(CommentDbContext ctx, IPublishEndpoint bus,
IHttpClientFactory httpFactory)
    {
        _ctx = ctx; _bus = bus; _httpFactory = httpFactory;
    }

    public async Task<CommentDto> AddAsync(int userId, CreateCommentDto dto)
    {
        var comment = new CommentEntity
        {
            PostId = dto.PostId,
            UserId = userId,
            ParentCommentId = dto.ParentCommentId == 0 ? null : dto.ParentCommentId,
            Content = dto.Content
        };

        _ctx.Comments.Add(comment);

        await _ctx.SaveChangesAsync();

        await _bus.Publish(new CommentAddedEvent(
            comment.CommentId, comment.PostId, comment.UserId,
            comment.ParentCommentId, comment.Content, comment.CreatedAt));

        await ProcessMentionsAsync(comment.UserId, comment.CommentId, comment.Content);
        return ToDto(comment);
    }

    private async Task ProcessMentionsAsync(int actorId, int commentId, string content)
    {
        var usernames = Regex.Matches(content, @"@(\w+)")
            .Select(m => m.Groups[1].Value).Distinct().ToList();

        if (!usernames.Any()) return;

        var client = _httpFactory.CreateClient("AuthService");
        foreach (var username in usernames)
        {
            try
            {
                var response = await client.GetAsync($"api/users/username/{username}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
                    if (result?.Data != null && result.Data.UserId != actorId)
                    {
                        await _bus.Publish(new MentionEvent(
                            actorId,
                            result.Data.UserId,
                            commentId,
                            TargetType.COMMENT,
                            content,
                            DateTime.UtcNow));
                    }
                }
            }
            catch (Exception)
            {
                // Log error
            }
        }
    }
    public async Task<IList<CommentDto>> GetByUserAsync(int userId) =>
    (await _ctx.Comments
        .Where(c => c.UserId == userId && !c.IsDeleted)
        .OrderByDescending(c => c.CreatedAt)
        .ToListAsync()).Select(ToDto).ToList();

    public async Task<int> GetCountByPostAsync(int postId) =>
        await _ctx.Comments.CountAsync(c => c.PostId == postId && !c.IsDeleted);

    public async Task IncrementLikeCountAsync(int commentId, int delta)
    {
        await _ctx.Comments
            .Where(c => c.CommentId == commentId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.LikeCount, c => c.LikeCount + delta));
    }

    public async Task<CommentDto?> GetByIdAsync(int commentId)
    {
        var comment = await _ctx.Comments.FirstOrDefaultAsync(c => c.CommentId ==
commentId);
        return comment == null ? null : ToDto(comment);
    }

    public async Task<IList<CommentDto>> GetTopLevelByPostAsync(int postId) =>
        (await _ctx.Comments
            .Where(c => c.PostId == postId && (c.ParentCommentId == null || c.ParentCommentId == 0))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync()).Select(ToDto).ToList();

    public async Task<IList<CommentDto>> GetRepliesAsync(int commentId) =>
        (await _ctx.Comments
            .Where(c => c.ParentCommentId == commentId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync()).Select(ToDto).ToList();

    public async Task<CommentDto> EditAsync(int commentId, int userId, string
newContent)
    {
        var comment = await _ctx.Comments.FirstOrDefaultAsync(c => c.CommentId ==
commentId)
            ?? throw new KeyNotFoundException();
        if (comment.UserId != userId) throw new UnauthorizedAccessException();

        await _ctx.Comments.Where(c => c.CommentId == commentId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Content, newContent)
                .SetProperty(c => c.IsEdited, true)
                .SetProperty(c => c.EditedAt, DateTime.UtcNow));

        comment.Content = newContent;
        comment.IsEdited = true;
        return ToDto(comment);
    }

   public async Task SoftDeleteAsync(int commentId, int userId)
{
    var comment = await _ctx.Comments
        .FirstOrDefaultAsync(c => c.CommentId == commentId)
        ?? throw new KeyNotFoundException();

    if (comment.UserId != userId) throw new UnauthorizedAccessException();

    // Soft-delete the comment itself
    await _ctx.Comments
        .Where(c => c.CommentId == commentId)
        .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsDeleted, true));

    // Soft-delete all direct replies to this comment
    var replyCount = await _ctx.Comments
        .Where(c => c.ParentCommentId == commentId && !c.IsDeleted)
        .CountAsync();

    if (replyCount > 0)
    {
        await _ctx.Comments
            .Where(c => c.ParentCommentId == commentId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsDeleted, true));
    }

    // Total deleted = the comment itself + its replies
    int totalDeleted = 1 + replyCount;

    await _bus.Publish(new CommentDeletedEvent(
        commentId,
        comment.PostId,
        comment.ParentCommentId,
        totalDeleted));
}

    public async Task<int> GetCountAsync()
    {
        return await _ctx.Comments.CountAsync(c => !c.IsDeleted);
    }

    private static CommentDto ToDto(CommentEntity c)
    {
        var displayContent = c.IsDeleted ? "This comment was deleted." : c.Content;
        return new CommentDto(c.CommentId, c.PostId, c.UserId, c.ParentCommentId,
            displayContent, c.LikeCount, c.ReplyCount, c.IsEdited, c.IsDeleted,
c.CreatedAt, c.EditedAt);
    }
}