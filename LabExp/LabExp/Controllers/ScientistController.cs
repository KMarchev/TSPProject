using LabExp.Data;
using LabExp.Models.Entities;
using LabExp.Models.ScientistModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabExp.Controllers
{
    public class ScientistController : Controller
    {
        private readonly UserManager<Scientist> _userManager;
        private readonly ApplicationDbContext _context;

        public ScientistController(
            UserManager<Scientist> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            var model = _context.Users
                .Include(s => s.Clearance)
                .Where(c => c.Clearance!.LevelPriority<=1)
                .OrderBy(s => s.UserName)
                .Select(s => new ScientistModel
                {
                    Id = s.Id,
                    UserName = s.UserName!,
                    Email = s.Email!,
                    ClearanceName = s.Clearance != null
                        ? s.Clearance.LevelName
                        : "No Clearance"
                })
                .ToList();

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var scientist = await _context.Users
                .FirstOrDefaultAsync(s => s.Id == id);

            if (scientist == null)
            {
                return NotFound();
            }

            var tests = await _context.Tests
                .Where(t => t.Scientists.Any(s => s.Id == id))
                .OrderBy(t=>t.Number)
                .ToListAsync();

            if (tests.Any())
            {
                TempData["DeleteError"] =
                    $"<strong>Cannot delete {scientist.UserName}.</strong><br/>" +
                    "The scientist is assigned to:" +
                    "<ul>" +
                    string.Join("", tests.Select(t =>
                        $"<li>Test #{t.Number} - {t.Name}</li>")) +
                    "</ul>";

                return RedirectToAction(nameof(Index));
            }

            await _userManager.DeleteAsync(scientist);

            return RedirectToAction(nameof(Index));
        }
    }
}