using System.ComponentModel.DataAnnotations;

namespace GAM106_LAB.Models
{
    public class GameLevel
    {
        [Key]
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        public string? Difficulty { get; set; }
    }
}
