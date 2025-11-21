using System.ComponentModel.DataAnnotations;

namespace GAM106_LAB.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string Name { get; set; }
    }
}
