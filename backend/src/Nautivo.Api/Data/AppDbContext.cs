using Microsoft.EntityFrameworkCore;
using Nautivo.Api.Domain;
using Nautivo.Api.Infrastructure;

namespace Nautivo.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, TenantContext tenant) : DbContext(options)
{
    // Bez wybranej organizacji filtr nie zwraca żadnych wierszy.
    private Guid? CurrentOrganizationId => tenant.OrganizationId;

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Vessel> Vessels => Set<Vessel>();
    public DbSet<VesselBlock> VesselBlocks => Set<VesselBlock>();
    public DbSet<Cruise> Cruises => Set<Cruise>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CruiseCost> CruiseCosts => Set<CruiseCost>();
    public DbSet<ContactActivity> ContactActivities => Set<ContactActivity>();
    public DbSet<ContactTask> ContactTasks => Set<ContactTask>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseSession> CourseSessions => Set<CourseSession>();
    public DbSet<CourseAttendance> CourseAttendance => Set<CourseAttendance>();
    public DbSet<CourseMaterial> CourseMaterials => Set<CourseMaterial>();
    public DbSet<CourseTest> CourseTests => Set<CourseTest>();
    public DbSet<CourseTestResult> CourseTestResults => Set<CourseTestResult>();
    public DbSet<Charter> Charters => Set<Charter>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Organization>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.ContactEmail).HasMaxLength(254);
            e.Property(x => x.Phone).HasMaxLength(40);
            e.Property(x => x.City).HasMaxLength(100);
            e.Property(x => x.TaxId).HasMaxLength(20);
            e.Property(x => x.Website).HasMaxLength(200);
        });

        b.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(254);
            e.Property(x => x.FullName).HasMaxLength(200);
        });

        b.Entity<Membership>(e =>
        {
            e.HasKey(x => new { x.UserId, x.OrganizationId });
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        });

        b.Entity<Vessel>(e =>
        {
            e.HasIndex(x => x.OrganizationId);
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Model).HasMaxLength(100);
            e.Property(x => x.RegistrationNumber).HasMaxLength(50);
            e.Property(x => x.LengthMeters).HasPrecision(5, 2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.CharterDayRate).HasPrecision(12, 2);
            e.OwnsMany(x => x.Checklist, c => c.ToJson());
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<Charter>(e =>
        {
            e.HasIndex(x => new { x.OrganizationId, x.VesselId, x.StartDate });
            e.HasIndex(x => new { x.OrganizationId, x.StartDate });
            e.Property(x => x.DayRate).HasPrecision(12, 2);
            e.Property(x => x.Total).HasPrecision(12, 2);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.ToTable(t => t.HasCheckConstraint("CK_Charters_Dates", "\"EndDate\" >= \"StartDate\""));
            e.HasOne(x => x.Vessel).WithMany().HasForeignKey(x => x.VesselId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Participant).WithMany().HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<VesselBlock>(e =>
        {
            e.HasIndex(x => new { x.OrganizationId, x.VesselId });
            e.Property(x => x.Reason).HasMaxLength(200);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<Cruise>(e =>
        {
            e.HasIndex(x => new { x.OrganizationId, x.StartDate });
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.DeparturePort).HasMaxLength(100);
            e.Property(x => x.ArrivalPort).HasMaxLength(100);
            e.OwnsMany(x => x.RoutePoints, r => r.ToJson());
            e.Property(x => x.YachtName).HasMaxLength(100);
            e.Property(x => x.YachtModel).HasMaxLength(100);
            e.Property(x => x.YachtLengthMeters).HasPrecision(5, 2);
            e.Property(x => x.Price).HasPrecision(12, 2);
            e.Property(x => x.Deposit).HasPrecision(12, 2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Vessel).WithMany().HasForeignKey(x => x.VesselId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<Participant>(e =>
        {
            e.HasIndex(x => x.OrganizationId);
            e.Property(x => x.FullName).HasMaxLength(200);
            e.Property(x => x.Email).HasMaxLength(254);
            e.Property(x => x.Phone).HasMaxLength(40);
            e.Property(x => x.Experience).HasMaxLength(200);
            e.Property(x => x.EmergencyContact).HasMaxLength(200);
            e.Property(x => x.Notes).HasMaxLength(4000);
            e.Property(x => x.Source).HasMaxLength(60);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<Enrollment>(e =>
        {
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Total).HasPrecision(12, 2);
            e.HasOne(x => x.Cruise).WithMany(x => x.Enrollments).HasForeignKey(x => x.CruiseId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Course).WithMany(x => x.Enrollments).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.ExamResult).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.CertificateNumber).HasMaxLength(60);
            // Zapis dotyczy dokładnie jednego: rejsu albo szkolenia.
            e.ToTable(t => t.HasCheckConstraint("CK_Enrollments_CruiseOrCourse", "(\"CruiseId\" IS NULL) <> (\"CourseId\" IS NULL)"));
            e.HasOne(x => x.Participant).WithMany(x => x.Enrollments).HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            // Baza pilnuje, że nikt nie jest zapisany podwójnie na ten sam rejs.
            e.HasIndex(x => new { x.CruiseId, x.ParticipantId }).IsUnique().HasFilter("\"Status\" <> 'Cancelled'");
            e.HasIndex(x => new { x.CourseId, x.ParticipantId }).IsUnique().HasFilter("\"Status\" <> 'Cancelled'");
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<Payment>(e =>
        {
            e.HasIndex(x => new { x.OrganizationId, x.PaidOn });
            e.Property(x => x.Amount).HasPrecision(12, 2);
            e.Property(x => x.Method).HasMaxLength(40);
            e.Property(x => x.Note).HasMaxLength(300);
            e.HasOne(x => x.Enrollment).WithMany(x => x.Payments).HasForeignKey(x => x.EnrollmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<Course>(e =>
        {
            e.HasIndex(x => new { x.OrganizationId, x.StartDate });
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Location).HasMaxLength(200);
            e.Property(x => x.Instructor).HasMaxLength(200);
            e.Property(x => x.ExamInfo).HasMaxLength(1000);
            e.Property(x => x.Description).HasMaxLength(4000);
            e.Property(x => x.Price).HasPrecision(12, 2);
            e.Property(x => x.Deposit).HasPrecision(12, 2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<CourseSession>(e =>
        {
            e.HasIndex(x => new { x.OrganizationId, x.CourseId, x.Date });
            e.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Topic).HasMaxLength(200);
            e.Property(x => x.Notes).HasMaxLength(4000);
            e.HasOne(x => x.Course).WithMany(x => x.Sessions).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<CourseAttendance>(e =>
        {
            e.HasKey(x => new { x.SessionId, x.EnrollmentId });
            e.HasIndex(x => x.OrganizationId);
            e.HasOne(x => x.Session).WithMany().HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Enrollment).WithMany().HasForeignKey(x => x.EnrollmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<CourseMaterial>(e =>
        {
            e.HasIndex(x => new { x.OrganizationId, x.CourseId });
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Url).HasMaxLength(1000);
            e.Property(x => x.Note).HasMaxLength(1000);
            e.HasOne(x => x.Course).WithMany(x => x.Materials).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<CourseTest>(e =>
        {
            e.HasIndex(x => new { x.OrganizationId, x.CourseId });
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.MaxScore).HasPrecision(7, 2);
            e.Property(x => x.PassScore).HasPrecision(7, 2);
            e.HasOne(x => x.Course).WithMany(x => x.Tests).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<CourseTestResult>(e =>
        {
            e.HasKey(x => new { x.TestId, x.EnrollmentId });
            e.HasIndex(x => x.OrganizationId);
            e.Property(x => x.Score).HasPrecision(7, 2);
            e.HasOne(x => x.Test).WithMany(x => x.Results).HasForeignKey(x => x.TestId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Enrollment).WithMany().HasForeignKey(x => x.EnrollmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<ContactActivity>(e =>
        {
            e.HasIndex(x => new { x.OrganizationId, x.ParticipantId, x.OccurredAt });
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Text).HasMaxLength(2000);
            e.HasOne(x => x.Participant).WithMany(x => x.Activities).HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<ContactTask>(e =>
        {
            e.HasIndex(x => new { x.OrganizationId, x.DoneAt, x.DueDate });
            e.Property(x => x.Title).HasMaxLength(200);
            e.HasOne(x => x.Participant).WithMany(x => x.Tasks).HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });

        b.Entity<CruiseCost>(e =>
        {
            e.HasIndex(x => new { x.OrganizationId, x.CruiseId });
            e.Property(x => x.Category).HasMaxLength(60);
            e.Property(x => x.Description).HasMaxLength(300);
            e.Property(x => x.Amount).HasPrecision(12, 2);
            e.HasOne(x => x.Cruise).WithMany(x => x.Costs).HasForeignKey(x => x.CruiseId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Course).WithMany(x => x.Costs).HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.OrganizationId, x.CourseId });
            e.ToTable(t => t.HasCheckConstraint("CK_CruiseCosts_CruiseOrCourse", "(\"CruiseId\" IS NULL) <> (\"CourseId\" IS NULL)"));
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.OrganizationId == CurrentOrganizationId);
        });
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.OrganizationId = tenant.OrganizationId
                    ?? throw new InvalidOperationException("Zapis danych organizacji bez wybranej organizacji.");
            }
            else if (entry.State is EntityState.Modified or EntityState.Deleted && entry.Entity.OrganizationId != tenant.OrganizationId)
            {
                throw new InvalidOperationException("Próba zmiany danych innej organizacji.");
            }
        }
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
