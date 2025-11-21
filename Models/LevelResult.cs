using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAM106_LAB.Models
{
    public class LevelResult
    {
        [Key]
        public int QuizResultId { get; set; }
        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        [ForeignKey("GameLevel")]
        public int LevelId { get; set; }
        public int Score { get; set; }
        public DateOnly CompletionDate { get; set; }
    }
}
