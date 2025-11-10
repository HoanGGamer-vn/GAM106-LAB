using System.ComponentModel.DataAnnotations;

namespace GAM106.Models
{
    public class GameLevel
    {
        [Key]
        public int LevelId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}