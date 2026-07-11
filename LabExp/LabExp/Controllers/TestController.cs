using LabExp.Data;
using LabExp.Models.Entities;
using LabExp.Models.Interfaces;
using LabExp.Models.TestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

namespace LabExp.Controllers
{
    public class TestController : Controller
    {
        private readonly UserManager<Scientist> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public TestController(UserManager<Scientist> userManager, ApplicationDbContext context, IAuditService auditService)
        {
            _userManager = userManager;
            _context = context;
            _auditService = auditService;
        }

        public IActionResult Index()
        {
            var userId = Guid.Parse(_userManager.GetUserId(User)!);

            var query = _context.Tests
                .Include(t => t.Subject)
                .Include(t => t.Substance)
                .Include(t => t.Scientists)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(t => t.Scientists.Any(s => s.Id == userId));
            }

            var model = query
                .OrderBy(t => t.Number)
                .Select(t => new TestModel
                {
                    Id = t.TestId,
                    Number = t.Number,
                    Name = t.Name,
                    Subject = t.Subject!.Name,
                    Substance = t.Substance!.Name,
                    ScientistCount = t.Scientists.Count
                })
                .ToList();

            return View(model);
        }



        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AddTest()
        {
            var model = new CreateTestViewModel
            {
                Subjects = _context.Subjects
                    .OrderBy(s => s.Name)
                    .ThenBy(s => s.SubjectId)
                    .ToList(),

                Substances = _context.Substances
                    .Include(s => s.Severity)
                    .OrderBy(s => s.Severity.SeverityLevel)
                    .ThenBy(s => s.Name)
                    .ThenBy(s => s.SubstanceId)
                    .ToList(),

                Scientists = _context.Scientists
                    .Include(s => s.Clearance)
                    .OrderByDescending(s => s.Clearance.LevelPriority)
                    .ThenBy(s => s.UserName)
                    .ThenBy(s => s.Id)
                    .ToList(),

                Statuses = _context.Statuses
                    .OrderBy(s => s.Name)
                    .ToList()
            };

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AddTest(CreateTestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Subjects = _context.Subjects
                    .OrderBy(s => s.Name)
                    .ToList();

                model.Substances = _context.Substances
                    .Include(s => s.Severity)
                    .OrderBy(s => s.Name)
                    .ToList();

                model.Scientists = _context.Scientists
                    .Include(s => s.Clearance)
                    .OrderBy(s => s.UserName)
                    .ToList();

                model.Statuses = _context.Statuses
                    .OrderBy(s => s.Name)
                    .ToList();

                return View(model);
            }

            var subject = _context.Subjects
                .FirstOrDefault(s => s.SubjectId == model.SubjectId);

            var substance = _context.Substances
                .FirstOrDefault(s => s.SubstanceId == model.SubstanceId);

            var scientists = _context.Scientists
                .Where(s => model.ScientistIds.Contains(s.Id))
                .ToList();

            int nextNumber = _context.Tests.Any()
                ? _context.Tests.Max(t => t.Number) + 1
                : 1;

            var test = new Test
            {
                TestId = Guid.NewGuid(),
                Name = model.NameInput,
                Number = nextNumber,
                Description = model.Description,
                SubjectId = subject!.SubjectId,
                SubstanceId = substance!.SubstanceId,
                StatusId = model.SubjectStatusId,

                Scientists = scientists
            };

            _context.Tests.Add(test);

            subject = _context.Subjects
                .FirstOrDefault(s => s.SubjectId == model.SubjectId);

            subject!.StatusId = model.SubjectStatusId;

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            _auditService.LogAsync("Created", "Test", test.TestId);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Details(Guid id)
        {
            var test = _context.Tests
                .Include(t => t.Subject)
                    .ThenInclude(s => s.Status)
                .Include(t => t.Status)
                .Include(t => t.Substance)
                    .ThenInclude(s => s.Severity)
                .Include(t => t.Scientists)
                .FirstOrDefault(t => t.TestId == id);

            if (test == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var userId = Guid.Parse(_userManager.GetUserId(User)!);

                bool assignedToTest = test.Scientists
                    .Any(s => s.Id == userId);

                if (!assignedToTest)
                {
                    return Forbid();
                }
            }

            var clearanceLevel = int.Parse(User.FindFirst("ClearanceLevel")?.Value ?? "0");

            var model = new TestDetailsViewModel
            {
                Id = test.TestId,
                Number = test.Number,
                Name = clearanceLevel>=2 ? test.Name : "[REDACTED]",
                Description = test.Description,
                Subject = clearanceLevel >= 2 ? test.Subject!.Name : "[REDACTED]",
                Substance = clearanceLevel >= 2 ? test.Substance!.Name +" - "+ test.Substance!.Severity!.SeverityName : "████████████",
                Status = clearanceLevel >= 2 ? test.Status!.Name : "Unknown",
                Scientists = test.Scientists
                    .Select(s => clearanceLevel >= 2 ? s.UserName! : "[REDACTED]")
                    .ToList()
            };

            _auditService.LogAsync("Viewed", "Test", test.TestId);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var test = await _context.Tests
                .FirstOrDefaultAsync(x => x.TestId == id);

            if (test == null)
                return NotFound();


            int deletedNumber = test.Number;


            _context.Tests.Remove(test);



            var testsAbove = await _context.Tests
                .Where(x => x.Number > deletedNumber)
                .ToListAsync();



            foreach (var t in testsAbove)
            {
                t.Number--;
            }



            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Deleted", "Test", test.TestId);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ExportPdf(Guid id)
        {
            var test = _context.Tests
                .Include(t => t.Subject)
                    .ThenInclude(s => s.Status)
                .Include(t => t.Status)
                .Include(t => t.Substance)
                    .ThenInclude(s => s.Severity)
                .Include(t => t.Scientists)
                .FirstOrDefault(t => t.TestId == id);

            if (test == null)
                return NotFound();


            var model = new TestDetailsViewModel
            {
                Id = test.TestId,
                Number = test.Number,
                Name = test.Name,
                Description = test.Description,
                Subject = test.Subject!.Name,
                Substance = test.Substance!.Name +
                            " - " +
                            test.Substance.Severity!.SeverityName,
                Status = test.Status!.Name,
                Scientists = test.Scientists
                    .Select(s => s.UserName!)
                    .ToList()
            };

            _auditService.LogAsync("Downloaded", "Test", test.TestId);


            return new ViewAsPdf("Details", model)
            {
                FileName = $"Test_{test.Number}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins
                {
                    Top = 10,
                    Bottom = 10,
                    Left = 10,
                    Right = 10
                },
                CustomSwitches = "--print-media-type"
            };
        }
    }
}
