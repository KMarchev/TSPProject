using LabExp.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LabExp.Data;

public class ApplicationDbContext
    : IdentityDbContext<Scientist, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Test> Tests { get; set; }
    public DbSet<Scientist> Scientists { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Substance> Substances { get; set; }
    public DbSet<Clearance> Clearances { get; set; }
    public DbSet<Severity> Severities { get; set; }
    public DbSet<Gender> Genders { get; set; }
    public DbSet<Status> Statuses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
