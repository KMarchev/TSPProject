using LabExp.Data;
using LabExp.Models.Entities;
using LabExp.Models.SubjectModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabExp.Controllers
{
    public class SubjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Scientist> _userManager;

        public SubjectController(UserManager<Scientist> userManager,ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Subjects()
        {
            if(int.Parse(User.FindFirst("ClearanceLevel")?.Value ?? "0") < 2)
            {
                return Forbid();
            }

            var model = _context.Subjects
                .Include(s => s.Status)
                .Include(s => s.Gender)
                .Select(s => new SubjectModel
                {
                    Id = s.SubjectId,
                    Name = s.Name!,
                    Age = s.Age,
                    Status = s.Status!.Name,
                    Gender = s.Gender!.Name
                })
                .ToList();

            return View(model);
        }
    }
}