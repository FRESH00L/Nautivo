using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Nautivo.Api.Data;
using Nautivo.Api.Domain;
using Nautivo.Api.Infrastructure;

namespace Nautivo.Api.Features;

/// <summary>Wspólne zasady zapisu osoby na rejs i na szkolenie.</summary>
public static class EnrollmentRules
{
    /// <summary>Sprawdza dane z formularza zapisu, zanim cokolwiek trafi do bazy.</summary>
    public static IResult? ValidateRequest(EnrollRequest request)
    {
        if ((request.ParticipantId is null) == (request.Participant is null))
        {
            return Problems.Bad("Wybierz osobę z listy albo podaj dane nowej osoby.");
        }
        if (request.Participant is { } incoming)
        {
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(incoming, new ValidationContext(incoming), results, true))
            {
                return Problems.Bad(results[0].ErrorMessage ?? "Sprawdź dane osoby.");
            }
        }
        if (request.Total is { } total && (total < 0 || decimal.Round(total, 2) != total))
        {
            return Problems.Bad("Podaj poprawną kwotę do zapłaty.");
        }
        return null;
    }

    /// <summary>
    /// Zwraca istniejącą osobę z bazy klientów albo dodaje nową. `alreadyEnrolled` sprawdza duplikat zapisu
    /// na ten sam rejs lub to samo szkolenie.
    /// </summary>
    public static async Task<(Participant? Participant, IResult? Error)> ResolveParticipantAsync(
        AppDbContext db, EnrollRequest request, Func<Guid, Task<bool>> alreadyEnrolled, CancellationToken ct)
    {
        if (request.ParticipantId is { } participantId)
        {
            var existing = await db.Participants.FirstOrDefaultAsync(p => p.Id == participantId, ct);
            if (existing is null)
            {
                return (null, Problems.NotFound("Nie znaleziono uczestnika."));
            }
            if (existing.AnonymizedAt is not null)
            {
                return (null, Problems.Conflict("Dane tej osoby zostały zanonimizowane."));
            }
            if (await alreadyEnrolled(participantId))
            {
                return (null, Problems.Conflict($"{existing.FullName} jest już zapisany(-a)."));
            }
            return (existing, null);
        }

        var data = request.Participant!;
        var participant = new Participant { FullName = data.FullName.Trim(), Email = Clean(data.Email), Phone = Clean(data.Phone), Experience = Clean(data.Experience) };
        db.Participants.Add(participant);
        return (participant, null);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
