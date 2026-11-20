namespace Nautivo.Api.Domain;

/// <summary>Dane należące do organizacji. DbContext filtruje je po bieżącej organizacji.</summary>
public interface ITenantEntity
{
    Guid OrganizationId { get; set; }
}

public static class Modules
{
    public const string Cruises = "rejsy";
    public const string Fleet = "flota";
    public const string Calendar = "kalendarz";
    public const string Participants = "uczestnicy";
    public const string Courses = "szkolenia";
    public const string Charters = "czartery";

    public static readonly string[] All = [Cruises, Fleet, Calendar, Participants, Courses, Charters];
}

public class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string[] EnabledModules { get; set; } = Modules.All;
    public string? ContactEmail { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public string PasswordHash { get; set; } = "";
    public bool IsPlatformAdmin { get; set; }
    public int FailedLogins { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<Membership> Memberships { get; set; } = [];
}

public enum OrganizationRole { Owner, Staff }

public class Membership
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public OrganizationRole Role { get; set; }
}

public enum VesselStatus { InService, OutOfService }

/// <summary>Rodzaj jednostki. Służy do opisu i filtrowania, nie zmienia zasad rezerwacji.</summary>
public enum VesselKind { Yacht, Sailboat, Motorboat, Jetski, Kayak, Other }

public class Vessel : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public required string Name { get; set; }
    public string Model { get; set; } = "";
    public decimal LengthMeters { get; set; }
    public int Berths { get; set; }
    public string? RegistrationNumber { get; set; }
    public DateOnly? InspectionDue { get; set; }
    public VesselStatus Status { get; set; }
    public string? Notes { get; set; }
    public VesselKind Kind { get; set; }
    /// <summary>Cena czarteru za dzień. Pusta oznacza, że jednostka nie jest wynajmowana.</summary>
    public decimal? CharterDayRate { get; set; }
    /// <summary>Wyposażenie na pokładzie: co jest, a co trzeba uzupełnić.</summary>
    public List<ChecklistItem> Checklist { get; set; } = [];
    public List<VesselBlock> Blocks { get; set; } = [];
}

