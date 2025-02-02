using System.Text;
using FollowingService.Data.Models;
using FollowingService.Data.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace FollowService.Services;

public class FollowService : IFollowService
{
    private readonly IFollowRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _cacheOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly IHttpClientFactory _httpClientFactory;
    public FollowService(IFollowRepository repository, IDistributedCache cache, IHttpClientFactory httpClientFactory)
    {
        _repository = repository;
        _cache = cache;
        _httpClientFactory = httpClientFactory;
    }

    public async Task FollowUserAsync(int followerId, int followeeId)
    {
        // Fetch usernames asynchronously
        var followerUsernameTask = GetUsernameFromExternalApi(followerId);
        var followeeUsernameTask = GetUsernameFromExternalApi(followeeId);

        await Task.WhenAll(followerUsernameTask, followeeUsernameTask); // Run both in parallel

        var followerUsername = followerUsernameTask.Result;
        var followeeUsername = followeeUsernameTask.Result;

        // Validate usernames
        if (string.IsNullOrEmpty(followerUsername) && string.IsNullOrEmpty(followeeUsername))
        {
            throw new Exception($"Both users (ID: {followerId} & {followeeId}) do not have a username.");
        }
        else if (string.IsNullOrEmpty(followerUsername))
        {
            throw new Exception($"Follower with ID {followerId} does not have a username.");
        }
        else if (string.IsNullOrEmpty(followeeUsername))
        {
            throw new Exception($"User with ID {followeeId} does not have a username.");
        }

        // Proceed with following if both usernames exist
        await _repository.FollowUserAsync(followerId, followeeId);

        // Remove cache entries for updated follow lists
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

    public async Task<GetFollowersResponseDto> GetFollowersAsync(int userId)
    {
        string cacheKey = $"followers:{userId}";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (cachedData != null)
        {
            return JsonSerializer.Deserialize<GetFollowersResponseDto>(cachedData, _cacheOptions);
        }

        // Retrieve follower IDs
        var followers = await _repository.GetFollowersAsync(userId);

        // Call an external API to get usernames for each follower
        var usernameTasks = followers.Select(async id => 
        {
            var username = await GetUsernameFromExternalApi(id);
            return username;
        });

        var usernames = await Task.WhenAll(usernameTasks);

        var response = new GetFollowersResponseDto 
        { 
            Usernames = usernames.ToList() // Assuming you modify your DTO to have a List<string> Usernames
        };

        // Cache the response
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        return response;
    }

private async Task<string> GetUsernameFromExternalApi(int userId)
{
    var httpClient = _httpClientFactory.CreateClient();

    var requestPayload = new { userID = userId };
    var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");

    var response = await httpClient.PostAsync("https://mosquito-dear-mainly.ngrok-free.app/getUsername", content);
    
    if (!response.IsSuccessStatusCode)
    {
        throw new Exception($"Error fetching username for user {userId}: {response.StatusCode} - {response.ReasonPhrase}");
    }

    // Read response body
    var responseBody = await response.Content.ReadAsStringAsync();

    try
    {
        Console.WriteLine($"API Response for user {userId}: {responseBody}");  // Debugging log

        // Deserialize JSON response safely
        var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);

        if (responseData == null || !responseData.ContainsKey("userName"))
        {
            throw new Exception($"Invalid API response structure for user {userId}. Response: {responseBody}");
        }

        var userNameObject = responseData["userName"];

        // Handle null or empty username cases
        if (userNameObject == null)
        {
            throw new Exception($"User {userId} exists but does not have a username.");
        }

        if (userNameObject is JsonElement jsonElement && jsonElement.TryGetProperty("username", out JsonElement usernameElement))
        {
            var username = usernameElement.GetString();
            if (string.IsNullOrEmpty(username))
            {
                throw new Exception($"User {userId} exists but has an empty username.");
            }
            return username;
        }

        throw new Exception($"Unexpected API response format for user {userId}. Response: {responseBody}");
    }
    catch (JsonException ex)
    {
        throw new Exception($"JSON deserialization error: {ex.Message} | Response: {responseBody}");
    }
}

    public async Task<GetFollowingsResponseDto> GetFollowingAsync(int userId)
    {
        string cacheKey = $"following:{userId}";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (cachedData != null)
        {
            return JsonSerializer.Deserialize<GetFollowingsResponseDto>(cachedData, _cacheOptions);
        }

        var following = await _repository.GetFollowingAsync(userId);
        var response = new GetFollowingsResponseDto { FollowingIds = following };

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        });

        return response;
    }
}
