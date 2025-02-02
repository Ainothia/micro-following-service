using FollowingService.Data.Models;
using FollowService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FollowingService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpPost("")]
        public async Task<IActionResult> Follow(FollowRequestModel followRequestModel)
        {
            var userId = followRequestModel.UserId;
            var followerId = followRequestModel.FollowerId;
            Console.WriteLine("UserId: " + userId + " FollowerId: " + followerId);

            try
            {
                await _followService.FollowUserAsync(followerId, userId);
                return Ok(new { message = "Follow request successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpDelete("{userId}")]
        public async Task<IActionResult> Unfollow(int userId, [FromQuery] int followerId)
        {
            await _followService.UnfollowUserAsync(followerId, userId);
            return Ok();
        }

        [HttpGet("followers/{userId}")]
        public async Task<IActionResult> GetFollowers(int userId) => Ok(await _followService.GetFollowersAsync(userId));

        [HttpGet("following/{userId}")]
        public async Task<IActionResult> GetFollowing(int userId) => Ok(await _followService.GetFollowingAsync(userId));

        [HttpGet("is-following/{followerId}/{followeeId}")]
        public async Task<IActionResult> IsFollowing(int followerId, int followeeId) => Ok(await _followService.IsFollowingAsync(followerId, followeeId));
    }
}