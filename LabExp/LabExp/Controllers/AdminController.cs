using LabExp.Data;
using LabExp.Models.Entities;
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
    }
}
