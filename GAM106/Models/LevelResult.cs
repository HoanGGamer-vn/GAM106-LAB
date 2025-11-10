using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAM106.Models
{
    public class LevelResult
    {
        [Key]
        public int LevelResultId { get; set; }

        // Reference to the user who completed the level
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        // Which game level
        public int LevelId { get; set; }
        [ForeignKey(nameof(LevelId))]
        public GameLevel? Level { get; set; }

        // Score or result value
        public int Score { get; set; }

        // When completed
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
