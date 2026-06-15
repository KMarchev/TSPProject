using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.Entities;

public class Status
{
    [Key]
    public Guid StatusId { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage ="Please give status name!")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Subject> Subjects { get; set; }
        = new List<Subject>();
}
