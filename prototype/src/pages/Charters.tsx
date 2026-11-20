import { useQuery } from "@tanstack/react-query";
import { Anchor, CalendarPlus, ChevronLeft, ChevronRight, ClipboardCheck, Plus, Sailboat, Ship, Users, Wallet, Waves, type LucideIcon } from "lucide-react";
import { useState } from "react";
import { api, type Charter, type CharterClient, type CharterSummary, type ChecklistItem, type Vessel, type VesselKind } from "@/api";
import { NautivoButton, NautivoIconButton, NautivoRowActions, NautivoTable, type NautivoColumn } from "@/components/nautivo";
import { addDays, ConfirmDialog, EmptyState, Field, FormError, LoadState, PageHead, plural, range, selectClass, today, useAction, useFormSubmit, zl } from "@/components/shared";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { cn } from "@/lib/utils";
import { KIND_LABEL, VesselForm } from "@/pages/Fleet";

const KIND_ICON: Record<VesselKind, LucideIcon> = { Yacht: Sailboat, Sailboat: Sailboat, Motorboat: Ship, Jetski: Waves, Kayak: Waves, Other: Anchor };
const MONTHS = ["Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec", "Lipiec", "Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień"];
const CHECKLIST_SUGGESTIONS = ["Kamizelki ratunkowe", "Koło ratunkowe", "Gaśnica", "Apteczka", "Kotwica z liną", "Odbijacze", "Cumy", "Paliwo do pełna", "Latarka", "Czerpak"];

const dayNumber = (iso: string) => Math.round(Date.parse(iso) / 86400000);
/** Dzień zwrotu się nie liczy (sobota–sobota to 7 dni); wynajem w jednym dniu to 1 dzień. Tak samo liczy API. */
export const charterDays = (start: string, end: string) => Math.max(1, dayNumber(end) - dayNumber(start));
/** Ta sama zasada co w API: nowy wynajem może zacząć się w dniu zwrotu poprzedniego, ale nie może się z nim nakładać. */
const clashes = (a: { startDate: string; endDate: string }, b: { startDate: string; endDate: string }) =>
  (a.startDate < b.endDate && a.endDate > b.startDate) || a.startDate === b.startDate;

type Tab = "summary" | "units" | "bookings";

export function ChartersPage() {
  const [tab, setTab] = useState<Tab>("summary");
  const vessels = useQuery({ queryKey: ["vessels"], queryFn: () => api<Vessel[]>("/vessels") });
  const units = (vessels.data ?? []).filter((v) => v.charterDayRate !== null);
  const [editingUnit, setEditingUnit] = useState<Vessel | "new" | null>(null);
  const [booking, setBooking] = useState<{ charter?: Charter; vesselId?: string } | null>(null);
  const [checklistFor, setChecklistFor] = useState<Vessel | null>(null);
  const [deletingUnit, setDeletingUnit] = useState<Vessel | null>(null);
  const removeUnit = useAction((id: string) => api(`/vessels/${id}`, { method: "DELETE" }), "Usunięto jednostkę");

  const tabs: { id: Tab; label: string }[] = [
    { id: "summary", label: "Podsumowanie" },
    { id: "units", label: `Jednostki (${units.length})` },
    { id: "bookings", label: "Rezerwacje" },
  ];

  return (
    <>
      <PageHead
        title="Czartery"
        subtitle="Wynajem łodzi, motorówek, jachtów i skuterów na dni. Kliknij jednostkę, aby ją zarezerwować."
        actions={
          <>
            <NautivoButton variant="secondary" icon={CalendarPlus} disabled={units.length === 0} onClick={() => setBooking({})}>
              Nowa rezerwacja
            </NautivoButton>
            <NautivoButton icon={Plus} onClick={() => setEditingUnit("new")}>
              Dodaj jednostkę
            </NautivoButton>
          </>
        }
      />

      <div className="mb-5 flex flex-wrap gap-1.5" role="tablist" aria-label="Widok czarterów">
        {tabs.map((t) => (
          <button
            key={t.id}
            type="button"
            role="tab"
            aria-selected={tab === t.id}
            onClick={() => setTab(t.id)}
            className={cn("rounded-full border px-4 py-1.5 text-sm font-semibold transition-colors", tab === t.id ? "border-primary bg-accent text-primary" : "text-muted-foreground hover:bg-muted")}
          >
            {t.label}
          </button>
        ))}
      </div>

      {!vessels.data && <LoadState error={vessels.error} />}
      {vessels.data && tab === "summary" && <SummaryView />}
      {vessels.data && tab === "units" && (
        <UnitsView
          units={units}
          onAdd={() => setEditingUnit("new")}
          onBook={(v) => setBooking({ vesselId: v.id })}
          onEdit={setEditingUnit}
          onChecklist={setChecklistFor}
          onDelete={setDeletingUnit}
        />
      )}
      {vessels.data && tab === "bookings" && <BookingsView units={units} onEdit={(charter) => setBooking({ charter })} />}

      {editingUnit && <VesselForm forCharter vessel={editingUnit === "new" ? undefined : editingUnit} onClose={() => setEditingUnit(null)} />}
      {booking && <CharterForm units={units} charter={booking.charter} vesselId={booking.vesselId} onClose={() => setBooking(null)} />}
      {checklistFor && <ChecklistDialog vessel={checklistFor} onClose={() => setChecklistFor(null)} />}
      <ConfirmDialog
        open={!!deletingUnit}
        title={`Usunąć jednostkę: ${deletingUnit?.name ?? ""}?`}
        text="Jednostki z rezerwacjami lub rejsami nie można usunąć. Możesz ją oznaczyć jako niedostępną albo usunąć cenę czarteru."
        confirmLabel="Usuń jednostkę"
        onConfirm={async () => {
          await removeUnit.mutateAsync(deletingUnit!.id);
        }}
        onClose={() => setDeletingUnit(null)}
      />
    </>
  );
}

