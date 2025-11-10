using Microsoft.EntityFrameworkCore;
using GAM106.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GAM106.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

    public DbSet<GameLevel> GameLevels { get; set; } = null!;
    public DbSet<Region> Regions { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<LevelResult> LevelResults { get; set; } = null!;
    }
}
