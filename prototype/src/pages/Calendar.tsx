import { useQuery } from "@tanstack/react-query";
import { ChevronLeft, ChevronRight, Plus } from "lucide-react";
import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { api, type CalendarData, type CalendarItem, type Vessel } from "@/api";
import { useSession } from "@/auth";
import { addDays, LoadState, PageHead, range, today } from "@/components/shared";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { CharterForm } from "@/pages/Charters";
import { CruiseForm } from "@/pages/Cruises";

const WEEKDAYS = ["Pon", "Wt", "Śr", "Czw", "Pt", "Sob", "Nd"];
const VESSEL_COLORS = ["#0b6bde", "#00875f", "#7a4fd0", "#b85a12", "#c0307a"];
const UNASSIGNED = "#5a6a81";
const COURSE_COLOR = "#0f766e";

type Kind = "all" | "cruise" | "charter" | "course";
const KINDS: { id: Kind; label: string; module?: string }[] = [
  { id: "all", label: "Wszystko" },
  { id: "cruise", label: "Rejsy", module: "rejsy" },
  { id: "charter", label: "Czartery", module: "czartery" },
  { id: "course", label: "Szkolenia", module: "szkolenia" },
];
const TYPE_LABEL: Record<CalendarItem["type"], string> = { cruise: "Rejs", block: "Blokada", charter: "Czarter", course: "Szkolenie" };
const LANE = 26;

const monthName = new Intl.DateTimeFormat("pl-PL", { month: "long", year: "numeric", timeZone: "UTC" });
const longDay = new Intl.DateTimeFormat("pl-PL", { day: "numeric", month: "long", timeZone: "UTC" });
const asDate = (iso: string) => new Date(`${iso}T12:00:00Z`);
const monthStart = (iso: string) => `${iso.slice(0, 7)}-01`;
const dayNumber = (iso: string) => Math.round(Date.parse(iso) / 86400000);

function shiftMonth(iso: string, by: number) {
  const d = asDate(iso);
  d.setUTCMonth(d.getUTCMonth() + by);
  return d.toISOString().slice(0, 10);
}

/** Poniedziałek tygodnia, w którym wypada podana data. */
function weekStart(iso: string) {
  return addDays(iso, -((asDate(iso).getUTCDay() + 6) % 7));
}

interface Entry {
  key: string;
  type: CalendarItem["type"];
  id: string;
  title: string;
  detail: string;
  start: string;
  end: string;
  color: string;
}

interface Segment extends Entry {
  column: number;
  span: number;
  lane: number;
  continuesBefore: boolean;
  continuesAfter: boolean;
}

/** Tnie wpisy na odcinki w obrębie tygodnia i układa je w wierszach tak, by się nie nakładały. */
function layoutWeek(week: string, entries: Entry[]): Segment[] {
  const last = addDays(week, 6);
  const lanes: number[] = [];
  return entries
    .filter((e) => e.start <= last && e.end >= week)
    .sort((a, b) => a.start.localeCompare(b.start) || b.end.localeCompare(a.end))
    .map((e) => {
      const from = e.start < week ? week : e.start;
      const to = e.end > last ? last : e.end;
      const column = dayNumber(from) - dayNumber(week);
      const span = dayNumber(to) - dayNumber(from) + 1;
      let lane = lanes.findIndex((end) => end < column);
      if (lane === -1) lane = lanes.length;
      lanes[lane] = column + span - 1;
      return { ...e, column, span, lane, continuesBefore: e.start < week, continuesAfter: e.end > last };
    });
}