/* ------------------------------------------------------------- Podsumowanie */

function Stat({ icon: Icon, value, label, note }: { icon: LucideIcon; value: string; label: string; note: string }) {
  return (
    <div className="card flex items-start gap-3.5 p-5">
      <span className="grid size-11 shrink-0 place-items-center rounded-xl bg-accent text-primary">
        <Icon className="size-6" aria-hidden />
      </span>
      <div className="min-w-0">
        <p className="text-[1.65rem] leading-tight font-bold tracking-tight">{value}</p>
        <p className="mt-0.5 text-sm text-muted-foreground">{label}</p>
        <p className="mt-1 text-xs text-muted-foreground">{note}</p>
      </div>
    </div>
  );
}

function SummaryView() {
  const [year, setYear] = useState(() => Number(today().slice(0, 4)));
  const { data, error, isFetching } = useQuery({
    queryKey: ["charter-summary", year],
    queryFn: () => api<CharterSummary>(`/charters/summary?year=${year}`),
    placeholderData: (previous) => previous,
  });
  if (!data) return <LoadState error={error} />;
  const maxRevenue = Math.max(1, ...data.months.map((m) => m.revenue));
  const totals = data.perUnit.reduce((t, u) => ({ bookings: t.bookings + u.bookings, days: t.days + u.days, listValue: t.listValue + u.listValue, discounts: t.discounts + u.discounts, revenue: t.revenue + u.revenue }), {
    bookings: 0,
    days: 0,
    listValue: 0,
    discounts: 0,
    revenue: 0,
  });

  const unitColumns: NautivoColumn<CharterSummary["perUnit"][number]>[] = [
    {
      key: "name",
      header: "Jednostka",
      cell: (u) => (
        <>
          <span className="font-medium">{u.name}</span>
          <span className="block text-xs text-muted-foreground">
            {KIND_LABEL[u.kind]}
            {u.status === "OutOfService" && ", niedostępna"}
          </span>
        </>
      ),
    },
    { key: "rate", header: "Cena / dzień", align: "right", cell: (u) => (u.dayRate !== null ? zl(u.dayRate) : "Nie wynajmowana") },
    { key: "bookings", header: "Wynajmy", align: "right", cell: (u) => u.bookings },
    { key: "days", header: "Dni", align: "right", cell: (u) => u.days },
    { key: "list", header: "Wg cennika", align: "right", hideBelow: "md", cell: (u) => zl(u.listValue) },
    { key: "discounts", header: "Rabaty", align: "right", hideBelow: "md", cell: (u) => (u.discounts > 0 ? <span className="text-late">−{zl(u.discounts)}</span> : u.discounts < 0 ? `+${zl(-u.discounts)}` : "—") },
    { key: "revenue", header: "Przychód", align: "right", cell: (u) => <span className="font-semibold">{zl(u.revenue)}</span> },
    {
      key: "util",
      header: "Obłożenie",
      align: "right",
      hideBelow: "sm",
      cell: (u) => (
        <span title="Wynajęte dni w stosunku do dni w roku">
          {u.utilizationPercent}%
        </span>
      ),
    },
    {
      key: "checklist",
      header: "Wyposażenie",
      align: "right",
      hideBelow: "sm",
      cell: (u) => (u.checklistTotal === 0 ? <span className="text-muted-foreground">Brak listy</span> : u.checklistMissing > 0 ? <span className="font-medium text-late">{u.checklistMissing} do uzupełnienia</span> : <span className="text-paid">Kompletne</span>),
    },
  ];

  return (
    <div className="grid gap-6 transition-opacity" style={{ opacity: isFetching ? 0.6 : 1 }}>
      <div className="flex items-center gap-2">
        <NautivoButton variant="secondary" size="sm" aria-label="Poprzedni rok" onClick={() => setYear(year - 1)}>
          <ChevronLeft aria-hidden />
        </NautivoButton>
        <h2 className="min-w-16 text-center text-xl" aria-live="polite">
          {year}
        </h2>
        <NautivoButton variant="secondary" size="sm" aria-label="Następny rok" onClick={() => setYear(year + 1)}>
          <ChevronRight aria-hidden />
        </NautivoButton>
      </div>

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4" aria-label="Najważniejsze liczby czarterów">
        <Stat icon={Sailboat} value={String(data.units)} label="Jednostki do wynajęcia" note={`Dostępne: ${data.unitsAvailable}, nadchodzące rezerwacje: ${data.upcoming}`} />
        <Stat icon={Wallet} value={zl(data.revenueYear)} label={`Przychód w ${data.year}`} note={`W tym miesiącu: ${zl(data.revenueThisMonth)}. ${plural(data.bookingsYear, "wynajem", "wynajmy", "wynajmów")}, ${plural(data.daysYear, "dzień", "dni", "dni")}`} />
        <Stat icon={Users} value={String(data.clients)} label="Klienci czarterów" note={`W ${data.year}: ${data.clientsThisYear}`} />
        <Stat icon={ClipboardCheck} value={String(data.checklistMissing)} label="Pozycje do uzupełnienia" note="Braki w wyposażeniu wszystkich jednostek" />
      </section>

      <section className="card p-5" aria-labelledby="months-title">
        <h2 id="months-title" className="text-lg">
          Podsumowanie miesięczne
        </h2>
        <p className="text-sm text-muted-foreground">Przychód to uzgodniona kwota wynajmu (po rabatach), przypisana do miesiąca odbioru jednostki. Wpłat w czarterach jeszcze nie rejestrujemy.</p>
        <div className="mt-4 flex h-40 items-end gap-1.5" aria-hidden>
          {data.months.map((m) => (
            <div key={m.month} className="flex h-full min-w-0 flex-1 flex-col items-center justify-end gap-1">
              <div className="w-full rounded-t-md bg-primary/85" style={{ height: `${(m.revenue / maxRevenue) * 100}%`, minHeight: m.revenue > 0 ? 3 : 0 }} title={`${MONTHS[m.month - 1]}: ${zl(m.revenue)}`} />
              <span className="text-[0.65rem] text-muted-foreground">{MONTHS[m.month - 1].slice(0, 3)}</span>
            </div>
          ))}
        </div>
        <div className="mt-4 overflow-x-auto">
          <table className="w-full min-w-[420px] text-left text-sm">
            <caption className="sr-only">Wynajmy, dni i przychód w kolejnych miesiącach</caption>
            <thead className="border-b text-muted-foreground">
              <tr>
                <th scope="col" className="p-2 font-medium">Miesiąc</th>
                <th scope="col" className="p-2 text-right font-medium">Wynajmy</th>
                <th scope="col" className="p-2 text-right font-medium">Dni</th>
                <th scope="col" className="p-2 text-right font-medium">Przychód</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {data.months.map((m) => (
                <tr key={m.month} className={cn(m.bookings === 0 && "text-muted-foreground")}>
                  <td className="p-2">{MONTHS[m.month - 1]}</td>
                  <td className="p-2 text-right">{m.bookings}</td>
                  <td className="p-2 text-right">{m.days}</td>
                  <td className="p-2 text-right">{zl(m.revenue)}</td>
                </tr>
              ))}
            </tbody>
            <tfoot className="border-t font-semibold">
              <tr>
                <td className="p-2">Razem</td>
                <td className="p-2 text-right">{data.bookingsYear}</td>
                <td className="p-2 text-right">{data.daysYear}</td>
                <td className="p-2 text-right">{zl(data.revenueYear)}</td>
              </tr>
            </tfoot>
          </table>
        </div>
      </section>

      <section aria-labelledby="units-title">
        <h2 id="units-title" className="mb-1 text-lg">
          Koszty wynajmu per jednostka
        </h2>
        <p className="mb-3 text-sm text-muted-foreground">„Wg cennika” to cena za dzień razy liczba dni. Różnica do przychodu to udzielone rabaty.</p>
        {data.perUnit.length === 0 ? (
          <EmptyState title="Brak jednostek do wynajęcia" text="Dodaj jednostkę z ceną za dzień, aby zobaczyć jej rozliczenie." />
        ) : (
          <NautivoTable
            columns={unitColumns}
            rows={data.perUnit}
            rowKey={(u) => u.vesselId}
            caption="Rozliczenie wynajmu per jednostka"
            minWidth={560}
            footer={
              <tr>
                <td className="p-3">Razem</td>
                <td className="p-3" />
                <td className="p-3 text-right">{totals.bookings}</td>
                <td className="p-3 text-right">{totals.days}</td>
                <td className="hidden p-3 text-right md:table-cell">{zl(totals.listValue)}</td>
                <td className="hidden p-3 text-right md:table-cell">{totals.discounts > 0 ? `−${zl(totals.discounts)}` : "—"}</td>
                <td className="p-3 text-right">{zl(totals.revenue)}</td>
                <td className="hidden p-3 sm:table-cell" />
                <td className="hidden p-3 sm:table-cell" />
              </tr>
            }
          />
        )}
      </section>
    </div>
  );
}

