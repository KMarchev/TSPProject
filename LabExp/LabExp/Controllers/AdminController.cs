using LabExp.Data;
using LabExp.Models.AdminModels;
using LabExp.Models.AuditLogModels;
using LabExp.Models.Entities;
using LabExp.Models.Interfaces;
using LabExp.Models.ScientistModels;
using LabExp.Models.SubjectModels;
using LabExp.Models.SubstanceModels;
using LabExp.Models.TestModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabExp.Controllers
{

    public class AdminController : Controller
    {
        private readonly UserManager<Scientist> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public AdminController(UserManager<Scientist> userManager, ApplicationDbContext context, IAuditService auditService)
        {
            _userManager = userManager;
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalScientists = await _context.Users.CountAsync(),
                TotalSubjects = await _context.Subjects.CountAsync(),
                TotalTests = await _context.Tests.CountAsync(),
                TotalSubstances = await _context.Substances.CountAsync(),
                TotalStatuses = await _context.Statuses.CountAsync(),
                TotalSeverities = await _context.Severities.CountAsync(),
                TotalClearances = await _context.Clearances.CountAsync(),

                RecentTests = await _context.Tests
                    .Include(t => t.Subject)
                    .Include(t => t.Substance)
                    .OrderByDescending(t => t.Number)
                    .Take(5)
                    .Select(t => new RecentTestViewModel
                    {
                        Number = t.Number,
                        TestName = t.Name,
                        Subject = t.Subject!.Name!,
                        Substance = t.Substance!.Name!
                    })
                    .ToListAsync()
            };

            return View(vm);
        }

        private async Task<IActionResult> CreateScientistUser(
            string userName,
            string email,
            string password,
            string role,
            string clearanceName)
        {
            var existing = await _userManager.FindByEmailAsync(email);

            if (existing != null)
            {
                Console.WriteLine("Scientist already exists!");
                return RedirectToAction("Index");
            }


            var clearance = await _context.Clearances
                .FirstOrDefaultAsync(c => c.LevelName == clearanceName);

            if (clearance == null)
            {
                Console.WriteLine("Clearance not found: " + clearanceName);
                return RedirectToAction("Index");
            }

            var scientist = new Scientist
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
                ClearanceId = clearance.ClearanceId
            };

            var result = await _userManager.CreateAsync(scientist, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(scientist, role);
            }
            else
            {
                foreach (var error in result.Errors)
                    Console.WriteLine(error.Description);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CreateAdminScientist()
        {
            return await CreateScientistUser(
                "Ta4",
                "Ta4@secretcorp.com",
                "Ta4123123",
                "Scientist",
                "Junior Scientist"
            );
        }

        public async Task<IActionResult> CreateLevelOneScientistOne()
        {
            return await CreateScientistUser(
                "SM",
                "SM123456789@secretcorp.com",
                "Sm123123",
                "Scientist",
                "Junior Scientist"
            );
        }

        public async Task<IActionResult> CreateLevelOneScientistTwo()
        {
            return await CreateScientistUser(
                "AG",
                "AG123456789@secretcorp.com",
                "Ag123123",
                "Scientist",
                "Scientist"
            );
        }

        public IActionResult GoBack()
        {
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ManageScientists()
        {
            var model = _context.Users
                .Include(s => s.Clearance)
                .OrderByDescending(s => s.Clearance!.LevelPriority)
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
        public async Task<IActionResult> DeleteScientist(Guid id)
        {
            var scientist = await _context.Users
                .FirstOrDefaultAsync(s => s.Id == id);

            if (scientist == null)
            {
                return NotFound();
            }

            var tests = await _context.Tests
                .Where(t => t.Scientists.Any(s => s.Id == id))
                .OrderBy(t => t.Number)
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

                return RedirectToAction(nameof(ManageScientists));
            }

            await _userManager.DeleteAsync(scientist);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult AddScientist()
        {
            var vm = new ScientistFormViewModel
            {
                Clearances = _context.Clearances
                    .OrderBy(c => c.LevelPriority)
                    .ToList()
            };

            return View("ScientistForm", vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddScientist(ScientistFormViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                ModelState.AddModelError(nameof(vm.Password), "Password is required.");
                vm.Clearances = await _context.Clearances.ToListAsync();
                return View("ScientistForm", vm);
            }


            var existingUser = await _userManager.FindByEmailAsync(vm.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(vm.Email), "This email is already in use.");
                vm.Clearances = await _context.Clearances.ToListAsync();
                return View("ScientistForm", vm);
            }

            var scientist = new Scientist
            {
                UserName = vm.UserName,
                Email = vm.Email,
                EmailConfirmed = true,
                ClearanceId = vm.ClearanceId
            };

            var result = await _userManager.CreateAsync(scientist, vm.Password!);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                vm.Clearances = _context.Clearances.ToList();
                return View("ScientistForm", vm);
            }

            await _userManager.AddToRoleAsync(scientist, vm.Role);

            return RedirectToAction(nameof(ManageScientists));
        }

        [HttpGet]
        public async Task<IActionResult> EditScientist(Guid id)
        {
            var currentUserId = _userManager.GetUserId(User);

            if (id.ToString() == currentUserId)
            {
                return Forbid();
            }

            var scientist = await _context.Users
                .Include(x => x.Clearance)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (scientist == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(scientist);

            var vm = new ScientistFormViewModel
            {
                Id = scientist.Id,
                UserName = scientist.UserName!,
                Email = scientist.Email!,
                ClearanceId = scientist.ClearanceId,
                Role = roles.FirstOrDefault() ?? "Scientist",
                Clearances = await _context.Clearances.ToListAsync()
            };

            return View("ScientistForm", vm);
        }
        [HttpPost]
        public async Task<IActionResult> EditScientist(ScientistFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Clearances = await _context.Clearances
                    .OrderBy(x => x.LevelName)
                    .ToListAsync();

                return View("ScientistForm", vm);
            }

            var scientist = await _userManager.FindByIdAsync(vm.Id.ToString());

            if (scientist == null)
                return NotFound();

            bool usernameExists = await _userManager.Users.AnyAsync(x =>
                x.Id != vm.Id &&
                x.UserName == vm.UserName);

            if (usernameExists)
            {
                ModelState.AddModelError(nameof(vm.UserName),
                    "This username is already taken.");

                vm.Clearances = await _context.Clearances.ToListAsync();
                return View("ScientistForm", vm);
            }

            bool emailExists = await _userManager.Users.AnyAsync(x =>
                x.Id != vm.Id &&
                x.Email == vm.Email);

            if (emailExists)
            {
                ModelState.AddModelError(nameof(vm.Email),
                    "This email is already in use.");

                vm.Clearances = await _context.Clearances.ToListAsync();
                return View("ScientistForm", vm);
            }

            scientist.UserName = vm.UserName;
            scientist.Email = vm.Email;
            scientist.ClearanceId = vm.ClearanceId;

            var updateResult = await _userManager.UpdateAsync(scientist);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                vm.Clearances = await _context.Clearances.ToListAsync();
                return View("ScientistForm", vm);
            }

            var currentRoles = await _userManager.GetRolesAsync(scientist);

            if (!currentRoles.Contains(vm.Role))
            {
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(scientist, currentRoles);
                }

                await _userManager.AddToRoleAsync(scientist, vm.Role);
            }

            return RedirectToAction(nameof(ManageScientists));
        }

        public async Task<IActionResult> ManageSubstances()
        {
            var model = await _context.Substances
                .Include(s => s.Severity)
                .OrderBy(s => s.Severity!.SeverityLevel)
                .Select(s => new SubstanceModel
                {
                    Id = s.SubstanceId,
                    Name = s.Name!,
                    Description = s.Description,
                    Severity = s.Severity!.SeverityName
                })
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AddSubstance()
        {
            var vm = new SubstanceFormViewModel
            {
                Severities = await _context.Severities
                    .OrderBy(s => s.SeverityLevel)
                    .ToListAsync()
            };

            return View("SubstanceForm", vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddSubstance(SubstanceFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Severities = await _context.Severities.ToListAsync();
                return View("SubstanceForm", vm);
            }

            var existingSubstance = await _context.Substances
                .AnyAsync(s => s.Name == vm.Name);

            if (existingSubstance)
            {
                ModelState.AddModelError(nameof(vm.Name), "A substance with this name already exists.");

                vm.Severities = await _context.Severities.ToListAsync();
                return View("SubstanceForm", vm);
            }

            var substance = new Substance
            {
                Name = vm.Name,
                Description = vm.Description,
                SeverityId = vm.SeverityId
            };

            _context.Substances.Add(substance);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageSubstances));
        }

        [HttpGet]
        public async Task<IActionResult> EditSubstance(Guid id)
        {
            var substance = await _context.Substances
                .FirstOrDefaultAsync(x => x.SubstanceId == id);

            if (substance == null)
                return NotFound();

            var vm = new SubstanceFormViewModel
            {
                SubstanceId = substance.SubstanceId,
                Name = substance.Name!,
                Description = substance.Description,
                SeverityId = substance.SeverityId,
                Severities = await _context.Severities
                    .OrderBy(s => s.SeverityLevel)
                    .ThenBy(s => s.SeverityName)
                    .ThenBy(s => s.SeverityId)
                    .ToListAsync()
            };

            return View("SubstanceForm", vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditSubstance(SubstanceFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Severities = await _context.Severities
                    .OrderBy(s => s.SeverityLevel)
                    .ToListAsync();

                return View("SubstanceForm", vm);
            }

            var substance = await _context.Substances
                .FirstOrDefaultAsync(x => x.SubstanceId == vm.SubstanceId);

            if (substance == null)
                return NotFound();

            bool exists = await _context.Substances.AnyAsync(x =>
                x.SubstanceId != vm.SubstanceId &&
                x.Name == vm.Name);

            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Name),
                    "A substance with this name already exists.");

                vm.Severities = await _context.Severities
                    .OrderBy(s => s.SeverityLevel)
                    .ToListAsync();

                return View("SubstanceForm", vm);
            }

            substance.Name = vm.Name;
            substance.Description = vm.Description;
            substance.SeverityId = vm.SeverityId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageSubstances));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSubstance(Guid id)
        {
            var substance = await _context.Substances
                .Include(s => s.Tests)
                .FirstOrDefaultAsync(s => s.SubstanceId == id);

            if (substance == null)
                return NotFound();

            if (substance.Tests.Any())
            {
                TempData["DeleteError"] =
                    $"<strong>Cannot delete {substance.Name}.</strong><br/>" +
                    "The substance is assigned to:" +
                    "<ul>" +
                    string.Join("", substance.Tests.Select(t =>
                        $"<li>Test #{t.Number} - {t.Name}</li>")) +
                    "</ul>";

                return RedirectToAction(nameof(ManageSubstances));
            }

            _context.Substances.Remove(substance);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageSubstances));
        }

        public async Task<IActionResult> ManageSubjects()
        {
            var model = await _context.Subjects
                .Include(s => s.Status)
                .Include(s => s.Gender)
                .OrderBy(s => s.Name)
                .Select(s => new SubjectModel
                {
                    Id = s.SubjectId,
                    Name = s.Name!,
                    Age = s.Age,
                    Status = s.Status!.Name,
                    Gender = s.Gender!.Name
                })
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AddSubject()
        {
            var vm = new SubjectFormViewModel
            {
                Statuses = await _context.Statuses
                    .OrderBy(s => s.Name)
                    .ToListAsync(),

                Genders = await _context.Genders
                    .OrderByDescending(g => g.Name)
                    .ToListAsync()
            };

            return View("SubjectForm", vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddSubject(SubjectFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Statuses = await _context.Statuses.ToListAsync();
                vm.Genders = await _context.Genders.ToListAsync();

                return View("SubjectForm", vm);
            }

            bool exists = await _context.Subjects
                .AnyAsync(x => x.Name == vm.Name);

            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Name),
                    "A subject with this name already exists.");

                vm.Statuses = await _context.Statuses.ToListAsync();
                vm.Genders = await _context.Genders.ToListAsync();

                return View("SubjectForm", vm);
            }

            var subject = new Subject
            {
                Name = vm.Name,
                Age = vm.Age,
                StatusId = vm.StatusId,
                GenderId = vm.GenderId
            };

            _context.Subjects.Add(subject);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageSubjects));
        }

        [HttpGet]
        public async Task<IActionResult> EditSubject(Guid id)
        {
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(x => x.SubjectId == id);

            if (subject == null)
                return NotFound();

            var vm = new SubjectFormViewModel
            {
                SubjectId = subject.SubjectId,
                Name = subject.Name!,
                Age = subject.Age,
                StatusId = subject.StatusId,
                GenderId = subject.GenderId,

                Statuses = await _context.Statuses
                    .OrderByDescending(s => s.Name)
                    .ToListAsync(),

                Genders = await _context.Genders
                    .OrderBy(g => g.Name)
                    .ToListAsync()
            };

            return View("SubjectForm", vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditSubject(SubjectFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Statuses = await _context.Statuses
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                vm.Genders = await _context.Genders
                    .OrderBy(g => g.Name)
                    .ToListAsync();

                return View("SubjectForm", vm);
            }

            var subject = await _context.Subjects
                .FirstOrDefaultAsync(x => x.SubjectId == vm.SubjectId);

            if (subject == null)
                return NotFound();

            bool exists = await _context.Subjects.AnyAsync(x =>
                x.SubjectId != vm.SubjectId &&
                x.Name == vm.Name);

            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Name),
                    "A subject with this name already exists.");

                vm.Statuses = await _context.Statuses
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                vm.Genders = await _context.Genders
                    .OrderBy(g => g.Name)
                    .ToListAsync();

                return View("SubjectForm", vm);
            }

            subject.Name = vm.Name;
            subject.Age = vm.Age;
            subject.StatusId = vm.StatusId;
            subject.GenderId = vm.GenderId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageSubjects));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSubject(Guid id)
        {
            var subject = await _context.Subjects
                .Include(s => s.Tests)
                .FirstOrDefaultAsync(x => x.SubjectId == id);

            if (subject == null)
                return NotFound();

            if (subject.Tests.Any())
            {
                TempData["DeleteError"] =
                    $"<strong>Cannot delete {subject.Name}.</strong><br/>" +
                    "The subject is assigned to:" +
                    "<ul>" +
                    string.Join("", subject.Tests.Select(t =>
                        $"<li>Test #{t.Number} - {t.Name}</li>")) +
                    "</ul>";

                return RedirectToAction(nameof(ManageSubjects));
            }

            _context.Subjects.Remove(subject);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageSubjects));
        }

        public IActionResult AuditLogs()
        {
            var logs = _context.AuditLogs
                .OrderByDescending(x => x.TimeStamp)
                .Select(x => new AuditLogViewModel
                {
                    TimeStamp = x.TimeStamp,
                    UserName = x.UserName,
                    Action = x.Action,
                    EntityName = x.EntityName,
                    EntityId = x.EntityId
                })
                .ToList();

            return View(logs);
        }

        private async Task LoadTestLists(TestFormViewModel vm)
        {
            vm.Subjects = await _context.Subjects
                .OrderBy(s => s.Name)
                .ToListAsync();


            vm.Statuses = await _context.Statuses
                .OrderBy(s => s.Name)
                .ToListAsync();


            vm.Substances = await _context.Substances
                .Include(s => s.Severity)
                .OrderBy(s => s.Name)
                .ToListAsync();


            vm.Scientists = await _userManager.Users
                .Include(u => u.Clearance)
                .OrderBy(u => u.UserName)
                .ToListAsync();
        }

        public async Task<IActionResult> ManageTests()
        {
            var userId = Guid.Parse(_userManager.GetUserId(User)!);

            var query = _context.Tests
                .Include(t => t.Subject)
                .Include(t => t.Substance)
                .Include(t => t.Scientists)
                .AsQueryable();

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

        [HttpGet]
        public async Task<IActionResult> AddTest()
        {
            var vm = new TestFormViewModel();

            await LoadTestLists(vm);

            return View("TestForm", vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddTest(TestFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadTestLists(vm);
                return View("TestForm", vm);
            }


            bool exists = await _context.Tests
                .AnyAsync(t => t.Name == vm.Name);


            if (exists)
            {
                ModelState.AddModelError(nameof(vm.Name),
                    "A test with this name already exists.");

                await LoadTestLists(vm);

                return View("TestForm", vm);
            }


            int nextNumber = await _context.Tests.AnyAsync()
                ? await _context.Tests.MaxAsync(t => t.Number) + 1
                : 1;



            var scientists = await _context.Scientists
                .Where(s => vm.ScientistIds.Contains(s.Id))
                .ToListAsync();



            var test = new Test
            {
                Number = nextNumber,

                Name = vm.Name,

                Description = vm.Description,

                SubjectId = vm.SubjectId,

                SubstanceId = vm.SubstanceId,

                StatusId = vm.StatusId,

                Scientists = scientists
            };

            _context.Tests.Add(test);

            var subject = await _context.Subjects
                .FirstAsync(s => s.SubjectId == vm.SubjectId);


            subject.StatusId = vm.StatusId;



            await _context.SaveChangesAsync();


            await _auditService.LogAsync(
                "Created",
                "Test",
                test.TestId);



            return RedirectToAction(nameof(ManageTests));
        }

        [HttpGet]
        public async Task<IActionResult> EditTest(Guid id)
        {
            var test = await _context.Tests
                .Include(t => t.Scientists)
                .FirstOrDefaultAsync(t => t.TestId == id);

            if (test == null)
                return NotFound();


            var vm = new TestFormViewModel
            {
                TestId = test.TestId,

                Number = test.Number,
                Name = test.Name,
                Description = test.Description,

                SubstanceId = test.SubstanceId,
                SubjectId = test.SubjectId,
                StatusId = test.StatusId,


                ScientistIds = test.Scientists
                    .Select(s => s.Id)
                    .ToList()
            };


            await LoadTestLists(vm);


            return View("TestForm", vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditTest(TestFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadTestLists(vm);
                return View("TestForm", vm);
            }

            var test = await _context.Tests
                .Include(t => t.Scientists)
                .FirstOrDefaultAsync(t => t.TestId == vm.TestId);

            if (test == null)
                return NotFound();

            test.Name = vm.Name;
            test.Description = vm.Description;
            test.SubjectId = vm.SubjectId;
            test.SubstanceId = vm.SubstanceId;
            test.StatusId = vm.StatusId;



            var scientists = await _context.Scientists
                .Where(s => vm.ScientistIds.Contains(s.Id))
                .ToListAsync();
            test.Scientists.Clear();

            foreach (var scientist in scientists)
            {
                test.Scientists.Add(scientist);
            }

            var subject = await _context.Subjects
                .FirstAsync(s => s.SubjectId == vm.SubjectId);

            subject.StatusId = vm.StatusId;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                "Edited",
                "Test",
                test.TestId);

            return RedirectToAction(nameof(ManageTests));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTest(Guid id)
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

            await _auditService.LogAsync(
                "Deleted",
                "Test",
                test.TestId
            );

            return RedirectToAction(nameof(ManageTests));
        }

        [HttpGet]
        public IActionResult TestDetails(Guid id)
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
                Name = clearanceLevel >= 2 ? test.Name : "[REDACTED]",
                Description = test.Description,
                Subject = clearanceLevel >= 2 ? test.Subject!.Name : "[REDACTED]",
                Substance = clearanceLevel >= 2 ? test.Substance!.Name + " - " + test.Substance!.Severity!.SeverityName : "████████████",
                Status = clearanceLevel >= 2 ? test.Status!.Name : "Unknown",
                Scientists = test.Scientists
                    .Select(s => clearanceLevel >= 2 ? s.UserName! : "[REDACTED]")
                    .ToList()
            };

            _auditService.LogAsync("Viewed", "Test", test.TestId);

            return View(model);
        }
    }
}
