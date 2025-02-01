using FollowingService.Data.Models;

namespace FollowService.Services;

public interface IFollowService
{
    Task FollowUserAsync(int followerId, int followeeId);
    Task UnfollowUserAsync(int followerId, int followeeId);
    Task<bool> IsFollowingAsync(int followerId, int followeeId);
    Task<GetFollowersResponseDto> GetFollowersAsync(int userId);
    Task<GetFollowingsResponseDto> GetFollowingAsync(int userId);
}