/* ------------------------------------------------------------- Jednostki */

function UnitsView({
  units,
  onAdd,
  onBook,
  onEdit,
  onChecklist,
  onDelete,
}: {
  units: Vessel[];
  onAdd: () => void;
  onBook: (v: Vessel) => void;
  onEdit: (v: Vessel) => void;
  onChecklist: (v: Vessel) => void;
  onDelete: (v: Vessel) => void;
}) {
  if (units.length === 0) {
    return (
      <EmptyState
        title="Nie ma jeszcze jednostek do wynajęcia"
        text="Dodaj łódź, motorówkę, jacht lub skuter z ceną za dzień. Jednostki z floty pojawią się tu po ustawieniu ceny czarteru."
        action={
          <NautivoButton icon={Plus} onClick={onAdd}>
            Dodaj jednostkę
          </NautivoButton>
        }
      />
    );
  }
  return (
    <ul className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
      {units.map((v) => {
        const Icon = KIND_ICON[v.kind];
        const missing = v.checklist.filter((i) => !i.isOk);
        const available = v.status === "InService";
        return (
          <li key={v.id} className="card flex flex-col p-4">
            <button type="button" disabled={!available} onClick={() => onBook(v)} className="-m-1 rounded-lg p-1 text-left transition-colors enabled:hover:bg-accent/60 disabled:cursor-not-allowed" aria-label={`Zarezerwuj: ${v.name}`}>
              <div className="flex items-start gap-3">
                <span className="grid size-11 shrink-0 place-items-center rounded-xl bg-accent text-primary">
                  <Icon className="size-6" aria-hidden />
                </span>
                <div className="min-w-0 flex-1">
                  <h2 className="truncate text-lg">{v.name}</h2>
                  <p className="truncate text-sm text-muted-foreground">{[KIND_LABEL[v.kind], v.model, `${v.berths} miejsc`].filter(Boolean).join(", ")}</p>
                </div>
                <span className={cn("shrink-0 rounded-full px-2.5 py-0.5 text-xs font-semibold", available ? "bg-paid-soft text-paid" : "bg-late-soft text-late")}>{available ? "Dostępna" : "Niedostępna"}</span>
              </div>
              <p className="mt-3 text-2xl font-bold tracking-tight">
                {zl(v.charterDayRate ?? 0)} <span className="text-sm font-normal text-muted-foreground">/ dzień</span>
              </p>
              <p className="text-sm text-muted-foreground">Nadchodzące rezerwacje: {v.upcomingCharters}</p>
            </button>
            <div className="mt-3 rounded-lg bg-muted/60 p-2.5 text-sm">
              {v.checklist.length === 0 ? (
                <span className="text-muted-foreground">Brak listy wyposażenia.</span>
              ) : missing.length === 0 ? (
                <span className="text-paid">Wyposażenie kompletne ({v.checklist.length} poz.)</span>
              ) : (
                <>
                  <span className="font-medium text-late">Do uzupełnienia: {missing.length}</span>
                  <span className="block truncate text-xs text-muted-foreground">{missing.map((i) => i.name).join(", ")}</span>
                </>
              )}
            </div>
            <div className="mt-3 flex flex-wrap items-center gap-2 pt-1">
              <NautivoButton size="sm" icon={CalendarPlus} disabled={!available} onClick={() => onBook(v)}>
                Zarezerwuj
              </NautivoButton>
              <NautivoButton size="sm" variant="secondary" icon={ClipboardCheck} onClick={() => onChecklist(v)}>
                Wyposażenie
              </NautivoButton>
              <span className="ml-auto flex gap-1">
                <NautivoIconButton action="edit" size="sm" label={v.name} onClick={() => onEdit(v)} />
                <NautivoIconButton action="delete" size="sm" label={v.name} onClick={() => onDelete(v)} />
              </span>
            </div>
          </li>
        );
      })}
    </ul>
  );
}

