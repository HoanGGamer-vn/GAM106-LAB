using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAM106_LAB.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }
        [Required]
        public string QuestionText { get; set; }
        public string Answer { get; set; }
        public string Options { get; set; }
        [ForeignKey("GameLevel")]
        public int LevelId { get; set; }
    }
}
