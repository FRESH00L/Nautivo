using Microsoft.EntityFrameworkCore;
using Nautivo.Api.Domain;
using Nautivo.Api.Features;
using Nautivo.Api.Infrastructure;

namespace Nautivo.Api.Data;

/// <summary>
/// Fikcyjne organizacje i osoby do pokazów, z rocznym ruchem na wykresie trendu.
/// Działa tylko gdy Seed:Demo=true i nie ma jeszcze organizacji demo. Konta użytkowników zostawia w spokoju.
/// </summary>
public static class DemoSeeder
{
    private const string DemoSuffix = "(demo)";

    private static readonly string[] FirstNames = ["Anna", "Marek", "Kasia", "Tomasz", "Ewa", "Piotr", "Magda", "Paweł", "Ola", "Jan", "Zofia", "Michał", "Iga", "Adam", "Natalia", "Kuba", "Hanna", "Igor", "Lena", "Filip"];
    private static readonly string[] LastNames = ["Nowak", "Lis", "Wrona", "Sowa", "Zięba", "Kania", "Mazur", "Bąk", "Kos", "Czapla", "Dudek", "Szpak", "Rybka", "Wilk", "Sikora", "Żuraw", "Gil", "Bór"];
    private static readonly string[] Experience = ["Bez doświadczenia", "Kilka rejsów jako załoga", "Żeglarz jachtowy", "Jachtowy sternik morski"];

    /// <summary>Przybliżone współrzędne portów z danych demo (do mapy trasy).</summary>
    private static readonly Dictionary<string, (double Lat, double Lng)> Ports = new()
    {
        ["Gdynia"] = (54.519, 18.553), ["Hel"] = (54.606, 18.801), ["Jastarnia"] = (54.700, 18.676), ["Bornholm"] = (55.103, 14.700),
        ["Giżycko"] = (54.038, 21.766), ["Mikołajki"] = (53.802, 21.573), ["Ruciane"] = (53.640, 21.548), ["Sztynort"] = (54.130, 21.682), ["Wilkasy"] = (54.010, 21.737),
        ["Split"] = (43.503, 16.440), ["Brač"] = (43.380, 16.550), ["Hvar"] = (43.172, 16.441), ["Vis"] = (43.062, 16.183), ["Biograd"] = (43.940, 15.443), ["Kornati"] = (43.800, 15.290),
        ["Olbia"] = (40.923, 9.508), ["La Maddalena"] = (41.214, 9.408),
        ["Lefkada"] = (38.833, 20.708), ["Itaka"] = (38.367, 20.718), ["Kefalonia"] = (38.177, 20.489),
        ["Lavrio"] = (37.714, 24.056), ["Kythnos"] = (37.393, 24.425), ["Serifos"] = (37.143, 24.512),
        ["Arrecife"] = (28.963, -13.547), ["La Graciosa"] = (29.231, -13.503),
        ["Santa Cruz"] = (28.466, -16.249), ["San Sebastián"] = (28.091, -17.110),
        ["Horta"] = (38.532, -28.626), ["Lajes do Pico"] = (38.396, -28.255),
    };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var tenant = services.GetRequiredService<TenantContext>();
        if (await db.Organizations.AnyAsync(o => o.Name.EndsWith(DemoSuffix)))
        {
            await FillDemoCoordinatesAsync(db, tenant);
            await FillDemoCrmAsync(db, tenant, services.GetRequiredService<TimeProvider>().Today());
            await FillDemoCoursesAsync(db, tenant, services.GetRequiredService<TimeProvider>().Today());
            await FillDemoChartersAsync(db, tenant, services.GetRequiredService<TimeProvider>().Today());
            return;
        }

        var today = services.GetRequiredService<TimeProvider>().Today();
        var random = new Random(2027);

        var main = new Organization { Name = $"Rejsy Północny Hals {DemoSuffix}", City = "Gdynia", ContactEmail = "biuro@example.com", Phone = "+48 500 100 200", Website = "https://example.com" };
        var club = new Organization { Name = $"Klub Żeglarski Fordewind {DemoSuffix}", City = "Wilkasy", ContactEmail = "klub@example.com" };
        db.Organizations.AddRange(main, club);
        await db.SaveChangesAsync();

