using LabExp.Models.Entities;
using LabExp.Models.OtherModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LabExp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Scientist> _signInManager;
        private readonly UserManager<Scientist> _userManager;

        public AccountController(SignInManager<Scientist> signInManager, UserManager<Scientist> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            Console.WriteLine("STEP 1");

            var user = await _userManager.FindByEmailAsync(model.Email);

            Console.WriteLine("STEP 2");

            if (user == null)
            {
                return View(model);
            }

            Console.WriteLine("STEP 3");

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                model.Password,
                false);

            Console.WriteLine("STEP 4");
            Console.WriteLine(result.ToString());
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
