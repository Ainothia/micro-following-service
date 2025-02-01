using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FollowService.Data.Entities
{
    public class Follower
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // ✅ Auto-increment ID
        public int Id { get; set; }

        [Required]
        public int FollowerId { get; set; } // The user who follows

        [Required]
        public int FolloweeId { get; set; } // The user being followed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}