        tenant.OrganizationId = main.Id;
        var zefir = new Vessel { Name = "Zefir", Model = "Delphia 40", LengthMeters = 12m, Berths = 8, RegistrationNumber = "PZZ-1042", InspectionDue = today.AddDays(18) };
        var dal = new Vessel { Name = "Morska Dal", Model = "Bavaria 46", LengthMeters = 14.3m, Berths = 10, RegistrationNumber = "PZZ-2210", InspectionDue = today.AddDays(210) };
        var bryza = new Vessel { Name = "Bryza", Model = "Maxus 28", LengthMeters = 8.5m, Berths = 6, RegistrationNumber = "WAR-3381", InspectionDue = today.AddDays(95), Status = VesselStatus.OutOfService, Notes = "Wymiana olinowania" };
        db.Vessels.AddRange(zefir, dal, bryza);
        db.VesselBlocks.Add(new VesselBlock { Vessel = bryza, StartDate = today.AddDays(-5), EndDate = today.AddDays(16), Reason = "Serwis: olinowanie" });

        var people = Enumerable.Range(0, 60).Select(i =>
        {
            var name = $"{FirstNames[i % FirstNames.Length]} {LastNames[(i * 7) % LastNames.Length]}";
            return new Participant
            {
                FullName = name,
                Email = $"uczestnik{i + 1}@example.com",
                Phone = $"+48 500 100 {100 + i}",
                Experience = Experience[i % Experience.Length],
                CreatedAt = Utc(today.AddDays(-random.Next(5, 400))),
            };
        }).ToList();
        db.Participants.AddRange(people);

        // Zakończone rejsy z ostatniego roku, po kolei na dwóch jachtach.
        (string Name, string Route, int Price)[] past =
        [
            ("Bałtyk: Gdynia, Hel", "Gdynia, Hel, Jastarnia, Gdynia", 1950),
            ("Mazury: Giżycko, Mikołajki", "Giżycko, Mikołajki, Ruciane", 1490),
            ("Chorwacja: Dalmacja", "Split, Brač, Hvar, Split", 2750),
            ("Sardynia: Costa Smeralda", "Olbia, La Maddalena, Olbia", 3190),
            ("Grecja: Wyspy Jońskie", "Lefkada, Itaka, Kefalonia, Lefkada", 2890),
            ("Bałtyk: Bornholm", "Gdynia, Bornholm, Gdynia", 2350),
            ("Chorwacja: Kornati", "Biograd, Kornati, Biograd", 2650),
            ("Wyspy Kanaryjskie: Lanzarote", "Arrecife, La Graciosa, Arrecife", 3290),
        ];
        for (var i = 0; i < past.Length; i++)
        {
            var start = today.AddDays(-340 + i * 42);
            var cruise = AddCruise(db, past[i].Name, past[i].Route, start, start.AddDays(7), i % 2 == 0 ? zefir : dal, past[i].Price, 8, confirmed: true);
            Enroll(db, random, cruise, people, new string('P', 5 + random.Next(4)), today);
        }

        var canary = AddCruise(db, "Teneryfa i La Gomera", "Santa Cruz, San Sebastián, Santa Cruz", today.AddDays(9), today.AddDays(16), zefir, 3450, 8, confirmed: true);
        var azores = AddCruise(db, "Azory: Faial i Pico", "Horta, Lajes do Pico, Horta", today.AddDays(40), today.AddDays(47), dal, 4190, 8, confirmed: true);
        var mazury = AddCruise(db, "Mazury: Weekend pod żaglami", "Giżycko, Sztynort, Giżycko", today.AddDays(75), today.AddDays(78), zefir, 890, 6, confirmed: false);
        var croatia = AddCruise(db, "Chorwacja: Split, Hvar, Vis", "Split, Hvar, Vis, Split", today.AddDays(120), today.AddDays(127), zefir, 2750, 8, confirmed: true);
        var greece = AddCruise(db, "Grecja: Cyklady", "Lavrio, Kythnos, Serifos, Lavrio", today.AddDays(150), today.AddDays(157), dal, 3190, 8, confirmed: false);

