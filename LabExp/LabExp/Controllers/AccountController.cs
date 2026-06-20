using LabExp.Data;
using LabExp.Models.AccountModels;
using LabExp.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LabExp.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<Scientist> _signInManager;
        private readonly UserManager<Scientist> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<Scientist> signInManager,
            UserManager<Scientist> userManager,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _context.Scientists
                .Include(u => u.Clearance)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError(nameof(model.Email), "No account exists with this email.");
                return View(model);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                model.Password,
                false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(nameof(model.Password), "Incorrect password.");
                return View(model);
            }

            if (user.Clearance == null)
            {
                ModelState.AddModelError("", "User has no assigned clearance level.");
                return View(model);
            }

            var clearanceLevel = user.Clearance.LevelPriority;
            var clearanceName = user.Clearance.LevelName;

            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            var identity = (ClaimsIdentity)principal.Identity!;

            foreach (var claim in identity.FindAll("ClearanceLevel").ToList())
            {
                identity.RemoveClaim(claim);
            }

            foreach (var claim in identity.FindAll("ClearanceName").ToList())
            {
                identity.RemoveClaim(claim);
            }

            
            identity.AddClaim(new Claim("ClearanceLevel", clearanceLevel.ToString()));
            identity.AddClaim(new Claim("ClearanceName", clearanceName));

            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}