export function CalendarPage() {
  const navigate = useNavigate();
  const { hasModule } = useSession();
  const now = today();
  const [month, setMonth] = useState(() => monthStart(now));
  const [vesselFilter, setVesselFilter] = useState<string | null>(null);
  const [kind, setKind] = useState<Kind>("all");
  const [draft, setDraft] = useState<{ vesselId?: string; startDate: string } | null>(null);
  const kinds = KINDS.filter((k) => !k.module || hasModule(k.module));
  // Kliknięcie dnia przy filtrze „Czartery” otwiera rezerwację zamiast nowego rejsu.
  const chartering = kind === "charter";
  const units = useQuery({ queryKey: ["vessels"], queryFn: () => api<Vessel[]>("/vessels"), enabled: chartering });
  const shows = (type: CalendarItem["type"]) => kind === "all" || type === kind || (type === "block" && kind !== "course");

  const firstWeek = weekStart(month);
  const lastDay = addDays(shiftMonth(month, 1), -1);
  const lastWeek = weekStart(lastDay);
  const weeks = useMemo(() => {
    const out: string[] = [];
    for (let w = firstWeek; w <= lastWeek; w = addDays(w, 7)) out.push(w);
    return out;
  }, [firstWeek, lastWeek]);
  const gridEnd = addDays(lastWeek, 6);

  const { data, error, isFetching } = useQuery({
    queryKey: ["calendar", firstWeek, gridEnd],
    queryFn: () => api<CalendarData>(`/calendar?from=${firstWeek}&to=${gridEnd}`),
    placeholderData: (previous) => previous,
  });

  const vessels = data?.vessels ?? [];
  const colorOf = (id: string) => VESSEL_COLORS[Math.max(0, vessels.findIndex((v) => v.id === id)) % VESSEL_COLORS.length];

  const vesselDetail = (name: string, i: CalendarItem) =>
    i.type === "cruise" ? `${name}, zapisanych ${i.confirmed} z ${i.capacity}` : i.type === "charter" ? `${name}: wynajem dla ${i.title}` : `${name}: niedostępna`;

  const entries: Entry[] = [
    ...vessels
      .filter((v) => !vesselFilter || v.id === vesselFilter)
      .flatMap((v) =>
        v.items.filter((i) => shows(i.type)).map((i) => ({
          key: `${v.id}-${i.id}`,
          type: i.type,
          id: i.id,
          title: i.type === "charter" ? `${v.name}: ${i.title}` : i.title,
          detail: vesselDetail(v.name, i),
          start: i.startDate,
          end: i.endDate,
          color: colorOf(v.id),
        })),
      ),
    ...(vesselFilter || !shows("course")
      ? []
      : (data?.courses ?? []).map((i) => ({
          key: i.id,
          type: i.type,
          id: i.id,
          title: i.title,
          detail: `Szkolenie, zapisanych ${i.confirmed} z ${i.capacity}`,
          start: i.startDate,
          end: i.endDate,
          color: COURSE_COLOR,
        }))),
    ...(vesselFilter || !shows("cruise")
      ? []
      : (data?.withoutVessel ?? []).map((i) => ({
          key: i.id,
          type: i.type,
          id: i.id,
          title: i.title,
          detail: `Bez jednostki, zapisanych ${i.confirmed} z ${i.capacity}`,
          start: i.startDate,
          end: i.endDate,
          color: UNASSIGNED,
        }))),
  ];

  return (
    <>
      <PageHead
        title="Kalendarz"
        subtitle={chartering ? "Kliknij dzień, aby zarezerwować jednostkę, albo czarter, aby przejść do rezerwacji." : "Kliknij dzień, aby dodać rejs, albo wpis, aby go otworzyć."}
        actions={
          <Button onClick={() => setDraft({ startDate: now > month && now <= lastDay ? now : month, vesselId: vesselFilter ?? undefined })}>
            <Plus /> {chartering ? "Nowa rezerwacja" : "Dodaj rejs"}
          </Button>
        }
      />

      <div className="card overflow-hidden">
        <div className="flex flex-wrap items-center justify-between gap-3 border-b p-4">
          <div className="flex items-center gap-2">
            <Button variant="outline" size="icon" aria-label="Poprzedni miesiąc" onClick={() => setMonth(shiftMonth(month, -1))}>
              <ChevronLeft />
            </Button>
            <Button variant="outline" size="icon" aria-label="Następny miesiąc" onClick={() => setMonth(shiftMonth(month, 1))}>
              <ChevronRight />
            </Button>
            <Button variant="outline" onClick={() => setMonth(monthStart(now))}>
              Dziś
            </Button>
            <h2 className="ml-2 text-xl first-letter:uppercase" aria-live="polite">
              {monthName.format(asDate(month))}
            </h2>
          </div>
          {kinds.length > 2 && (
            <div className="flex flex-wrap gap-1.5" role="group" aria-label="Pokaż w kalendarzu">
              {kinds.map((k) => (
                <button key={k.id} type="button" aria-pressed={kind === k.id} onClick={() => setKind(k.id)} className={cn("rounded-full border px-3 py-1 text-xs font-semibold transition-colors", kind === k.id ? "border-primary bg-primary text-white" : "text-muted-foreground hover:bg-muted")}>
                  {k.label}
                </button>
              ))}
            </div>
          )}
          {vessels.length > 0 && kind !== "course" && (
            <div className="flex flex-wrap gap-1.5" role="group" aria-label="Filtruj według jednostki">
              <button type="button" aria-pressed={!vesselFilter} onClick={() => setVesselFilter(null)} className={cn("rounded-full border px-3 py-1 text-xs font-semibold transition-colors", !vesselFilter ? "border-primary bg-accent text-primary" : "text-muted-foreground hover:bg-muted")}>
                Wszystkie
              </button>
              {vessels.map((v) => (
                <button key={v.id} type="button" aria-pressed={vesselFilter === v.id} onClick={() => setVesselFilter(vesselFilter === v.id ? null : v.id)} className={cn("flex items-center gap-1.5 rounded-full border px-3 py-1 text-xs font-semibold transition-colors", vesselFilter === v.id ? "border-primary bg-accent text-primary" : "text-muted-foreground hover:bg-muted")}>
                  <span className="size-2.5 rounded-full" style={{ background: colorOf(v.id) }} aria-hidden />
                  {v.name}
                </button>
              ))}
            </div>
          )}
        </div>

        {!data && <div className="p-6"><LoadState error={error} /></div>}
        {data && (
          <div className="transition-opacity" style={{ opacity: isFetching ? 0.6 : 1 }}>
            <div className="grid grid-cols-7 border-b bg-muted/50" aria-hidden>
              {WEEKDAYS.map((d) => (
                <div key={d} className="py-2 text-center text-xs font-semibold text-muted-foreground">
                  {d}
                </div>
              ))}
            </div>
            {weeks.map((week) => {
              const segments = layoutWeek(week, entries);
              const laneCount = Math.max(2, ...segments.map((s) => s.lane + 1));
              return (
                <div key={week} className="relative border-b last:border-b-0" style={{ minHeight: 34 + laneCount * LANE + 8 }}>
                  <div className="absolute inset-0 grid grid-cols-7">
                    {Array.from({ length: 7 }, (_, i) => {
                      const date = addDays(week, i);
                      const outside = date.slice(0, 7) !== month.slice(0, 7);
                      const isToday = date === now;
                      const weekend = i >= 5;
                      return (
                        <button
                          key={date}
                          type="button"
                          aria-label={`${longDay.format(asDate(date))}: ${chartering ? "nowa rezerwacja" : "dodaj rejs"}`}
                          aria-current={isToday ? "date" : undefined}
                          onClick={() => setDraft({ startDate: date, vesselId: vesselFilter ?? undefined })}
                          className={cn("flex flex-col items-start justify-start border-l p-1.5 text-left transition-colors first:border-l-0 hover:bg-accent/70", weekend && "bg-muted/40", outside && "bg-muted/60")}
                        >
                          <span className={cn("grid size-6 place-items-center rounded-full text-xs font-semibold", isToday ? "bg-primary text-white" : outside ? "text-muted-foreground/60" : "text-foreground")}>{asDate(date).getUTCDate()}</span>
                        </button>
                      );
                    })}
                  </div>
                  <div className="pointer-events-none relative grid grid-cols-7 gap-y-1 px-0 pt-[34px] pb-2" style={{ gridAutoRows: LANE - 4 }}>
                    {segments.map((s) => (
                      <button
                        key={s.key}
                        type="button"
                        title={`${s.title}, ${range(s.start, s.end)}. ${s.detail}`}
                        aria-label={`${TYPE_LABEL[s.type]}: ${s.title}, ${range(s.start, s.end)}. ${s.detail}`}
                        onClick={() => navigate(s.type === "cruise" ? `/rejsy/${s.id}` : s.type === "course" ? `/szkolenia/${s.id}` : s.type === "charter" ? "/czartery" : "/flota")}
                        className={cn(
                          "pointer-events-auto mx-0.5 truncate px-2 text-left text-xs font-semibold transition-[filter] hover:brightness-110",
                          s.continuesBefore ? "ml-0 rounded-l-none" : "rounded-l-md",
                          s.continuesAfter ? "mr-0 rounded-r-none" : "rounded-r-md",
                          s.type === "cruise" || s.type === "course" ? "text-white" : s.type === "charter" ? "border-2 bg-white text-foreground" : "border text-foreground",
                        )}
                        style={{
                          gridColumn: `${s.column + 1} / span ${s.span}`,
                          gridRow: s.lane + 1,
                          ...(s.type === "cruise" || s.type === "course"
                            ? { background: s.color }
                            : s.type === "charter"
                              ? { borderColor: s.color, background: `${s.color}1f` }
                              : { borderColor: s.color, background: `repeating-linear-gradient(135deg, #fff, #fff 5px, ${s.color}22 5px, ${s.color}22 10px)` }),
                        }}
                      >
                        {s.title}
                      </button>
                    ))}
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {data && (
        <div className="mt-3 flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground" aria-label="Legenda">
          <span className="flex items-center gap-1.5">
            <span className="h-3 w-5 rounded-sm bg-primary" aria-hidden /> Rejs
          </span>
          <span className="flex items-center gap-1.5">
            <span className="h-3 w-5 rounded-sm border-2 border-primary bg-primary/10" aria-hidden /> Czarter
          </span>
          <span className="flex items-center gap-1.5">
            <span className="h-3 w-5 rounded-sm" style={{ background: COURSE_COLOR }} aria-hidden /> Szkolenie
          </span>
          <span className="flex items-center gap-1.5">
            <span className="h-3 w-5 rounded-sm border border-primary" style={{ background: "repeating-linear-gradient(135deg, #fff, #fff 3px, #0b6bde33 3px, #0b6bde33 6px)" }} aria-hidden /> Niedostępność
          </span>
        </div>
      )}
      {data && entries.length === 0 && <p className="mt-3 text-sm text-muted-foreground">W tym miesiącu nie ma wpisów w tym widoku. Kliknij dzień, aby dodać pierwszy.</p>}
      {draft && !chartering && <CruiseForm key={`${draft.vesselId}-${draft.startDate}`} preset={draft} onClose={() => setDraft(null)} />}
      {draft && chartering && units.data && (
        <CharterForm
          key={`${draft.vesselId}-${draft.startDate}`}
          units={units.data.filter((u) => u.charterDayRate !== null)}
          vesselId={draft.vesselId && units.data.some((u) => u.id === draft.vesselId && u.charterDayRate !== null) ? draft.vesselId : undefined}
          startDate={draft.startDate}
          onClose={() => setDraft(null)}
        />
      )}
    </>
  );
}
