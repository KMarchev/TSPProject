using LabExp.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LabExp.Data;

public class ApplicationDbContext : IdentityDbContext<Scientist>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Test> Tests { get; set; }
    public DbSet<Scientist> Scientists { get; set; }
    public DbSet<TestScientist> TestScientists { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Substance> Substances { get; set; }
    public DbSet<Severity> Severities { get; set; }
    public DbSet<Status> Statuses { get; set; }
    public DbSet<Gender> Genders { get; set; }
    public DbSet<Clearance> Clearances { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TestScientist>()
            .HasOne(ts => ts.Test)
            .WithMany(t => t.TestScientists)
            .HasForeignKey(ts => ts.TestId);


        builder.Entity<TestScientist>()
            .HasOne(ts => ts.Scientist)
            .WithMany(s => s.TestScientists)
            .HasForeignKey(ts => ts.ScientistId)
            .HasPrincipalKey(s => s.Id);

        builder.Entity<Test>()
            .HasOne(t => t.Subject)
            .WithMany(s => s.Tests)
            .HasForeignKey(t => t.SubjectId);

        builder.Entity<Test>()
            .HasOne(t => t.Substance)
            .WithMany(s => s.Tests)
            .HasForeignKey(t => t.SubstanceId);

        builder.Entity<Subject>()
            .HasOne(s => s.Gender)
            .WithMany(g => g.Subjects)
            .HasForeignKey(s => s.GenderId);

        builder.Entity<Subject>()
            .HasOne(s => s.Status)
            .WithMany(st => st.Subjects)
            .HasForeignKey(s => s.StatusId);

        builder.Entity<Substance>()
            .HasOne(s => s.Severity)
            .WithMany(se => se.Substances)
            .HasForeignKey(s => s.SeverityId);

        builder.Entity<Scientist>()
            .HasOne(s => s.Clearance)
            .WithMany(c => c.Scientists)
            .HasForeignKey(s => s.ClearanceId);
    }
}
