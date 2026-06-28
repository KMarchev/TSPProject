using LabExp.Data;
using LabExp.Models.Entities;
using LabExp.Models.ScientistModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabExp.Controllers
{
    public class ScientistController : Controller
    {
        private readonly UserManager<Scientist> _userManager;
        private readonly ApplicationDbContext _context;

        public ScientistController(
            UserManager<Scientist> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            var model = _context.Users
                .Include(s => s.Clearance)
                .OrderBy(s => s.UserName)
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
    }
}