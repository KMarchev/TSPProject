using System.ComponentModel.DataAnnotations;
using LabExp.Models.Entities;

namespace LabExp.Models.ScientistModels
{
    public class ScientistFormViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string UserName { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        public Guid ClearanceId { get; set; }

        [Required]
        public string Role { get; set; } = "Scientist";

        public List<Clearance> Clearances { get; set; } = new();
    }
}