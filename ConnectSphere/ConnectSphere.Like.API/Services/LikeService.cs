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
   

    public LikeService(LikeDbContext ctx, IPublishEndpoint bus)
    {
        _ctx = ctx; _bus = bus;
    }

    public async Task<bool> ToggleLikeAsync(int userId, int targetId, TargetType targetType)
    {
        
        var existing = await _ctx.Likes.FirstOrDefaultAsync(l =>
            l.UserId == userId && l.TargetId == targetId && l.TargetType == targetType);

        bool willBeLiked = (existing == null); 

        Console.WriteLine(willBeLiked);

        if (willBeLiked)
        {
            _ctx.Likes.Add(new LikeEntity { UserId = userId, TargetId = targetId, TargetType = targetType });

            Console.WriteLine("entered true if");

            await _ctx.SaveChangesAsync();

            
           
            await _bus.Publish(new LikeToggledEvent(userId, targetId, targetType, true));
            Console.WriteLine("about to exit true if");


        }
        else
        {

            Console.WriteLine("entered false if");
            _ctx.Likes.Remove(existing!);

            await _ctx.SaveChangesAsync();

            
            await _bus.Publish(new LikeToggledEvent(userId, targetId, targetType, false));


            

            Console.WriteLine("about to exit false if");

            

        }

        


        Console.WriteLine("exited true if/else" + willBeLiked);
        return willBeLiked;
    }
    
    
    
    
    
    
    
    
    
    
    

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
    public async Task<int> GetTotalLikeCountAsync()
    {
        return await _ctx.Likes.CountAsync();
    }
}