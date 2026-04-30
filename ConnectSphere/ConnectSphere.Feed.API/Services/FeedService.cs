using ConnectSphere.Contracts.DTOs;
using ConnectSphere.Feed.API.Data;
using ConnectSphere.Feed.API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ConnectSphere.Feed.API.Services;

public class FeedService : IFeedService
{
    private readonly FeedDbContext _ctx;
    private readonly IDistributedCache _cache;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<FeedService> _logger;

    private static readonly DistributedCacheEntryOptions FeedCacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };
    private static readonly DistributedCacheEntryOptions TrendingCacheOptions =
new()
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
};

    public FeedService(FeedDbContext ctx, IDistributedCache cache,
        IHttpClientFactory httpFactory, ILogger<FeedService> logger)
    {
        _ctx = ctx; _cache = cache; _httpFactory = httpFactory; _logger = logger;
    }

    

    public async Task<PagedResult<FeedItemDto>> GetFeedForUserAsync(int userId, int
page, int pageSize)
    {
        var cacheKey = $"feed:user:{userId}:{page}";
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
        {
            var cachedResult =
JsonSerializer.Deserialize<PagedResult<FeedItemDto>>(cached);
            if (cachedResult != null) return cachedResult;
        }

        var query = _ctx.FeedItems.Where(f => f.UserId == userId).OrderByDescending(f => f.CreatedAt);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var dtos = items.Select(ToDto).ToList();
        var result = new PagedResult<FeedItemDto>(dtos, page, pageSize, total);

        var json = JsonSerializer.Serialize(result);
        await _cache.SetStringAsync(cacheKey, json, FeedCacheOptions);

        return result;
    }

    

        public async Task<IList<FeedItemDto>> GetSuggestedFeedAsync(int userId, int page, int pageSize)
    {
        
        var tagAffinities = await _ctx.UserTagPreferences
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.Tag, p => p.AffinityScore);

        
        var since = DateTime.UtcNow.AddDays(-7);
        var candidates = await _ctx.FeedItems
            .Where(f => f.UserId == userId && f.CreatedAt >= since)
            .ToListAsync();

        if (candidates.Count == 0)
            return new List<FeedItemDto>();

        
        
        var postIds = candidates.Select(c => c.PostId).Distinct().ToList();
        var postHashtags = await FetchPostHashtagsAsync(postIds);

        
        var maxEngagement = candidates.Max(c => (double)c.Score);
        if (maxEngagement == 0) maxEngagement = 1;

        
        bool hasTasteProfile = tagAffinities.Count > 0;

        var scored = candidates.Select(item =>
        {
            var normalizedEngagement = (double)item.Score / maxEngagement;

            double personalisationScore = 0.0;
            if (hasTasteProfile && postHashtags.TryGetValue(item.PostId, out var tags) && tags.Count > 0)
            {
                
                var matchScore = tags
                    .Select(tag => tagAffinities.TryGetValue(tag, out var aff) ? aff : 0.0)
                    .Sum();
                personalisationScore = matchScore / tags.Count;
            }

            
            double finalScore = hasTasteProfile
                ? (0.6 * personalisationScore) + (0.4 * normalizedEngagement)
                : normalizedEngagement;

            return (Item: item, FinalScore: finalScore);
        })
        .OrderByDescending(x => x.FinalScore)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(x => ToDto(x.Item))
        .ToList();

        return scored;
    }

    

        public async Task UpdateTagPreferencesOnLikeAsync(int userId, int postId)
    {
        try
        {
            
            var client = _httpFactory.CreateClient("PostService");
            var response = await client.GetAsync($"api/posts/{postId}");
            if (!response.IsSuccessStatusCode) return;

            var postData = await
response.Content.ReadFromJsonAsync<PostApiWrapper>();
            var hashtagsRaw = postData?.Data?.Hashtags;
            if (string.IsNullOrWhiteSpace(hashtagsRaw)) return;

            
            var tags = hashtagsRaw
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().TrimStart('#').ToLowerInvariant())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();

            foreach (var tag in tags)
            {
                var existing = await _ctx.UserTagPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.Tag == tag);

                if (existing != null)
                {
                    
                    existing.AffinityScore = Math.Min(existing.AffinityScore + 1.0, 100.0);
                    existing.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    _ctx.UserTagPreferences.Add(new UserTagPreference
                    {
                        UserId = userId,
                        Tag = tag,
                        AffinityScore = 1.0,
                        LastUpdated = DateTime.UtcNow
                    });
                }
            }

            await _ctx.SaveChangesAsync();
            _logger.LogInformation("Updated tag preferences for user {UserId} from post {PostId}:{Tags}",
                userId, postId, string.Join(", ", tags));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update tag preferences for user {UserId}", userId);
            
        }
    }

    

    public async Task<IList<FeedItemDto>> GetExploreAsync(int userId)
    {
        
        var inFeed = await _ctx.FeedItems
            .Where(f => f.UserId == userId)
            .Select(f => f.PostId)
            .Distinct()
            .ToListAsync();

        
        var tagAffinities = await _ctx.UserTagPreferences
            .Where(p => p.UserId == userId)
            .ToDictionaryAsync(p => p.Tag, p => p.AffinityScore);

        
        
        var exploreCandidates = await _ctx.FeedItems
            .Where(f => f.UserId != userId && !inFeed.Contains(f.PostId))
            .OrderByDescending(f => f.Score)
            .Take(50)
            .ToListAsync();

        return exploreCandidates.Select(ToDto).ToList();
    }

    

    public async Task<IList<string>> GetTrendingHashtagsAsync(int topN = 10)
    {
        const string cacheKey = "feed:trending";
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
            return JsonSerializer.Deserialize<IList<string>>(cached) ?? new
List<string>();

        
        var client = _httpFactory.CreateClient("PostService");
        var response = await
client.GetAsync("api/posts/public?page=1&pageSize=200");
        if (!response.IsSuccessStatusCode) return new List<string>();

        var postsWrapper = await
response.Content.ReadFromJsonAsync<PostListWrapper>();
        var trending = postsWrapper?.Data?.Items
            .Where(p => !string.IsNullOrWhiteSpace(p.Hashtags))
            .SelectMany(p => p.Hashtags!.Split(',',
StringSplitOptions.RemoveEmptyEntries))
            .Select(t => t.Trim().ToLowerInvariant())
            .GroupBy(t => t)
            .OrderByDescending(g => g.Count())
            .Take(topN)
            .Select(g => g.Key)
            .ToList() ?? new List<string>();

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(trending),
TrendingCacheOptions);
        return trending;
    }

    

    public async Task AddToFeedAsync(int userId, int postId, int actorId, decimal
score)
    {
        var existing = await _ctx.FeedItems
            .AnyAsync(f => f.UserId == userId && f.PostId == postId);
        if (existing) return;

        _ctx.FeedItems.Add(new FeedItem
        {
            UserId = userId,
            PostId = postId,
            ActorId = actorId,
            Score = score,
            CreatedAt = DateTime.UtcNow
        });
        await _ctx.SaveChangesAsync();
        await InvalidateFeedCacheAsync(userId);
    }

    public async Task InvalidateFeedCacheAsync(int userId)
    {
        
        for (int i = 1; i <= 5; i++)
            await _cache.RemoveAsync($"feed:user:{userId}:{i}");
    }

    public async Task RemoveFromFeedsAsync(int postId)
    {
        var affected = await _ctx.FeedItems
            .Where(f => f.PostId == postId)
            .Select(f => f.UserId)
            .Distinct()
            .ToListAsync();

        await _ctx.FeedItems.Where(f => f.PostId == postId).ExecuteDeleteAsync();

        foreach (var uid in affected)
            await InvalidateFeedCacheAsync(uid);
    }

    public async Task BackfillFeedAsync(int recipientUserId, int followeeId)
    {
        var client = _httpFactory.CreateClient("PostService");
        var response = await
client.GetAsync($"api/posts/user/{followeeId}?page=1&pageSize=20");
        if (!response.IsSuccessStatusCode) return;

        var posts = await response.Content.ReadFromJsonAsync<PostListWrapper>();
        if (posts?.Data?.Items == null) return;

        foreach (var post in posts.Data.Items.Take(20))
        {
            await AddToFeedAsync(recipientUserId, post.PostId, followeeId,
CalculateEngagementScore(post));
        }
    }

    

    private async Task<Dictionary<int, List<string>>>
