using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace LabExp.Models.Entities;

public class Scientist : IdentityUser<Guid>
{

    [Required(ErrorMessage = "Please select clearance level!")]
    public Guid ClearanceId { get; set; }

    public Clearance? Clearance { get; set; }

    public ICollection<Test> Tests { get; set; }
        = new List<Test>();
}
