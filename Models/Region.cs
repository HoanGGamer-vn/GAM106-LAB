using System.ComponentModel.DataAnnotations;

namespace GAM106_LAB.Models
{
    public class Region
    {
        public int RegionId { get; set; }
        [Required]
        public string RegionName { get; set; }
    }
}