FetchPostHashtagsAsync(List<int> postIds)
    {
        var result = new Dictionary<int, List<string>>();
        if (!postIds.Any()) return result;

        try
        {
            var client = _httpFactory.CreateClient("PostService");
            
            var ids = string.Join(",", postIds);
            var response = await client.GetAsync($"api/posts/batch?ids={ids}");
            if (!response.IsSuccessStatusCode) return result;

            var posts = await
response.Content.ReadFromJsonAsync<PostBatchWrapper>();
            if (posts?.Data == null) return result;

            foreach (var p in posts.Data)
            {
                if (!string.IsNullOrWhiteSpace(p.Hashtags))
                {
                    result[p.PostId] = p.Hashtags
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim().TrimStart('#').ToLowerInvariant())
                        .ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch post hashtags for scoring");
        }

        return result;
    }

    private static decimal CalculateEngagementScore(PostData post) =>
        (post.LikeCount * 3) + (post.CommentCount * 2) + (post.ShareCount * 1);

    private static FeedItemDto ToDto(FeedItem f) =>
        new(f.FeedItemId, f.UserId, f.PostId, f.ActorId, f.Score, f.CreatedAt);

    

    private record PostApiWrapper(PostData? Data);
    private record PostData(int PostId, string? Hashtags, int LikeCount, int
CommentCount, int ShareCount);
    private record PostListWrapper(PostListData? Data);
    private record PostListData(IList<PostData> Items);
    private record PostBatchWrapper(IList<PostData>? Data);
}