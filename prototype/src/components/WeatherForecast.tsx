import { useQuery } from "@tanstack/react-query";
import { ArrowUp, Wind } from "lucide-react";
import { useState } from "react";
import type { RoutePoint } from "@/api";
import { NautivoTable, type NautivoColumn } from "@/components/nautivo";
import { day, fullDay, LoadState, today } from "@/components/shared";
import { cn } from "@/lib/utils";

/*
 * Prognoza z Open-Meteo (pogoda do 16 dni, fale do 8 dni). Zapytania idą prosto z przeglądarki
 * i zawierają tylko współrzędne portu. Darmowy dostęp jest przeznaczony do testów i prototypów;
 * do użytku komercyjnego potrzebna jest subskrypcja (https://open-meteo.com/en/pricing).
 */

interface Day {
  date: string;
  code: number;
  tempMax: number;
  tempMin: number;
  wind: number;
  gusts: number;
  direction: number;
  rain: number;
  wave: number | null;
  wavePeriod: number | null;
}

const FORECAST_DAYS = 16;

async function loadForecast(lat: number, lng: number): Promise<Day[]> {
  const weatherUrl =
    `https://api.open-meteo.com/v1/forecast?latitude=${lat}&longitude=${lng}` +
    "&daily=weather_code,temperature_2m_max,temperature_2m_min,wind_speed_10m_max,wind_gusts_10m_max,wind_direction_10m_dominant,precipitation_sum" +
    `&wind_speed_unit=kn&timezone=auto&forecast_days=${FORECAST_DAYS}`;
  const marineUrl = `https://marine-api.open-meteo.com/v1/marine?latitude=${lat}&longitude=${lng}&daily=wave_height_max,wave_period_max&timezone=auto&forecast_days=8`;

  const [weather, marine] = await Promise.all([
    fetch(weatherUrl).then((r) => (r.ok ? r.json() : Promise.reject(new Error("Serwis pogody nie odpowiada. Spróbuj później.")))),
    // Fale są dodatkiem: na jeziorach i w głębi lądu ich nie ma, więc błąd tylko je pomija.
    fetch(marineUrl).then((r) => (r.ok ? r.json() : null)).catch(() => null),
  ]);
  const d = weather.daily;
  const waves = new Map<string, { height: number | null; period: number | null }>();
  marine?.daily?.time?.forEach((t: string, i: number) => waves.set(t, { height: marine.daily.wave_height_max[i], period: marine.daily.wave_period_max[i] }));
  return (d.time as string[]).map((date, i) => ({
    date,
    code: d.weather_code[i],
    tempMax: d.temperature_2m_max[i],
    tempMin: d.temperature_2m_min[i],
    wind: d.wind_speed_10m_max[i],
    gusts: d.wind_gusts_10m_max[i],
    direction: d.wind_direction_10m_dominant[i],
    rain: d.precipitation_sum[i],
    wave: waves.get(date)?.height ?? null,
    wavePeriod: waves.get(date)?.period ?? null,
  }));
}

/** Skala Beauforta z prędkości w węzłach. */
export function beaufort(knots: number) {
  return [1, 4, 7, 11, 17, 22, 28, 34, 41, 48, 56, 64].filter((limit) => knots >= limit).length;
}

const COMPASS = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"];
const compass = (degrees: number) => COMPASS[Math.round(degrees / 45) % 8];

/** Kody pogody WMO, których używa Open-Meteo. */
function describe(code: number) {
  if (code === 0) return "Bezchmurnie";
  if (code <= 2) return "Małe zachmurzenie";
  if (code === 3) return "Pochmurno";
  if (code <= 48) return "Mgła";
  if (code <= 57) return "Mżawka";
  if (code <= 67) return "Deszcz";
  if (code <= 77) return "Śnieg";
  if (code <= 82) return "Przelotny deszcz";
  if (code <= 86) return "Przelotny śnieg";
  return "Burza";
}

/** Od 6°B (22 kn) warunki robią się trudne dla mniej doświadczonej załogi, od 8°B (34 kn) to sztorm. */
const windTone = (knots: number) => (knots >= 34 ? "text-late font-semibold" : knots >= 22 ? "text-part font-semibold" : undefined);

const round = (n: number) => Math.round(n);

