using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.Entities;

public class Status
{
    [Key]
    public string StatusId { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage ="Please give status name!")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Subject> Subjects { get; set; }
        = new List<Subject>();
}
