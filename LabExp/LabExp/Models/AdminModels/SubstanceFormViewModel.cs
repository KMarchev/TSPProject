using LabExp.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.AdminModels
{
    public class SubstanceFormViewModel
    {
        public Guid SubstanceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public Guid SeverityId { get; set; }

        public List<Severity> Severities { get; set; } = new();
    }
}
