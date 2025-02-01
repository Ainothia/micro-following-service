using FollowService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FollowingService.Data.Repositories;

public class FollowRepository : IFollowRepository
{
    private readonly AppDbContext _context;

    public FollowRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task FollowUserAsync(int followerId, int followeeId)
    {
        var follow = new Follower { FollowerId = followerId, FolloweeId = followeeId };
        _context.Followers.Add(follow);
        await _context.SaveChangesAsync();
    }

    public async Task UnfollowUserAsync(int followerId, int followeeId)
    {
        var follow = await _context.Followers.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
        if (follow != null)
        {
            _context.Followers.Remove(follow);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsFollowingAsync(int followerId, int followeeId)
    {
        return await _context.Followers.AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
    }

    public async Task<List<int>> GetFollowersAsync(int userId)
    {
        return await _context.Followers.Where(f => f.FolloweeId == userId).Select(f => f.FollowerId).ToListAsync();
    }

    public async Task<List<int>> GetFollowingAsync(int userId)
    {
        return await _context.Followers.Where(f => f.FollowerId == userId).Select(f => f.FolloweeId).ToListAsync();
    }
}