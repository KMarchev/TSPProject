using LabExp.Data;
using LabExp.Models.AccountModels;
using LabExp.Models.Entities;
using LabExp.Models.Interfaces;
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
        private readonly IAuditService _auditService;

        public AccountController(
            SignInManager<Scientist> signInManager,
            UserManager<Scientist> userManager,
            ApplicationDbContext context,
            IAuditService auditService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _auditService = auditService;
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

            identity.AddClaim(
                new Claim(
                    ClaimTypes.Name,
                    user.UserName!
                )
            );

            await HttpContext.SignInAsync(
                IdentityConstants.ApplicationScheme,
                principal
            );

            HttpContext.User = principal;

            await _auditService.LogAsync(
                "Login",
                "Account",
                user.Id
            );

            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> LogOut()
        {
            var userIdString = _userManager.GetUserId(User);

            if (Guid.TryParse(userIdString, out var userId))
            {
                await _auditService.LogAsync(
                    "Logout",
                    "Account",
                    userId
                );
            }

            await _signInManager.SignOutAsync();

            return RedirectToAction("Login", "Account");
        }
    }
}