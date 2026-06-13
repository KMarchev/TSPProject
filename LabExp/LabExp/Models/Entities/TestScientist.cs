using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.Entities;

public class TestScientist
{
    [Key]
    public string TestScientistId { get; set; }
        = Guid.NewGuid().ToString();

    public string TestId { get; set; } = string.Empty;

    public string ScientistId { get; set; } = string.Empty;


    public Test Test { get; set; } = null!;

    public Scientist Scientist { get; set; } = null!;
}
