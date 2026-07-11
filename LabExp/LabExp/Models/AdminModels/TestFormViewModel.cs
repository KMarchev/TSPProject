using LabExp.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.AdminModels
{
    public class TestFormViewModel
    {
        public Guid TestId { get; set; }


        [Required]
        public int Number { get; set; }


        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;


        [MaxLength(3000)]
        public string? Description { get; set; }


        [Required]
        public Guid SubstanceId { get; set; }


        [Required]
        public Guid SubjectId { get; set; }


        [Required]
        public Guid StatusId { get; set; }


        public List<Guid> ScientistIds { get; set; } = new();



        public List<Substance> Substances { get; set; } = new();

        public List<Subject> Subjects { get; set; } = new();

        public List<Status> Statuses { get; set; } = new();

        public List<Scientist> Scientists { get; set; } = new();
    }
}