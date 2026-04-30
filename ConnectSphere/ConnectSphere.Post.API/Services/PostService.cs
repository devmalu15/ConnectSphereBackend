using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ConnectSphere.Contracts.DTOs;
using ConnectSphere.Contracts.Enums;
using ConnectSphere.Contracts.Events.Implementation;
using ConnectSphere.Post.API.Data;
using ConnectSphere.Post.API.DTOs;
using ConnectSphere.Post.API.Entities;
using PostEntity = ConnectSphere.Post.API.Entities.Post;
using ConnectSphere.Post.API.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Net.Http.Json;

namespace ConnectSphere.Post.API.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _repo;
    private readonly PostDbContext _ctx;
    private readonly IPublishEndpoint _bus;
    private readonly Cloudinary _cloudinary;
    private readonly IHttpClientFactory _httpFactory;

    public PostService(IPostRepository repo, PostDbContext ctx,
        IPublishEndpoint bus, Cloudinary cloudinary, IHttpClientFactory httpFactory)
    {
        _repo = repo; _ctx = ctx; _bus = bus; _cloudinary = cloudinary; _httpFactory = httpFactory;
    }

    public async Task<PostDto> CreateAsync(int userId, CreatePostDto dto)
    {
        var post = new PostEntity
        {
            UserId = userId,
            Content = dto.Content,
            MediaType = dto.MediaType,
            Visibility = dto.Visibility,
            Hashtags = dto.Hashtags
        };

        if (dto.MediaFile != null)
        {
            using var stream = dto.MediaFile.OpenReadStream();
            
            if (dto.MediaType == MediaType.VIDEO)
            {
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(dto.MediaFile.FileName, stream),
                    Folder = "connectsphere/posts"
                };
                var result = await _cloudinary.UploadAsync(uploadParams);
                post.MediaUrl = result.SecureUrl.ToString();
                post.MediaPublicId = result.PublicId;
            }
            else 
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(dto.MediaFile.FileName, stream),
                    Folder = "connectsphere/posts"
                };
                var result = await _cloudinary.UploadAsync(uploadParams);
                post.MediaUrl = result.SecureUrl.ToString();
                post.MediaPublicId = result.PublicId;
            }
        }

        await _repo.AddAsync(post);
        await _repo.SaveChangesAsync();

        await _bus.Publish(new PostCreatedEvent(
            post.PostId, post.UserId, post.Content,
            post.Hashtags, post.Visibility, post.CreatedAt));

        await ProcessMentionsAsync(post.UserId, post.PostId, post.Content);

        await _bus.Publish(new CountersUpdatedEvent(userId, 0, 0, 1));
        return ToDto(post);
    }

    private async Task ProcessMentionsAsync(int actorId, int postId, string content)
    {
        var usernames = Regex.Matches(content, @"@([a-zA-Z0-9._-]+)")
            .Select(m => m.Groups[1].Value).Distinct().ToList();

        Console.WriteLine($"[MENTIONS] Found {usernames.Count} potential mentions: {string.Join(", ", usernames)}");

        if (!usernames.Any()) return;

        var client = _httpFactory.CreateClient("AuthService");
        foreach (var username in usernames)
        {
            try
            {
                var url = $"api/users/username/{username}";
                Console.WriteLine($"[MENTIONS] Calling {client.BaseAddress}{url}");
                
                var response = await client.GetAsync(url);
                Console.WriteLine($"[MENTIONS] Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
                    if (result?.Data != null)
                    {
                        Console.WriteLine($"[MENTIONS] User Found: {result.Data.UserId} (Actor: {actorId})");
                        
                        if (result.Data.UserId != actorId)
                        {
                            _ctx.Mentions.Add(new Mention
                            {
                                PostId = postId,
                                UserId = result.Data.UserId,
                                MentionedAt = DateTime.UtcNow
                            });
                            await _ctx.SaveChangesAsync();
                            Console.WriteLine($"[MENTIONS] Saved mention for {username} (ID: {result.Data.UserId})");

                            await _bus.Publish(new MentionEvent(
                                actorId,
                                result.Data.UserId,
                                postId,
                                TargetType.POST,
                                content,
                                DateTime.UtcNow));
                        }
                        else
                        {
                            Console.WriteLine($"[MENTIONS] Skipping self-mention for {username}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[MENTIONS] User {username} not found in Auth Service (Data is null)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MENTIONS] CRITICAL ERROR for {username}: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"[MENTIONS] Inner: {ex.InnerException.Message}");
            }
        }
    }

    public async Task<PostDto?> GetByIdAsync(int postId, int? requestingUserId)
    {
        var post = await _repo.GetByIdAsync(postId);
        if (post == null) return null;
        if (post.Visibility == Visibility.PRIVATE && post.UserId !=
requestingUserId) return null;
        return ToDto(post);
    }

    public async Task<PagedResult<PostDto>> GetByUserIdAsync(int userId, int page,
int pageSize)
    {
        var result = await _repo.GetByUserIdAsync(userId, page, pageSize);
        return new PagedResult<PostDto>(result.Items.Select(ToDto).ToList(),
result.Page, result.PageSize, result.TotalCount);
    }

    public async Task<PagedResult<PostDto>> GetPublicAsync(int page, int pageSize)
    {
        var result = await _repo.GetPublicAsync(page, pageSize);
        return new PagedResult<PostDto>(result.Items.Select(ToDto).ToList(),
result.Page, result.PageSize, result.TotalCount);
    }

    public async Task<IList<PostDto>> GetByHashtagAsync(string tag)
    {
        var posts = await _repo.GetByHashtagAsync(tag);
        return posts.Select(ToDto).ToList();
    }

    public async Task<IList<PostDto>> SearchAsync(string query)
    {
        var posts = await _repo.SearchAsync(query);
        return posts.Select(ToDto).ToList();
    }


    public async Task UpdateLikeCountAsync(int postId, int delta)
    {
        await _ctx.Posts
            .Where(p => p.PostId == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(
                p => p.LikeCount,
                p => p.LikeCount + delta));
    }

    public async Task<IList<PostDto>> GetTrendingAsync(int topN = 20)
    {
        var posts = await _repo.GetTrendingAsync(topN);
        return posts.Select(ToDto).ToList();
    }

    public async Task<PostDto> UpdateAsync(int postId, int userId, UpdatePostDto
dto)
    {
        var post = await _repo.GetByIdAsync(postId) ?? throw new
KeyNotFoundException();
        if (post.UserId != userId) throw new UnauthorizedAccessException();

        if (dto.Content != null) post.Content = dto.Content;
        if (dto.Visibility.HasValue) post.Visibility = dto.Visibility.Value;
        if (dto.Hashtags != null) post.Hashtags = dto.Hashtags;
        post.UpdatedAt = DateTime.UtcNow;

        await _repo.SaveChangesAsync();
        return ToDto(post);
    }

    public async Task SoftDeleteAsync(int postId, int userId)
    {
        var post = await _repo.GetByIdAsync(postId) ?? throw new
KeyNotFoundException();
        if (post.UserId != userId) throw new UnauthorizedAccessException();
        await _ctx.Posts.Where(p => p.PostId == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsDeleted, true));
        await _bus.Publish(new PostDeletedEvent(postId, userId));

        await _bus.Publish(new CountersUpdatedEvent(userId, 0, 0, -1));
    }

    public async Task<PostDto> RepostAsync(int postId, int userId)
    {
        var original = await _repo.GetByIdAsync(postId) ?? throw new KeyNotFoundException();
        
        await _ctx.Posts.Where(p => p.PostId == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.ShareCount, p => p.ShareCount + 1));

        var repost = new PostEntity
        {
            UserId = userId,
            Content = original.Content,
            MediaUrl = original.MediaUrl,
            MediaType = original.MediaType,
            Visibility = original.Visibility,
            Hashtags = original.Hashtags,
            OriginalPostId = postId
        };
        await _repo.AddAsync(repost);
        await _repo.SaveChangesAsync();

        await _bus.Publish(new PostRepostedEvent(
            postId, 
            userId, 
            original.UserId, 
            repost.PostId, 
            DateTime.UtcNow));

        return ToDto(repost);
    }

    public async Task UpdateDistributionStatusAsync(int postId, string status)
    {
        var statusEnum = Enum.Parse<PostDistribution>(status, true);
        await _ctx.Posts.Where(p => p.PostId == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.DistributionStatus,
statusEnum));
    }

    public async Task IncrementCommentCountAsync(int postId)
    {
        await _ctx.Posts.Where(p => p.PostId == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.CommentCount, p =>
p.CommentCount + 1));
    }

    public async Task DecrementCommentCountAsync(int postId)
    {
        await _ctx.Posts.Where(p => p.PostId == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.CommentCount, p =>
p.CommentCount > 0 ? p.CommentCount - 1 : 0));
    }

    public async Task<int> GetCountAsync()
    {
        return await _ctx.Posts.CountAsync(p => !p.IsDeleted);
    }

    public async Task<PagedResult<PostDto>> GetMentionedPostsAsync(int userId, int page, int pageSize)
    {
        var query = _ctx.Mentions
            .Where(m => m.UserId == userId)
            .Include(m => m.Post)
            .OrderByDescending(m => m.MentionedAt)
            .Select(m => m.Post);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<PostDto>(
            items.Select(ToDto).ToList(),
            page,
            pageSize,
            totalCount);
    }

    private static PostDto ToDto(PostEntity p) => new(
        p.PostId, p.UserId, p.Content, p.MediaUrl, p.MediaType,
        p.Visibility, p.LikeCount, p.CommentCount, p.ShareCount,
        p.Hashtags, p.IsDeleted, p.CreatedAt, p.UpdatedAt);
}