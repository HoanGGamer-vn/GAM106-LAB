using GAM106_LAB.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GAM106_LAB.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options){}

        public DbSet<GameLevel> GameLevels { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<LevelResult> LevelResults { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Explicit FK relationship between ApplicationUser and Region
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Region)
                .WithMany()
                .HasForeignKey(u => u.RegionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Explicit FK relationship between ApplicationUser and Role (custom Role entity)
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<GameLevel>().HasData(
                new GameLevel { LevelId = 1, LevelName = "Beginner", Difficulty = "Easy" },
                new GameLevel { LevelId = 2, LevelName = "Intermediate", Difficulty = "Medium" },
                new GameLevel { LevelId = 3, LevelName = "Advanced", Difficulty = "Hard" }
            );

            builder.Entity<Region>().HasData(
                new Region { RegionId = 1, RegionName = "North America" },
                new Region { RegionId = 2, RegionName = "Europe" },
                new Region { RegionId = 3, RegionName = "Asia" }
            );

            builder.Entity<Role>().HasData(
                new Role { RoleId = 1, Name = "Admin" },
                new Role { RoleId = 2, Name = "User" }
            );

            builder.Entity<Question>().HasData(
                new Question { QuestionId = 1, QuestionText = "What is the capital of France?", Answer = "Paris", Options = "Paris;London;Berlin;Rome", LevelId = 1 },
                new Question { QuestionId = 2, QuestionText = "Which planet is known as the Red Planet?", Answer = "Mars", Options = "Mercury;Venus;Earth;Mars", LevelId = 1 },
                new Question { QuestionId = 3, QuestionText = "What is the largest ocean on Earth?", Answer = "Pacific", Options = "Atlantic;Pacific;Indian;Arctic", LevelId = 2 },
                new Question { QuestionId = 4, QuestionText = "Who wrote 'Romeo and Juliet'?", Answer = "William Shakespeare", Options = "Charles Dickens;William Shakespeare;Mark Twain;Jane Austen", LevelId = 2 },
                new Question { QuestionId = 5, QuestionText = "What is the chemical symbol for Gold?", Answer = "Au", Options = "Ag;Au;Gd;Pt", LevelId = 3 },
                new Question { QuestionId = 6, QuestionText = "In which year did the Titanic sink?", Answer = "1912", Options = "1905;1912;1920;1898", LevelId = 3 }
            );
        }


    }
}