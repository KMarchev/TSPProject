using LabExp.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.TestModels
{
    public class CreateTestViewModel
    {
        [Required(ErrorMessage = "Test name is required")]
        public string NameInput { get; set; }

        [Required(ErrorMessage = "You must select a subject")]
        public Guid SubjectId { get; set; }

        [Required(ErrorMessage = "You must select a substance")]
        public Guid SubstanceId { get; set; }

        [Required(ErrorMessage ="You must select a new status for the subject!")]
        public Guid SubjectStatusId { get; set; }

        [Required(ErrorMessage = "Select at least one scientist")]
        public List<Guid> ScientistIds { get; set; } = new();

        [MaxLength(3000,ErrorMessage = "Cannot be more that 3000 characters!")]
        public string? Description { get; set; }

        public List<Subject> Subjects { get; set; } = new();
        public List<Substance> Substances { get; set; } = new();
        public List<Scientist> Scientists { get; set; } = new();

        public List<Status> Statuses { get; set; } = new();
    }
}