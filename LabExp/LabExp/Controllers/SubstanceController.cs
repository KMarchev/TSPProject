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
    }
}