using LabExp.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LabExp.Controllers
{

    public class AdminController : Controller
    {
        private readonly UserManager<Scientist> _userManager;

        public AdminController(UserManager<Scientist> userManager)
        {
            _userManager = userManager;
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
            string clearanceId)
        {
            var existing = await _userManager.FindByEmailAsync(email);

            if (existing != null)
            {
                Console.WriteLine("Email already exists: " + email);
                return RedirectToAction("Index");
            }

            var scientist = new Scientist
            {
                UserName = userName,
                Email = email,
                ClearanceId = clearanceId
            };

            var result = await _userManager.CreateAsync(scientist, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(scientist, role);
                Console.WriteLine("Created successfully");
            }
            else
            {
                foreach (var error in result.Errors)
                    Console.WriteLine(error.Description);
            }

            return RedirectToAction("Index");
        }

        public Task<IActionResult> CreateAdminScientist()
        {
            return CreateScientistUser(
                "K",
                "K123456789@secretcorp.com",
                "StrongPassword123",
                "Admin",
                "865E2C76-B415-4387-ACAA-6514A4023BF6");
        }

        public Task<IActionResult> CreateLevelOneScientistOne()
        {
            return CreateScientistUser(
                "SM",
                "SM123456789@secretcorp.com",
                "Sm123123",
                "Scientist",
                "76B2B72E-9967-4768-9C7C-E77B3F18002E");
        }

        public Task<IActionResult> CreateLevelOneScientistTwo()
        {
            return CreateScientistUser(
                "AG",
                "AG123456789@secretcorp.com",
                "Ag123123",
                "Scientist",
                "76B2B72E-9967-4768-9C7C-E77B3F18002E");
        }

        public IActionResult GoBack()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