/* ------------------------------------------------------------- Rezerwacje */

function BookingsView({ units, onEdit }: { units: Vessel[]; onEdit: (c: Charter) => void }) {
  const { data, error } = useQuery({ queryKey: ["charters"], queryFn: () => api<Charter[]>("/charters") });
  const [when, setWhen] = useState<"upcoming" | "past" | "all">("upcoming");
  const [unit, setUnit] = useState("");
  const [deleting, setDeleting] = useState<Charter | null>(null);
  const remove = useAction((id: string) => api(`/charters/${id}`, { method: "DELETE" }), "Usunięto rezerwację");
  if (!data) return <LoadState error={error} />;

  const now = today();
  const rows = data
    .filter((c) => (when === "upcoming" ? c.endDate >= now : when === "past" ? c.endDate < now : true))
    .filter((c) => !unit || c.vesselId === unit)
    .sort((a, b) => (when === "past" ? b.startDate.localeCompare(a.startDate) : a.startDate.localeCompare(b.startDate)));

  const columns: NautivoColumn<Charter>[] = [
    {
      key: "unit",
      header: "Jednostka",
      cell: (c) => (
        <>
          <span className="font-medium">{c.vesselName}</span>
          <span className="block text-xs text-muted-foreground">{KIND_LABEL[c.vesselKind]}</span>
        </>
      ),
    },
    {
      key: "client",
      header: "Klient",
      cell: (c) => (
        <>
          <span>{c.clientName}</span>
          <span className="block text-xs text-muted-foreground">{c.clientPhone ?? c.clientEmail ?? "Bez kontaktu"}</span>
        </>
      ),
    },
    {
      key: "dates",
      header: "Termin",
      cell: (c) => (
        <>
          {range(c.startDate, c.endDate)}
          {c.startDate <= now && c.endDate >= now && <span className="ml-2 rounded-full bg-paid-soft px-2 py-0.5 text-xs font-semibold text-paid">Trwa</span>}
        </>
      ),
    },
    { key: "days", header: "Dni", align: "right", cell: (c) => c.days },
    {
      key: "total",
      header: "Kwota",
      align: "right",
      cell: (c) => {
        const list = c.dayRate * c.days;
        return (
          <>
            <span className="font-semibold">{zl(c.total)}</span>
            {c.total < list && <span className="block text-xs text-muted-foreground">rabat {zl(list - c.total)}</span>}
          </>
        );
      },
    },
    {
      key: "actions",
      header: "Akcje",
      srOnlyHeader: true,
      align: "right",
      cell: (c) => (
        <NautivoRowActions>
          <NautivoIconButton action="edit" size="sm" label={`${c.vesselName}, ${c.clientName}`} onClick={() => onEdit(c)} />
          <NautivoIconButton action="delete" size="sm" label={`${c.vesselName}, ${c.clientName}`} onClick={() => setDeleting(c)} />
        </NautivoRowActions>
      ),
    },
  ];

  return (
    <>
      <div className="mb-4 flex flex-wrap items-center gap-3">
        <div className="flex gap-1.5" role="group" aria-label="Okres rezerwacji">
          {(
            [
              ["upcoming", "Nadchodzące i trwające"],
              ["past", "Zakończone"],
              ["all", "Wszystkie"],
            ] as const
          ).map(([value, label]) => (
            <button key={value} type="button" aria-pressed={when === value} onClick={() => setWhen(value)} className={cn("rounded-full border px-3 py-1 text-xs font-semibold transition-colors", when === value ? "border-primary bg-accent text-primary" : "text-muted-foreground hover:bg-muted")}>
              {label}
            </button>
          ))}
        </div>
        <select className={cn(selectClass, "h-9 w-auto")} aria-label="Filtruj według jednostki" value={unit} onChange={(e) => setUnit(e.target.value)}>
          <option value="">Wszystkie jednostki</option>
          {units.map((u) => (
            <option key={u.id} value={u.id}>
              {u.name}
            </option>
          ))}
        </select>
      </div>
      {rows.length === 0 ? (
        <EmptyState title="Brak rezerwacji" text={when === "upcoming" ? "Nie ma nadchodzących wynajmów. Kliknij jednostkę, aby ją zarezerwować." : "Nie ma rezerwacji w tym widoku."} />
      ) : (
        <NautivoTable columns={columns} rows={rows} rowKey={(c) => c.id} caption="Rezerwacje czarterowe" minWidth={720} />
      )}
      <ConfirmDialog
        open={!!deleting}
        title="Usunąć rezerwację?"
        text={deleting ? `${deleting.vesselName}, ${deleting.clientName}, ${range(deleting.startDate, deleting.endDate)}. Termin zwolni się w kalendarzu.` : ""}
        confirmLabel="Usuń rezerwację"
        onConfirm={async () => {
          await remove.mutateAsync(deleting!.id);
        }}
        onClose={() => setDeleting(null)}
      />
    </>
  );
}

