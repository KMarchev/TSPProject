using LabExp.Data;
using LabExp.Models.Entities;
using LabExp.Models.ScientistModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabExp.Controllers
{

    public class AdminController : Controller
    {
        private readonly UserManager<Scientist> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<Scientist> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
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
                vm.Clearances = await _context.Clearances.ToListAsync();
                return View("ScientistForm", vm);
            }

            var scientist = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == vm.Id);

            if (scientist == null)
                return NotFound();

            scientist.UserName = vm.UserName;
            scientist.Email = vm.Email;
            scientist.ClearanceId = vm.ClearanceId;

            await _userManager.UpdateAsync(scientist);

            var currentRoles = await _userManager.GetRolesAsync(scientist);

            if (!currentRoles.Contains(vm.Role))
            {
                await _userManager.RemoveFromRolesAsync(scientist, currentRoles);
                await _userManager.AddToRoleAsync(scientist, vm.Role);
            }

            return RedirectToAction(nameof(ManageScientists));
        }
    }
}
