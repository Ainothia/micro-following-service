using FollowingService.Data.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace FollowService.Services;

public class FollowService : IFollowService
{
    private readonly IFollowRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _cacheOptions = new() { PropertyNameCaseInsensitive = true };

    public FollowService(IFollowRepository repository, IDistributedCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task FollowUserAsync(int followerId, int followeeId)
    {
        await _repository.FollowUserAsync(followerId, followeeId);
        await _cache.RemoveAsync($"followers:{followeeId}");
        await _cache.RemoveAsync($"following:{followerId}");
    }

    public async Task UnfollowUserAsync(int followerId, int followeeId)
    {
        await _repository.UnfollowUserAsync(followerId, followeeId);
        await _cache.RemoveAsync($"followers:{followeeId}");
        await _cache.RemoveAsync($"following:{followerId}");
    }

    public async Task<bool> IsFollowingAsync(int followerId, int followeeId) =>
        await _repository.IsFollowingAsync(followerId, followeeId);

    public async Task<List<int>> GetFollowersAsync(int userId)
    {
        string cacheKey = $"followers:{userId}";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (cachedData != null)
        {
            return JsonSerializer.Deserialize<List<int>>(cachedData, _cacheOptions);
        }

        var followers = await _repository.GetFollowersAsync(userId);
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(followers), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        return followers;
    }

    public async Task<List<int>> GetFollowingAsync(int userId)
    {
        string cacheKey = $"following:{userId}";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (cachedData != null)
        {
            return JsonSerializer.Deserialize<List<int>>(cachedData, _cacheOptions);
        }

        var following = await _repository.GetFollowingAsync(userId);
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(following), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        return following;
    }
}
