using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.Entities;

public class Substance
{
    [Key]
    public Guid SubstanceId { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Please select a severity name!")]
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please select a severity level!")]
    public Guid SeverityId { get; set; }

    public Severity? Severity { get; set; }

    public ICollection<Test> Tests { get; set; }
        = new List<Test>();
}
