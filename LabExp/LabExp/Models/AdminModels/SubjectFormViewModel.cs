using LabExp.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.AdminModels
{
    public class SubjectFormViewModel
    {
        public Guid SubjectId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        public int Age { get; set; }

        [Required]
        public Guid StatusId { get; set; }

        [Required]
        public Guid GenderId { get; set; }

        public List<Status> Statuses { get; set; } = new();

        public List<Gender> Genders { get; set; } = new();
    }
}
