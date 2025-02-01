namespace FollowService.Services;

public interface IFollowService
{
    Task FollowUserAsync(int followerId, int followeeId);
    Task UnfollowUserAsync(int followerId, int followeeId);
    Task<bool> IsFollowingAsync(int followerId, int followeeId);
    Task<List<int>> GetFollowersAsync(int userId);
    Task<List<int>> GetFollowingAsync(int userId);
}