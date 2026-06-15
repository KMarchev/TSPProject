using System.ComponentModel.DataAnnotations;
namespace LabExp.Models.Entities;


public class Clearance
{
    [Key]
    public Guid ClearanceId { get; set; } = Guid.NewGuid();

    public int LevelPriority { get; set; }

    [Required]
    [MaxLength(50)]
    public string LevelName { get; set; } = string.Empty;

    public ICollection<Scientist> Scientists { get; set; }
        = new List<Scientist>();
}