public class ChecklistItem
{
    public string Name { get; set; } = "";
    /// <summary>True: jest na pokładzie i sprawne. False: do uzupełnienia lub naprawy.</summary>
    public bool IsOk { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// Wynajem jednostki klientowi na okres od–do. Total to uzgodniona kwota: domyślnie cena za dzień razy liczba dni,
/// może być niższa, np. przy rabacie. DayRate zapisuje cenę z chwili rezerwacji.
/// </summary>
public class Charter : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid VesselId { get; set; }
    public Vessel? Vessel { get; set; }
    public Guid ParticipantId { get; set; }
    public Participant? Participant { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal DayRate { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>Okres, w którym jednostka jest niedostępna, np. serwis.</summary>
public class VesselBlock : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid VesselId { get; set; }
    public Vessel? Vessel { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public required string Reason { get; set; }
}

public enum CruiseStatus { Planned, Cancelled }

public class Cruise : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public required string Name { get; set; }
    /// <summary>Pierwszy punkt trasy. Kopia w osobnej kolumnie do list i dashboardu.</summary>
    public string DeparturePort { get; set; } = "";
    /// <summary>Ostatni punkt trasy.</summary>
    public string ArrivalPort { get; set; } = "";
    /// <summary>Cała trasa w kolejności: port startowy, porty pośrednie, port docelowy.</summary>
    public List<RoutePoint> RoutePoints { get; set; } = [];
    public bool IsConfirmed { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    /// <summary>Jacht z modułu floty. Opcjonalny; nazwa, model i długość mogą być wpisane ręcznie.</summary>
    public Guid? VesselId { get; set; }
    public Vessel? Vessel { get; set; }
    public string? YachtName { get; set; }
    public string? YachtModel { get; set; }
    public decimal? YachtLengthMeters { get; set; }
    public decimal Price { get; set; }
    public decimal Deposit { get; set; }
    public DateOnly BalanceDueDate { get; set; }
    public int Capacity { get; set; }
    public CruiseStatus Status { get; set; }
    public string? Description { get; set; }
    public List<Enrollment> Enrollments { get; set; } = [];
    public List<CruiseCost> Costs { get; set; } = [];
}

/// <summary>Punkt trasy. Współrzędne są puste, dopóki port nie zostanie znaleziony lub wskazany na mapie.</summary>
public class RoutePoint
{
    public string Name { get; set; } = "";
    public double? Lat { get; set; }
    public double? Lng { get; set; }
}

/// <summary>Koszt rejsu albo szkolenia (dokładnie jedno z nich), np. czarter, paliwo, wynajem sali, opłaty egzaminacyjne.</summary>
public class CruiseCost : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid? CruiseId { get; set; }
    public Cruise? Cruise { get; set; }
    public Guid? CourseId { get; set; }
    public Course? Course { get; set; }
    public required string Category { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Participant : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public required string FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Experience { get; set; }
    public string? EmergencyContact { get; set; }
    public string? Notes { get; set; }
    /// <summary>Skąd klient o nas wie, np. polecenie, Instagram. Do oceny, które kanały działają.</summary>
    public string? Source { get; set; }
    public string[] Tags { get; set; } = [];
    /// <summary>
    /// Uprawnienia zdobyte poza naszymi szkoleniami (nazwy z CourseType, np. "Zj").
    /// Uprawnienia ze zdanych egzaminów u nas wynikają z zapisów i nie są tu kopiowane.
    /// </summary>
    public string[] ManualQualifications { get; set; } = [];
    /// <summary>Zgoda na informacje marketingowe. Data zapisuje, kiedy zgodę odnotowano.</summary>
    public bool MarketingConsent { get; set; }
    public DateTimeOffset? MarketingConsentAt { get; set; }
    /// <summary>Ustawione po anonimizacji na żądanie osoby. Zostają tylko dane rozliczeń.</summary>
    public DateTimeOffset? AnonymizedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<Enrollment> Enrollments { get; set; } = [];
    public List<ContactActivity> Activities { get; set; } = [];
    public List<ContactTask> Tasks { get; set; } = [];
}

public enum ActivityType { Note, Call, Email, Meeting }

/// <summary>Wpis w historii kontaktu z klientem: notatka, rozmowa, e-mail, spotkanie.</summary>
public class ContactActivity : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid ParticipantId { get; set; }
    public Participant? Participant { get; set; }
    public ActivityType Type { get; set; }
    public required string Text { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    /// <summary>Wpis dodany przez system (np. zmiana zgody). Nie da się go usunąć, bo dokumentuje zgody.</summary>
    public bool IsSystem { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>Zadanie dotyczące klienta, np. „oddzwonić w sprawie Chorwacji”, z terminem.</summary>
public class ContactTask : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid ParticipantId { get; set; }
    public Participant? Participant { get; set; }
    public required string Title { get; set; }
    public DateOnly DueDate { get; set; }
    public DateTimeOffset? DoneAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public enum EnrollmentStatus { Confirmed, Waitlist, Cancelled }

public enum ExamResult { Pending, Passed, Failed, Absent }

/// <summary>
/// Zapis osoby na rejs albo na szkolenie (dokładnie jedno z nich). Total to kwota do zapłaty z chwili zapisu.
/// Pola egzaminu, patentu i dokumentów dotyczą tylko szkoleń.
/// </summary>
public class Enrollment : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid? CruiseId { get; set; }
    public Cruise? Cruise { get; set; }
    public Guid? CourseId { get; set; }
    public Course? Course { get; set; }
    public ExamResult ExamResult { get; set; }
    public string? CertificateNumber { get; set; }
    public string[] DocumentsDelivered { get; set; } = [];
    public Guid ParticipantId { get; set; }
    public Participant? Participant { get; set; }
    public EnrollmentStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<Payment> Payments { get; set; } = [];
}

public class Payment : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }
    public decimal Amount { get; set; }
    public DateOnly PaidOn { get; set; }
    public string Method { get; set; } = "Przelew";
    public string? Note { get; set; }
    public Guid? RecordedByUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>Rodzaj szkolenia. Wymagania egzaminacyjne ustala organizator; aplikacja ich nie narzuca.</summary>
public enum CourseType { Zj, Jsm, Stcw, Src, Smw, Other }

public class Course : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public CourseType Type { get; set; }
    public required string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Location { get; set; } = "";
    public string? Instructor { get; set; }
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public decimal Deposit { get; set; }
    public DateOnly BalanceDueDate { get; set; }
    public DateOnly? ExamDate { get; set; }
    /// <summary>Informacje o egzaminie, np. kto egzaminuje i co zabrać.</summary>
    public string? ExamInfo { get; set; }
    /// <summary>Dokumenty, które kursant musi dostarczyć, np. „zdjęcie”, „kopia patentu ŻJ”. Bez treści dokumentów.</summary>
    public string[] RequiredDocuments { get; set; } = [];
    public CruiseStatus Status { get; set; }
    public string? Description { get; set; }
    /// <summary>Kiedy prowadzący zakończył kurs. Zakończony kurs nie przyjmuje nowych zapisów.</summary>
    public DateTimeOffset? CompletedAt { get; set; }
    public List<CourseSession> Sessions { get; set; } = [];
    public List<Enrollment> Enrollments { get; set; } = [];
    public List<CruiseCost> Costs { get; set; } = [];
    public List<CourseMaterial> Materials { get; set; } = [];
    public List<CourseTest> Tests { get; set; } = [];
}

public enum SessionKind { Theory, Practice, Exam }

/// <summary>Zajęcia w harmonogramie szkolenia.</summary>
public class CourseSession : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid CourseId { get; set; }
    public Course? Course { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public SessionKind Kind { get; set; }
    public required string Topic { get; set; }
    /// <summary>Plan lekcji dla prowadzącego: zagadnienia, ćwiczenia, uwagi.</summary>
    public string? Notes { get; set; }
    /// <summary>Kiedy prowadzący oznaczył zajęcia jako ukończone. Na tej podstawie liczymy postęp kursu.</summary>
    public DateTimeOffset? CompletedAt { get; set; }
}

/// <summary>Materiał szkoleniowy: link (np. do prezentacji na dysku) albo opis. Plików nie przechowujemy.</summary>
public class CourseMaterial : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid CourseId { get; set; }
    public Course? Course { get; set; }
    public required string Title { get; set; }
    public string? Url { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>Test sprawdzający w trakcie szkolenia, np. próbny egzamin z locji.</summary>
public class CourseTest : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid CourseId { get; set; }
    public Course? Course { get; set; }
    public required string Title { get; set; }
    public DateOnly Date { get; set; }
    public decimal MaxScore { get; set; }
    public decimal PassScore { get; set; }
    public List<CourseTestResult> Results { get; set; } = [];
}

public class CourseTestResult : ITenantEntity
{
    public Guid OrganizationId { get; set; }
    public Guid TestId { get; set; }
    public CourseTest? Test { get; set; }
    public Guid EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }
    public decimal Score { get; set; }
}

/// <summary>Obecność kursanta na zajęciach. Brak wiersza oznacza nieobecność.</summary>
public class CourseAttendance : ITenantEntity
{
    public Guid OrganizationId { get; set; }
    public Guid SessionId { get; set; }
    public CourseSession? Session { get; set; }
    public Guid EnrollmentId { get; set; }
    public Enrollment? Enrollment { get; set; }
}
