using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.Entities;

public class Test
{
    [Key]
    public Guid TestId { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Please give a number to the test!")]
    public int Number {  get; set; }

    [Required(ErrorMessage = "Please give a name to the test!")]
    [MaxLength(200)]
    public string Name {  get; set; }

    [MaxLength(3000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please select a substance!")]
    public Guid SubstanceId { get; set; }

    [Required(ErrorMessage = "Please select a subject!")]
    public Guid SubjectId { get; set; }

    [Required(ErrorMessage = "Please select a status!")]
    public Guid StatusId { get; set; }

    public Substance? Substance { get; set; }

    public Subject? Subject { get; set; }

    public Status? Status { get; set; }

    public ICollection<Scientist> Scientists { get; set; }
        = new List<Scientist>();
}