        Enroll(db, random, canary, people, "PPZPZNPP", today);
        Enroll(db, random, azores, people, "PZZZNZ", today);
        Enroll(db, random, mazury, people, "ZN", today);
        Enroll(db, random, croatia, people, "ZZZ", today);
        Enroll(db, random, greece, people, "Z", today);
        AddCosts(db, canary, ("Czarter jachtu", 14500m), ("Opłaty portowe", 1200m), ("Paliwo", 600m), ("Skipper", 4000m));
        AddCosts(db, azores, ("Czarter jachtu", 16800m), ("Opłaty portowe", 1500m), ("Skipper", 4000m));
        await db.SaveChangesAsync();

        tenant.OrganizationId = club.Id;
        var omega = new Vessel { Name = "Omega 7", Model = "Omega", LengthMeters = 6.2m, Berths = 4, InspectionDue = today.AddDays(150) };
        db.Vessels.Add(omega);
        var members = new[] { "Hanna Gil", "Igor Bór" }.Select((name, i) => new Participant { FullName = name, Email = $"klubowicz{i + 1}@example.com", CreatedAt = Utc(today.AddDays(-20)) }).ToList();
        db.Participants.AddRange(members);
        var clubCruise = AddCruise(db, "Rejs klubowy: Wilkasy, Ruciane", "Wilkasy, Mikołajki, Ruciane", today.AddDays(30), today.AddDays(33), omega, 480, 4, confirmed: false);
        Enroll(db, random, clubCruise, members, "ZN", today);
        await db.SaveChangesAsync();

