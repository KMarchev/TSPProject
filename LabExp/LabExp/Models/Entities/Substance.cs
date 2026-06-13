using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.Entities;

public class Substance
{
    [Key]
    public string SubstanceId { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please select a severity level!")]
    public string? SeverityId { get; set; }

    public Severity? Severity { get; set; }

    public ICollection<Test> Tests { get; set; }
        = new List<Test>();
}
