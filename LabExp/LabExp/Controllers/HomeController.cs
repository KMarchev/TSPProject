using LabExp.Models;
using LabExp.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LabExp.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<Scientist> _signInManager;
        private readonly UserManager<Scientist> _userManager;

        public HomeController(SignInManager<Scientist> signInManager, UserManager<Scientist> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            if (!_signInManager.IsSignedIn(User)) return RedirectToAction("Login", "Account");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Tests()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
