using LabExp.Models.AccountModels;
using LabExp.Models.Entities;
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
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(nameof(model.Email), "No account exists with this email.");
                return View(model);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                model.Password,
                false);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(nameof(model.Password), "Incorrect password.");
            return View(model);
        }

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