/* ------------------------------------------------------------- Formularz rezerwacji */

export function CharterForm({ units, charter, vesselId, startDate, onClose }: { units: Vessel[]; charter?: Charter; vesselId?: string; startDate?: string; onClose: () => void }) {
  const clients = useQuery({ queryKey: ["charter-clients"], queryFn: () => api<CharterClient[]>("/charters/clients") });
  const bookings = useQuery({ queryKey: ["charters"], queryFn: () => api<Charter[]>("/charters") });
  const bookable = units.filter((u) => u.status === "InService" || u.id === charter?.vesselId);
  const initialStart = charter?.startDate ?? startDate ?? today();

  const [form, setForm] = useState({
    vesselId: charter?.vesselId ?? vesselId ?? (bookable.length === 1 ? bookable[0].id : ""),
    startDate: initialStart,
    endDate: charter?.endDate ?? addDays(initialStart, 1),
    notes: charter?.notes ?? "",
  });
  const [mode, setMode] = useState<"new" | "existing">(charter ? "existing" : "new");
  const [participantId, setParticipantId] = useState(charter?.participantId ?? "");
  const [person, setPerson] = useState({ fullName: "", email: "", phone: "" });
  // Przy edycji zachowujemy kwotę z rabatem; kwota cennikowa przelicza się razem z terminem.
  const [customTotal, setCustomTotal] = useState<string | null>(charter && charter.total !== charter.dayRate * charter.days ? String(charter.total) : null);

  const unit = units.find((u) => u.id === form.vesselId);
  const rate = charter && unit?.id === charter.vesselId ? charter.dayRate : (unit?.charterDayRate ?? 0);
  const validDates = form.endDate >= form.startDate;
  const days = validDates ? charterDays(form.startDate, form.endDate) : 0;
  const estimate = rate * days;
  const total = customTotal ?? String(estimate);
  const discount = estimate - Number(total || 0);

  const others = (bookings.data ?? []).filter((c) => c.vesselId === form.vesselId && c.id !== charter?.id && c.endDate >= today());
  const clash = validDates ? others.find((c) => clashes(c, form)) : undefined;

  const save = useAction(
    () => {
      const client = mode === "existing" ? { participantId } : { participant: { fullName: person.fullName, email: person.email || null, phone: person.phone || null } };
      const body = { ...form, notes: form.notes || null, total: Number(total), ...client };
      return charter ? api(`/charters/${charter.id}`, { method: "PUT", body }) : api("/charters", { method: "POST", body });
    },
    charter ? "Zapisano zmiany w rezerwacji" : `Zarezerwowano: ${unit?.name ?? "jednostka"}`,
  );
  const { submit, error, busy } = useFormSubmit(async () => {
    await save.mutateAsync(undefined);
    onClose();
  });
  const setPersonField = (key: keyof typeof person) => (e: { target: { value: string } }) => setPerson((p) => ({ ...p, [key]: e.target.value }));

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="sm:max-w-lg">
        <form className="grid gap-4" onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>{charter ? "Edytuj rezerwację" : unit ? `Rezerwacja: ${unit.name}` : "Nowa rezerwacja"}</DialogTitle>
            <DialogDescription>Odbiór i zwrot jednostki. Dzień zwrotu może być dniem odbioru przez kolejnego klienta.</DialogDescription>
          </DialogHeader>

          {(!vesselId || charter) && (
            <Field label="Jednostka">
              <select className={selectClass} required value={form.vesselId} onChange={(e) => setForm({ ...form, vesselId: e.target.value })}>
                <option value="">Wybierz jednostkę</option>
                {bookable.map((u) => (
                  <option key={u.id} value={u.id}>
                    {u.name} ({KIND_LABEL[u.kind]}, {zl(u.charterDayRate ?? 0)} / dzień)
                  </option>
                ))}
              </select>
            </Field>
          )}

          <div className="grid grid-cols-2 gap-3">
            <Field label="Od (odbiór)">
              <Input
                type="date"
                required
                value={form.startDate}
                onChange={(e) => {
                  const start = e.target.value;
                  setForm((f) => ({ ...f, startDate: start, endDate: f.endDate < start ? addDays(start, 1) : f.endDate }));
                }}
              />
            </Field>
            <Field label="Do (zwrot)">
              <Input type="date" required min={form.startDate} value={form.endDate} onChange={(e) => setForm({ ...form, endDate: e.target.value })} />
            </Field>
          </div>

          {others.length > 0 && (
            <div className="rounded-md border p-2.5 text-sm">
              <p className="font-medium">Zajęte terminy tej jednostki</p>
              <ul className="mt-1 text-xs text-muted-foreground">
                {others.slice(0, 5).map((c) => (
                  <li key={c.id} className={cn(clash?.id === c.id && "font-semibold text-late")}>
                    {range(c.startDate, c.endDate)}: {c.clientName}
                  </li>
                ))}
              </ul>
            </div>
          )}
          {clash && (
            <p className="rounded-sm bg-late-soft p-2 text-sm text-late" role="alert">
              Termin nakłada się z wynajmem {range(clash.startDate, clash.endDate)} ({clash.clientName}). Odbiór jest możliwy najwcześniej w dniu zwrotu.
            </p>
          )}

          <div className="rounded-lg bg-accent/70 p-3">
            <p className="text-sm text-muted-foreground">Szacowany koszt</p>
            <p className="text-lg font-semibold">
              {plural(days, "dzień", "dni", "dni")} × {zl(rate)} = {zl(estimate)}
            </p>
            <div className="mt-2 grid grid-cols-[1fr_auto] items-end gap-2">
              <Field label="Kwota do zapłaty (zł)" hint="Możesz ją zmienić, np. przy rabacie.">
                <Input type="number" min={0} step={0.01} required value={total} onChange={(e) => setCustomTotal(e.target.value)} />
              </Field>
              {customTotal !== null && (
                <NautivoButton type="button" size="sm" variant="ghost" className="mb-6" onClick={() => setCustomTotal(null)}>
                  Przywróć wyliczoną
                </NautivoButton>
              )}
            </div>
            {customTotal !== null && discount !== 0 && estimate > 0 && (
              <p className={cn("text-sm", discount > 0 ? "text-paid" : "text-muted-foreground")}>
                {discount > 0 ? `Rabat ${zl(discount)} (${Math.round((discount / estimate) * 100)}%)` : `Dopłata ${zl(-discount)} ponad cennik`}
              </p>
            )}
          </div>

          <fieldset className="grid gap-3">
            <legend className="mb-2 text-sm font-medium">Klient</legend>
            <div className="flex gap-4 text-sm">
              <label className="flex items-center gap-1.5">
                <input type="radio" name="client-mode" checked={mode === "new"} onChange={() => setMode("new")} /> Nowy klient
              </label>
              <label className="flex items-center gap-1.5">
                <input type="radio" name="client-mode" checked={mode === "existing"} onChange={() => setMode("existing")} disabled={!clients.data?.length} /> Z bazy klientów
              </label>
            </div>
            {mode === "existing" ? (
              <Field label="Klient z bazy">
                <select className={selectClass} required value={participantId} onChange={(e) => setParticipantId(e.target.value)}>
                  <option value="">Wybierz osobę</option>
                  {clients.data?.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.fullName}
                      {p.phone ? `, ${p.phone}` : p.email ? `, ${p.email}` : ""}
                    </option>
                  ))}
                </select>
              </Field>
            ) : (
              <>
                <Field label="Imię i nazwisko">
                  <Input required maxLength={200} value={person.fullName} onChange={setPersonField("fullName")} />
                </Field>
                <div className="grid grid-cols-2 gap-3">
                  <Field label="Telefon">
                    <Input type="tel" maxLength={40} value={person.phone} onChange={setPersonField("phone")} />
                  </Field>
                  <Field label="E-mail">
                    <Input type="email" maxLength={254} value={person.email} onChange={setPersonField("email")} />
                  </Field>
                </div>
              </>
            )}
          </fieldset>

          <Field label="Notatki">
            <Textarea maxLength={1000} value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} placeholder="np. kaucja, godzina odbioru, numer dokumentu do wglądu" />
          </Field>

          <FormError message={error} />
          <DialogFooter>
            <NautivoButton type="button" variant="secondary" onClick={onClose}>
              Anuluj
            </NautivoButton>
            <NautivoButton type="submit" loading={busy} disabled={!validDates || !form.vesselId || !!clash}>
              {charter ? "Zapisz zmiany" : "Zatwierdź rezerwację"}
            </NautivoButton>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

