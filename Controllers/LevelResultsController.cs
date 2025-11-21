using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GAM106_LAB.Data;
using GAM106_LAB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAM106_LAB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LevelResultsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LevelResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/LevelResults
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LevelResult>>> Get()
        {
            return await _context.LevelResults.ToListAsync();
        }

        // GET: api/LevelResults/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LevelResult>> Get(int id)
        {
            var item = await _context.LevelResults.FindAsync(id);
            if (item == null)
                return NotFound();
            return item;
        }

        // POST: api/LevelResults
        [HttpPost]
        public async Task<ActionResult<LevelResult>> Post([FromBody] LevelResult model)
        {
            // Prevent inserting explicit primary key value (avoid UNIQUE constraint failures)
            model.QuizResultId = 0;

            // Basic validation
            if (string.IsNullOrWhiteSpace(model.UserId) || model.LevelId <= 0)
            {
                return BadRequest("Invalid level result data.");
            }

            _context.LevelResults.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = model.QuizResultId }, model);
        }
    }
}
