using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GAM106.Data;
using GAM106.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GAM106.Controllers
{
    public class LevelResultsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public LevelResultsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Admin: view all results
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var results = await _db.LevelResults
                .Include(r => r.User)
                .Include(r => r.Level)
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync();
            return View(results);
        }

        // Authenticated user: view own results
        [Authorize]
        public async Task<IActionResult> MyResults()
        {
            var userId = _userManager.GetUserId(User);
            var results = await _db.LevelResults
                .Where(r => r.UserId == userId)
                .Include(r => r.Level)
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync();
            return View(results);
        }

        // Show create form for posting a new result (authenticated users)
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var levels = await _db.GameLevels.OrderBy(l => l.LevelId).ToListAsync();
            ViewBag.Levels = levels;
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int levelId, int score)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var level = await _db.GameLevels.FindAsync(levelId);
            if (level == null)
            {
                ModelState.AddModelError("LevelId", "Invalid level selected.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Levels = await _db.GameLevels.OrderBy(l => l.LevelId).ToListAsync();
                return View();
            }

            var result = new LevelResult
            {
                UserId = userId,
                LevelId = levelId,
                Score = score
            };
            _db.LevelResults.Add(result);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(MyResults));
        }
    }
}
