using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.Entities;

public class Severity
{
    [Key]
    public Guid SeverityId { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage ="Please give a severity level!")]
    public int SeverityLevel { get; set; }

    [Required(ErrorMessage ="Please give a severity name!")]
    [MaxLength(50)]
    public string SeverityName { get; set; } = string.Empty;

    public ICollection<Substance> Substances { get; set; }
        = new List<Substance>();
}
