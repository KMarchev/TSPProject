using LabExp.Data;
using LabExp.Models.SubstanceModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabExp.Controllers
{
    public class SubstanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubstanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Substances()
        {
            if (int.Parse(User.FindFirst("ClearanceLevel")?.Value ?? "0") < 2)
            {
                return Forbid();
            }

            var model = _context.Substances
                .Include(s => s.Severity)
                .OrderBy(s => s.Severity!.SeverityLevel)
                .ThenBy(s=>s.SubstanceId)
                .Select(s => new SubstanceModel
                {
                    Id = s.SubstanceId,
                    Name = s.Name!,
                    Description = s.Description,
                    Severity = s.Severity!.SeverityName,
                })
                .ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var substance = await _context.Substances
                .FirstOrDefaultAsync(s => s.SubstanceId == id);

            if (substance == null)
            {
                return NotFound();
            }

            var tests = await _context.Tests
                .Where(t => t.SubstanceId == id)
                .OrderBy(t => t.Number)
                .ToListAsync();

            if (tests.Any())
            {
                TempData["DeleteError"] =
                    $"<strong>Cannot delete {substance.Name}.</strong><br/>" +
                    "The substance is used in:" +
                    "<ul>" +
                    string.Join("", tests.Select(t =>
                        $"<li>Test #{t.Number} - {t.Name}</li>")) +
                    "</ul>";

                return RedirectToAction(nameof(Substances));
            }

            _context.Substances.Remove(substance);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Substances));
        }
    }
}