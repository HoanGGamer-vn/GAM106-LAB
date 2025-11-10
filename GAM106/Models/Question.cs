using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAM106.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }
        public string ContentQuestion { get; set; } = string.Empty;
        [Required]
        public string Answer { get; set; } = string.Empty;
        public string Option1 { get; set; } = string.Empty;
        public string Option2 { get; set; } = string.Empty;
        public string Option3 { get; set; } = string.Empty;
        public string Option4 { get; set; } = string.Empty;
        [ForeignKey("Level")]
        public int LevelId { get; set; }
    }
}