        tenant.OrganizationId = null;
        await FillDemoCrmAsync(db, tenant, today);
        await FillDemoCoursesAsync(db, tenant, today);
        await FillDemoChartersAsync(db, tenant, today);
    }

    /// <summary>
    /// Czartery demo w pierwszej organizacji demo: skuter, motorówka i łódź z cennikiem i wyposażeniem
    /// oraz wynajmy z ostatnich miesięcy i nadchodzące. Działa raz.
    /// </summary>
    private static async Task FillDemoChartersAsync(AppDbContext db, TenantContext tenant, DateOnly today)
    {
        var main = await db.Organizations.Where(o => o.Name.EndsWith(DemoSuffix)).OrderBy(o => o.CreatedAt).Select(o => o.Id).FirstOrDefaultAsync();
        if (main == Guid.Empty)
        {
            return;
        }
        tenant.OrganizationId = main;
        if (await db.Charters.AnyAsync() || await db.Vessels.AnyAsync(v => v.CharterDayRate != null))
        {
            tenant.OrganizationId = null;
            return;
        }
        var people = await db.Participants.Where(p => p.AnonymizedAt == null).OrderBy(p => p.CreatedAt).Take(12).ToListAsync();
        if (people.Count == 0)
        {
            tenant.OrganizationId = null;
            return;
        }

        static List<ChecklistItem> Items(params (string Name, bool Ok, string? Note)[] items) =>
            items.Select(i => new ChecklistItem { Name = i.Name, IsOk = i.Ok, Note = i.Note }).ToList();

        var jetski = new Vessel
        {
            Name = "Skuter Orka", Model = "Sea-Doo Spark", LengthMeters = 2.8m, Berths = 2, Kind = VesselKind.Jetski, CharterDayRate = 450m, InspectionDue = today.AddDays(120),
            Checklist = Items(("Kamizelki ratunkowe x2", true, null), ("Linka zabezpieczająca (kill switch)", true, null), ("Paliwo do pełna", false, "Dolać przed wydaniem")),
        };
        var motorboat = new Vessel
        {
            Name = "Motorówka Delfin", Model = "Quicksilver 605", LengthMeters = 6.1m, Berths = 6, Kind = VesselKind.Motorboat, CharterDayRate = 900m, InspectionDue = today.AddDays(60),
            Checklist = Items(("Kamizelki ratunkowe x6", true, null), ("Gaśnica", false, "Po terminie legalizacji"), ("Kotwica z liną", true, null), ("Apteczka", true, null), ("Odbijacze x4", false, "Brakuje jednego")),
        };
        var sailboat = new Vessel
        {
            Name = "Łódka Mewa", Model = "Omega", LengthMeters = 6.2m, Berths = 4, Kind = VesselKind.Sailboat, CharterDayRate = 300m,
            Checklist = Items(("Kamizelki ratunkowe x4", true, null), ("Wiosła", true, null), ("Pagaj", true, null), ("Czerpak", true, null)),
        };
        db.Vessels.AddRange(jetski, motorboat, sailboat);

        var random = new Random(2029);
        var slots = new[]
        {
            (jetski, -150, 1), (jetski, -120, 2), (jetski, -90, 1), (jetski, -60, 3), (jetski, -30, 1), (jetski, -8, 2), (jetski, 4, 1), (jetski, 12, 2),
            (motorboat, -140, 2), (motorboat, -100, 3), (motorboat, -45, 2), (motorboat, -14, 4), (motorboat, 7, 3), (motorboat, 20, 7),
            (sailboat, -110, 7), (sailboat, -70, 7), (sailboat, -20, 5), (sailboat, 2, 7),
        };
        foreach (var (vessel, offset, days) in slots)
        {
            var start = today.AddDays(offset);
            var listPrice = vessel.CharterDayRate!.Value * days;
            // Co czwarty wynajem z rabatem 10%.
            var total = random.Next(4) == 0 ? Math.Round(listPrice * 0.9m) : listPrice;
            db.Charters.Add(new Charter
            {
                Vessel = vessel, Participant = people[random.Next(people.Count)], StartDate = start, EndDate = start.AddDays(days),
                DayRate = vessel.CharterDayRate.Value, Total = total, CreatedAt = Utc(start.AddDays(-14)),
            });
        }
        await db.SaveChangesAsync();
        tenant.OrganizationId = null;
    }

    /// <summary>
    /// Szkolenia demo w pierwszej organizacji demo: zakończony kurs ŻJ (zdane egzaminy dają uprawnienia w CRM)
    /// oraz nadchodzące JSM i SRC z harmonogramem, testem, materiałami i kosztami. Działa raz.
    /// </summary>
    private static async Task FillDemoCoursesAsync(AppDbContext db, TenantContext tenant, DateOnly today)
    {
        var main = await db.Organizations.Where(o => o.Name.EndsWith(DemoSuffix)).OrderBy(o => o.CreatedAt).Select(o => o.Id).FirstOrDefaultAsync();
        if (main == Guid.Empty)
        {
            return;
        }
        tenant.OrganizationId = main;
        if (await db.Courses.AnyAsync())
        {
            tenant.OrganizationId = null;
            return;
        }
        var people = await db.Participants.Where(p => p.AnonymizedAt == null).OrderBy(p => p.CreatedAt).Take(24).ToListAsync();
        var random = new Random(2028);

        Course AddCourse(CourseType type, string name, int startOffset, int days, int capacity, decimal price, string location, string instructor, string[] documents)
        {
            var start = today.AddDays(startOffset);
            var course = new Course
            {
                Type = type, Name = name, StartDate = start, EndDate = start.AddDays(days), Capacity = capacity, Price = price,
                Deposit = Math.Round(price * 0.3m / 10m) * 10m, BalanceDueDate = start.AddDays(-7), Location = location, Instructor = instructor,
                ExamDate = start.AddDays(days), ExamInfo = "Egzamin przed komisją. Zabrać dokument tożsamości.", RequiredDocuments = documents,
            };
            db.Courses.Add(course);
            return course;
        }

        var zj = AddCourse(CourseType.Zj, "Żeglarz jachtowy: kurs weekendowy", -120, 9, 10, 1690, "Giżycko, przystań", "Marta Czapla", ["Zdjęcie", "Zgoda rodzica (niepełnoletni)"]);
        zj.CompletedAt = Utc(zj.EndDate);
        var jsm = AddCourse(CourseType.Jsm, "Jachtowy sternik morski", 25, 7, 8, 2890, "Gdynia, marina", "Leon Bąk", ["Zdjęcie", "Kopia patentu ŻJ", "Potwierdzenie stażu"]);
        var src = AddCourse(CourseType.Src, "Radio VHF (SRC)", 12, 1, 12, 690, "Gdynia, sala szkoleniowa", "Leon Bąk", ["Zdjęcie"]);

        void Schedule(Course course, params (int Day, int From, int To, SessionKind Kind, string Topic)[] sessions)
        {
            foreach (var x in sessions)
            {
                var date = course.StartDate.AddDays(x.Day);
                db.CourseSessions.Add(new CourseSession { Course = course, Date = date, StartTime = new TimeOnly(x.From, 0), EndTime = new TimeOnly(x.To, 0), Kind = x.Kind, Topic = x.Topic, CompletedAt = date < today ? Utc(date) : null });
            }
        }
        Schedule(zj, (0, 9, 13, SessionKind.Theory, "Budowa jachtu i locja"), (1, 9, 16, SessionKind.Practice, "Manewry portowe"), (7, 9, 16, SessionKind.Practice, "Człowiek za burtą"), (9, 9, 14, SessionKind.Exam, "Egzamin"));
        Schedule(jsm, (0, 9, 13, SessionKind.Theory, "Nawigacja morska"), (1, 9, 13, SessionKind.Theory, "Meteorologia"), (2, 8, 18, SessionKind.Practice, "Rejs szkoleniowy: manewry"), (5, 8, 18, SessionKind.Practice, "Nawigacja w nocy"), (7, 9, 14, SessionKind.Exam, "Egzamin"));
        Schedule(src, (0, 9, 15, SessionKind.Theory, "Procedury GMDSS i łączność VHF"), (1, 9, 12, SessionKind.Exam, "Egzamin SRC"));

        var zjTest = new CourseTest { Course = zj, Title = "Test próbny z teorii", Date = zj.StartDate.AddDays(6), MaxScore = 75, PassScore = 50 };
        db.CourseTests.Add(zjTest);
        db.CourseMaterials.Add(new CourseMaterial { Course = jsm, Title = "Konspekt: nawigacja morska", Note = "Wersja do druku dla kursantów." });
        db.CourseMaterials.Add(new CourseMaterial { Course = jsm, Title = "Lista sprzętu na rejs szkoleniowy", Url = "https://example.com/sprzet" });
        foreach (var (course, category, amount) in new[] { (zj, "Wynajem łodzi", 4200m), (zj, "Instruktor", 3000m), (jsm, "Czarter jachtu", 9800m), (jsm, "Instruktor", 4500m), (src, "Wynajem sali", 400m) })
        {
            db.CruiseCosts.Add(new CruiseCost { Course = course, Category = category, Amount = amount });
        }

        void Enroll(Course course, IEnumerable<Participant> group, bool finished)
        {
            foreach (var person in group)
            {
                var enrollment = new Enrollment { Course = course, Participant = person, Total = course.Price, Status = EnrollmentStatus.Confirmed, CreatedAt = Utc(course.StartDate.AddDays(-random.Next(15, 60))) };
                db.Enrollments.Add(enrollment);
                var paidOn = course.StartDate.AddDays(-random.Next(8, 14));
                if (paidOn > today) paidOn = today;
                enrollment.Payments.Add(new Payment { Enrollment = enrollment, Amount = course.Deposit, PaidOn = paidOn, Method = "Przelew", Note = "Zaliczka" });
                if (finished || random.Next(2) == 0)
                {
                    enrollment.Payments.Add(new Payment { Enrollment = enrollment, Amount = course.Price - course.Deposit, PaidOn = paidOn, Method = "Przelew", Note = "Saldo" });
                }
                if (finished)
                {
                    enrollment.DocumentsDelivered = course.RequiredDocuments.Take(1).ToArray();
                    enrollment.ExamResult = random.Next(6) == 0 ? ExamResult.Failed : ExamResult.Passed;
                    enrollment.CertificateNumber = enrollment.ExamResult == ExamResult.Passed ? $"ŻJ/{today.Year}/{random.Next(1000, 9999)}" : null;
                    db.CourseTestResults.Add(new CourseTestResult { Test = zjTest, Enrollment = enrollment, Score = random.Next(40, 76) });
                }
            }
        }
        Enroll(zj, people.Take(8), finished: true);
        Enroll(jsm, people.Skip(8).Take(5), finished: false);
        Enroll(src, people.Skip(13).Take(6), finished: false);
        await db.SaveChangesAsync();

        // Obecność na zajęciach zakończonego kursu (większość obecna).
        var zjSessions = await db.CourseSessions.Where(x => x.CourseId == zj.Id).ToListAsync();
        var zjEnrollments = await db.Enrollments.Where(e => e.CourseId == zj.Id).ToListAsync();
        foreach (var session in zjSessions)
        {
            foreach (var enrollment in zjEnrollments.Where(_ => random.Next(10) > 0))
            {
                db.CourseAttendance.Add(new CourseAttendance { SessionId = session.Id, EnrollmentId = enrollment.Id });
            }
        }
        await db.SaveChangesAsync();
        tenant.OrganizationId = null;
    }

    private static readonly string[] Sources = ["Polecenie", "Instagram", "Facebook", "Strona www", "Targi żeglarskie", "Wraca do nas"];
    private static readonly string[][] TagSets = [["rodzina"], ["firma"], ["początkujący"], ["kapitan", "doświadczony"], [], ["grupa znajomych"], [], ["VIP"]];

    /// <summary>
    /// Dane CRM dla organizacji demo: źródło, tagi, zgody, kilka notatek i zadań.
    /// Działa raz: pomija organizację, w której ktoś ma już wpisane źródło.
    /// </summary>
    private static async Task FillDemoCrmAsync(AppDbContext db, TenantContext tenant, DateOnly today)
    {
        var demoOrganizations = await db.Organizations.Where(o => o.Name.EndsWith(DemoSuffix)).Select(o => o.Id).ToListAsync();
        foreach (var organizationId in demoOrganizations)
        {
            tenant.OrganizationId = organizationId;
            if (await db.Participants.AnyAsync(p => p.Source != null))
            {
                continue;
            }
            var people = await db.Participants.OrderBy(p => p.CreatedAt).ToListAsync();
            for (var i = 0; i < people.Count; i++)
            {
                var person = people[i];
                person.Source = Sources[i % Sources.Length];
                person.Tags = TagSets[i % TagSets.Length];
                person.MarketingConsent = i % 3 != 0;
                person.MarketingConsentAt = person.MarketingConsent ? person.CreatedAt : null;
            }
            var notes = new (ActivityType Type, string Text)[]
            {
                (ActivityType.Call, "Pytała o rejs rodzinny w sierpniu, dwoje dzieci 10 i 12 lat."),
                (ActivityType.Email, "Wysłany program rejsu i warunki zaliczki."),
                (ActivityType.Meeting, "Spotkanie na targach, chce popłynąć jako drugi oficer."),
                (ActivityType.Note, "Preferuje kabinę dziobową, prosi o kontakt SMS."),
            };
            var tasks = new (string Title, int Days)[]
            {
                ("Oddzwonić w sprawie rejsu rodzinnego", -2),
                ("Przypomnieć o dopłacie", 0),
                ("Wysłać propozycję rejsu na Bałtyk", 3),
                ("Zapytać o opinię po rejsie", 7),
            };
            for (var i = 0; i < Math.Min(people.Count, notes.Length); i++)
            {
                var person = people[i * 3 % people.Count];
                db.ContactActivities.Add(new ContactActivity { Participant = person, Type = notes[i].Type, Text = notes[i].Text, OccurredAt = Utc(today.AddDays(-3 - i * 5)) });
                db.ContactTasks.Add(new ContactTask { Participant = person, Title = tasks[i].Title, DueDate = today.AddDays(tasks[i].Days) });
            }
            await db.SaveChangesAsync();
        }
        tenant.OrganizationId = null;
    }

    /// <summary>
    /// Rejsy demo sprzed dodania mapy mają trasę bez współrzędnych. Uzupełnia je dla znanych portów demo,
    /// tylko w organizacjach demo i tylko tam, gdzie współrzędnych brak.
    /// </summary>
    private static async Task FillDemoCoordinatesAsync(AppDbContext db, TenantContext tenant)
    {
        var demoOrganizations = await db.Organizations.Where(o => o.Name.EndsWith(DemoSuffix)).Select(o => o.Id).ToListAsync();
        foreach (var organizationId in demoOrganizations)
        {
            tenant.OrganizationId = organizationId;
            foreach (var cruise in await db.Cruises.ToListAsync())
            {
                if (!cruise.RoutePoints.Any(p => p.Lat is null && Ports.ContainsKey(p.Name)))
                {
                    continue;
                }
                cruise.RoutePoints = cruise.RoutePoints.Select(p => p.Lat is null && Ports.TryGetValue(p.Name, out var at)
                    ? new RoutePoint { Name = p.Name, Lat = at.Lat, Lng = at.Lng }
                    : new RoutePoint { Name = p.Name, Lat = p.Lat, Lng = p.Lng }).ToList();
            }
            await db.SaveChangesAsync();
        }
        tenant.OrganizationId = null;
    }

    private static DateTimeOffset Utc(DateOnly date) => new(date.ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero);

    private static Cruise AddCruise(AppDbContext db, string name, string route, DateOnly start, DateOnly end, Vessel vessel, decimal price, int capacity, bool confirmed)
    {
        var points = route.Split(',', StringSplitOptions.TrimEntries).Select(port => Ports.TryGetValue(port, out var at)
            ? new RoutePoint { Name = port, Lat = at.Lat, Lng = at.Lng }
            : new RoutePoint { Name = port }).ToList();
        var cruise = new Cruise
        {
            Name = name,
            RoutePoints = points,
            DeparturePort = points[0].Name,
            ArrivalPort = points[^1].Name,
            IsConfirmed = confirmed,
            StartDate = start,
            EndDate = end,
            Vessel = vessel,
            YachtName = vessel.Name,
            YachtModel = vessel.Model,
            YachtLengthMeters = vessel.LengthMeters,
            Price = price,
            Deposit = Math.Round(price * 0.4m / 10m) * 10m,
            BalanceDueDate = start.AddDays(-28),
            Capacity = capacity,
        };
        db.Cruises.Add(cruise);
        return cruise;
    }

    private static void AddCosts(AppDbContext db, Cruise cruise, params (string Category, decimal Amount)[] costs)
    {
        foreach (var (category, amount) in costs)
        {
            db.CruiseCosts.Add(new CruiseCost { Cruise = cruise, Category = category, Amount = amount });
        }
    }

    /// <summary>Wzorzec: P opłacone, Z sama zaliczka, N bez wpłaty. Daty zapisów rozłożone przed rejsem i nigdy nie z przyszłości.</summary>
    private static void Enroll(AppDbContext db, Random random, Cruise cruise, List<Participant> people, string pattern, DateOnly today)
    {
        var chosen = people.OrderBy(_ => random.Next()).Take(pattern.Length).ToList();
        for (var i = 0; i < chosen.Count; i++)
        {
            var signedUp = cruise.StartDate.AddDays(-random.Next(12, 110));
            if (signedUp > today)
            {
                signedUp = today.AddDays(-random.Next(0, 25));
            }
            var enrollment = new Enrollment
            {
                Cruise = cruise,
                Participant = chosen[i],
                Total = cruise.Price,
                Status = EnrollmentStatus.Confirmed,
                CreatedAt = Utc(signedUp),
            };
            db.Enrollments.Add(enrollment);

            if (pattern[i] != 'N')
            {
                enrollment.Payments.Add(new Payment { Enrollment = enrollment, Amount = cruise.Deposit, PaidOn = signedUp.AddDays(Math.Min(2, today.DayNumber - signedUp.DayNumber)), Method = "Przelew", Note = "Zaliczka" });
            }
            if (pattern[i] == 'P')
            {
                var balanceOn = cruise.BalanceDueDate.AddDays(-random.Next(0, 8));
                if (balanceOn <= signedUp) balanceOn = signedUp.AddDays(1);
                if (balanceOn > today) balanceOn = today;
                enrollment.Payments.Add(new Payment { Enrollment = enrollment, Amount = cruise.Price - cruise.Deposit, PaidOn = balanceOn, Method = "Przelew", Note = "Saldo" });
            }
        }
    }
}
