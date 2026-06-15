using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.Entities;
public class Gender
{
    [Key]
    public Guid GenderId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Subject> Subjects { get; set; }
        = new List<Subject>();
}