/* ------------------------------------------------------------- Checklista wyposażenia */

function ChecklistDialog({ vessel, onClose }: { vessel: Vessel; onClose: () => void }) {
  const [items, setItems] = useState<ChecklistItem[]>(vessel.checklist);
  const [newName, setNewName] = useState("");
  const save = useAction(() => api(`/vessels/${vessel.id}/checklist`, { method: "PUT", body: { items } }), `Zapisano wyposażenie: ${vessel.name}`);
  const { submit, error, busy } = useFormSubmit(async () => {
    await save.mutateAsync(undefined);
    onClose();
  });
  const update = (index: number, patch: Partial<ChecklistItem>) => setItems((list) => list.map((item, i) => (i === index ? { ...item, ...patch } : item)));
  const add = (name: string) => {
    const clean = name.trim();
    if (!clean || items.some((i) => i.name.toLowerCase() === clean.toLowerCase())) return;
    setItems((list) => [...list, { name: clean, isOk: true, note: null }]);
  };
  const suggestions = CHECKLIST_SUGGESTIONS.filter((s) => !items.some((i) => i.name.toLowerCase().startsWith(s.toLowerCase())));
  const missing = items.filter((i) => !i.isOk).length;

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="sm:max-w-xl">
        <form className="grid gap-4" onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>Wyposażenie: {vessel.name}</DialogTitle>
            <DialogDescription>Zaznacz, co jest na pokładzie. Odznaczone pozycje trafią do listy „do uzupełnienia”.</DialogDescription>
          </DialogHeader>

          {items.length === 0 ? (
            <p className="text-sm text-muted-foreground">Lista jest pusta. Dodaj pozycje poniżej.</p>
          ) : (
            <ul className="grid max-h-[45svh] gap-2 overflow-y-auto pr-1">
              {items.map((item, index) => (
                <li key={index} className={cn("grid grid-cols-[auto_1fr_auto] items-center gap-2 rounded-md border p-2", !item.isOk && "border-late/40 bg-late-soft/40")}>
                  <label className="flex items-center gap-1.5 text-xs font-semibold" title={item.isOk ? "Jest na pokładzie" : "Do uzupełnienia"}>
                    <input type="checkbox" className="size-4" checked={item.isOk} onChange={(e) => update(index, { isOk: e.target.checked })} aria-label={`${item.name}: jest na pokładzie`} />
                    <span className={item.isOk ? "text-paid" : "text-late"}>{item.isOk ? "Jest" : "Brak"}</span>
                  </label>
                  <div className="grid min-w-0 gap-1 sm:grid-cols-2">
                    <Input aria-label="Nazwa pozycji" required maxLength={100} value={item.name} onChange={(e) => update(index, { name: e.target.value })} />
                    <Input aria-label={`Uwagi: ${item.name}`} maxLength={200} placeholder={item.isOk ? "Uwagi" : "Co uzupełnić?"} value={item.note ?? ""} onChange={(e) => update(index, { note: e.target.value || null })} />
                  </div>
                  <NautivoIconButton action="delete" size="sm" label={item.name} onClick={() => setItems((list) => list.filter((_, i) => i !== index))} />
                </li>
              ))}
            </ul>
          )}

          <div className="flex gap-2">
            <Input
              aria-label="Nowa pozycja"
              maxLength={100}
              placeholder="np. Koło ratunkowe"
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  e.preventDefault();
                  add(newName);
                  setNewName("");
                }
              }}
            />
            <NautivoButton
              type="button"
              variant="secondary"
              icon={Plus}
              disabled={!newName.trim()}
              onClick={() => {
                add(newName);
                setNewName("");
              }}
            >
              Dodaj
            </NautivoButton>
          </div>
          {suggestions.length > 0 && (
            <div className="flex flex-wrap gap-1.5" aria-label="Szybkie dodawanie">
              {suggestions.map((s) => (
                <button key={s} type="button" onClick={() => add(s)} className="rounded-full border px-2.5 py-1 text-xs text-muted-foreground transition-colors hover:bg-muted">
                  + {s}
                </button>
              ))}
            </div>
          )}
          {items.length > 0 && <p className="text-sm text-muted-foreground">{missing > 0 ? `Do uzupełnienia: ${missing} z ${items.length}` : `Kompletne: ${items.length} pozycji`}</p>}

          <FormError message={error} />
          <DialogFooter>
            <NautivoButton type="button" variant="secondary" onClick={onClose}>
              Anuluj
            </NautivoButton>
            <NautivoButton type="submit" loading={busy}>
              Zapisz wyposażenie
            </NautivoButton>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
