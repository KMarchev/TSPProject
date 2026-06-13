using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.Entities;

public class Test
{
    [Key]
    public string TestId { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(1500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please select a substance!")]
    public string? SubstanceId { get; set; }

    [Required(ErrorMessage = "Please select a subject!")]
    public string? SubjectId { get; set; }

    public Substance? Substance { get; set; }

    public Subject? Subject { get; set; }

    public ICollection<TestScientist> TestScientists { get; set; }
        = new List<TestScientist>();
}