const COLUMNS: NautivoColumn<Day>[] = [
  { key: "date", header: "Dzień", cell: (d) => <span className="font-medium whitespace-nowrap">{fullDay(d.date)}</span> },
  { key: "sky", header: "Pogoda", cell: (d) => describe(d.code), hideBelow: "md" },
  { key: "temp", header: "Temp.", align: "right", cell: (d) => <span className="whitespace-nowrap tabular-nums">{round(d.tempMax)}° / {round(d.tempMin)}°</span> },
  {
    key: "wind",
    header: "Wiatr",
    align: "right",
    cell: (d) => (
      <span className={cn("inline-flex items-center justify-end gap-1.5 whitespace-nowrap tabular-nums", windTone(d.wind))}>
        {/* Kierunek podajemy skąd wieje; strzałka pokazuje, dokąd. */}
        <ArrowUp className="size-3.5" style={{ transform: `rotate(${d.direction + 180}deg)` }} aria-hidden />
        {compass(d.direction)} {round(d.wind)} kn ({beaufort(d.wind)}°B)
      </span>
    ),
  },
  { key: "gusts", header: "Porywy", align: "right", cell: (d) => <span className={cn("tabular-nums", windTone(d.gusts))}>{round(d.gusts)} kn</span> },
  { key: "waves", header: "Fala", align: "right", cell: (d) => (d.wave == null ? <span className="text-muted-foreground">brak</span> : <span className="tabular-nums whitespace-nowrap">{d.wave.toFixed(1)} m{d.wavePeriod ? `, ${round(d.wavePeriod)} s` : ""}</span>) },
  { key: "rain", header: "Opad", align: "right", hideBelow: "sm", cell: (d) => <span className="tabular-nums">{d.rain.toFixed(1)} mm</span> },
];

/** Prognoza pogody i wiatru dla wybranego portu trasy. Pokazuje dni rejsu, gdy mieszczą się w zasięgu prognozy. */
export function WeatherForecast({ points, startDate, endDate }: { points: RoutePoint[]; startDate: string; endDate: string }) {
  // Ten sam port na starcie i mecie pokazujemy raz.
  const places = points.filter((p, i) => p.lat != null && p.lng != null && points.findIndex((q) => q.lat === p.lat && q.lng === p.lng) === i);
  const [index, setIndex] = useState(0);
  const place = places[Math.min(index, places.length - 1)];
  const now = today();
  const finished = endDate < now;

  const { data, error } = useQuery({
    queryKey: ["weather", place?.lat, place?.lng],
    queryFn: () => loadForecast(place!.lat!, place!.lng!),
    enabled: !!place && !finished,
    staleTime: 30 * 60 * 1000,
    retry: 1,
  });

  if (!place || finished) return null;

  const cruiseDays = data?.filter((d) => d.date >= startDate && d.date <= endDate) ?? [];
  const showCruise = cruiseDays.length > 0;
  const days = showCruise ? cruiseDays : (data ?? []).slice(0, 7);
  const lastForecastDay = data?.[data.length - 1]?.date;
  const strongest = days.reduce((max, d) => Math.max(max, d.gusts), 0);

  return (
    <section className="mt-6" aria-labelledby="weather">
      <div className="mb-2 flex flex-wrap items-end justify-between gap-2">
        <h2 id="weather" className="flex items-center gap-2 text-xl">
          <Wind className="size-5 text-primary" aria-hidden /> Pogoda i wiatr
        </h2>
        {places.length > 1 && (
          <div className="flex flex-wrap gap-1 rounded-xl bg-muted p-1" role="group" aria-label="Port prognozy">
            {places.map((p, i) => (
              <button
                key={`${p.name}-${p.lat}`}
                type="button"
                aria-pressed={i === index}
                onClick={() => setIndex(i)}
                className={cn("rounded-lg px-3 py-1 text-sm font-medium transition-colors", i === index ? "bg-white text-sea shadow-sm" : "text-muted-foreground hover:text-foreground")}
              >
                {p.name}
              </button>
            ))}
          </div>
        )}
      </div>
      <p className="mb-3 text-sm text-muted-foreground">
        {!data
          ? `Prognoza dla: ${place.name}.`
          : showCruise
            ? `Prognoza dla: ${place.name}, na dni rejsu w zasięgu prognozy (do ${fullDay(lastForecastDay!)}).`
            : `Bieżąca prognoza dla: ${place.name}, najbliższe 7 dni. Prognoza na dni rejsu pojawi się około ${FORECAST_DAYS} dni przed startem (${day(startDate)}).`}
        {data && strongest >= 22 && <span className="ml-1 font-semibold text-part">Uwaga: porywy do {round(strongest)} kn ({beaufort(strongest)}°B).</span>}
      </p>
      {!data ? (
        <LoadState error={error} text="Pobieram prognozę…" />
      ) : (
        <NautivoTable columns={COLUMNS} rows={days} rowKey={(d) => d.date} caption={`Prognoza pogody: ${place.name}`} minWidth={560} />
      )}
      <p className="mt-2 text-xs text-muted-foreground">
        Dane: <a href="https://open-meteo.com/" target="_blank" rel="noreferrer" className="underline underline-offset-2">Open-Meteo.com</a> (CC BY 4.0). Prognoza modelowa, nie zastępuje komunikatów meteo dla żeglugi.
      </p>
    </section>
  );
}
