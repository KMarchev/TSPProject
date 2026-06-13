using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace LabExp.Models.Entities;

public class Scientist : IdentityUser
{

    [Required(ErrorMessage = "Please select clearance level!")]
    public string? ClearanceId { get; set; }

    public Clearance? Clearance { get; set; }

    public ICollection<TestScientist> TestScientists { get; set; }
        = new List<TestScientist>();
}
