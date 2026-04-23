using ConnectSphere.Contracts.Enums;
using ConnectSphere.Contracts.Events.Implementation;
using ConnectSphere.Contracts.Events.Interface;
using ConnectSphere.Like.API.Data;
using LikeEntity = ConnectSphere.Like.API.Entities.Like;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ConnectSphere.Like.API.Services;

public class LikeService : ILikeService
{
    private readonly LikeDbContext _ctx;
    private readonly IPublishEndpoint _bus;
    // private readonly IHttpClientFactory _httpFactory; 

    public LikeService(LikeDbContext ctx, IPublishEndpoint bus)
    {
        _ctx = ctx; _bus = bus;
    }

    public async Task<bool> ToggleLikeAsync(int userId, int targetId, TargetType targetType)
    {
        // 1. Determine state BEFORE changes
        var existing = await _ctx.Likes.FirstOrDefaultAsync(l =>
            l.UserId == userId && l.TargetId == targetId && l.TargetType == targetType);

        bool willBeLiked = (existing == null); // If it didn't exist, it will be a NEW like

        Console.WriteLine(willBeLiked);

        if (willBeLiked)
        {
            _ctx.Likes.Add(new LikeEntity { UserId = userId, TargetId = targetId, TargetType = targetType });

            Console.WriteLine("entered true if");

            await _ctx.SaveChangesAsync();

            
            // Change this in both the 'if' and 'else' branches
            await _bus.Publish(new LikeToggledEvent(userId, targetId, targetType, true));
            Console.WriteLine("about to exit true if");


        }
        else
        {

            Console.WriteLine("entered false if");
            _ctx.Likes.Remove(existing!);

            await _ctx.SaveChangesAsync();

            // Change this in both the 'if' and 'else' branches
            await _bus.Publish(new LikeToggledEvent(userId, targetId, targetType, false));


            

            Console.WriteLine("about to exit false if");

            

        }

        


        Console.WriteLine("exited true if/else" + willBeLiked);
        return willBeLiked;
    }
    //     private async Task UpdateCountOnRemoteService(int targetId, TargetType 
    // targetType, int delta) 
    //     { 
    //         var serviceName = targetType == TargetType.POST ? "PostService" : 
    // "CommentService"; 
    //         var client = _httpFactory.CreateClient(serviceName); 
    //         var endpoint = targetType == TargetType.POST 
    //             ? $"api/posts/{targetId}/like-count?delta={delta}" 
    //             : $"api/comments/{targetId}/like-count?delta={delta}"; 
    //         await client.PatchAsync(endpoint, null); 
    //     } 

    public async Task<int> GetLikeCountAsync(int targetId, TargetType targetType)
=>
        await _ctx.Likes.CountAsync(l => l.TargetId == targetId && l.TargetType ==
targetType);

    public async Task<bool> HasUserLikedAsync(int userId, int targetId, TargetType
targetType) =>
        await _ctx.Likes.AnyAsync(l => l.UserId == userId && l.TargetId == targetId
&& l.TargetType == targetType);

    public async Task<IList<int>> GetLikerIdsAsync(int targetId, TargetType
targetType) =>
        await _ctx.Likes.Where(l => l.TargetId == targetId && l.TargetType ==
targetType)
            .Select(l => l.UserId).ToListAsync();

    public async Task<IList<int>> GetLikedPostIdsByUserAsync(int userId) =>
        await _ctx.Likes.Where(l => l.UserId == userId && l.TargetType ==
TargetType.POST)
            .Select(l => l.TargetId).ToListAsync();
}