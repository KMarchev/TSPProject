using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.Entities;

public class Subject
{
    [Key]
    public string SubjectId { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "Please give name of subject!")]
    [MaxLength(100,ErrorMessage ="Max length of name is 100 characters!")]
    public string? Name { get; set; }

    [Required(ErrorMessage ="Please give age of subject!")]
    public int Age { get; set; }

    [Required(ErrorMessage ="Please select a status!")]
    public string? StatusId { get; set; }

    [Required(ErrorMessage = "Please select a gender!")]
    public string? GenderId { get; set; }

    public Status? Status { get; set; }

    public Gender? Gender { get; set; }

    public ICollection<Test> Tests { get; set; }
        = new List<Test>();
}
