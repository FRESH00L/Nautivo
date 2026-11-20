import { useQuery } from "@tanstack/react-query";
import { ArrowRight, Check, Loader2, MapPinned, Pencil, Plus, Search, Trash2 } from "lucide-react";
import { useState, type ReactNode } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { api, type CruiseCost, type CruiseDetail, type CruiseItem, type Enrollment, type ParticipantItem, type PaymentState, type RoutePoint, type Vessel } from "@/api";
import { useSession } from "@/auth";
import { NautivoBackButton, NautivoButton, NautivoIconButton, NautivoRowActions, NautivoTable, type NautivoColumn } from "@/components/nautivo";
import { geocode, RouteMap } from "@/components/RouteMap";
import { addDays, ConfirmDialog, EmptyState, Field, FormError, fullDay, LoadState, PageHead, PaidBar, PaymentDialog, plural, range, selectClass, STATE, StatusPill, today, useAction, useFormSubmit, zl } from "@/components/shared";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { WeatherForecast } from "@/components/WeatherForecast";
import { cn } from "@/lib/utils";

const COST_CATEGORIES = ["Czarter jachtu", "Skipper", "Paliwo", "Opłaty portowe", "Wyżywienie", "Transport", "Ubezpieczenie", "Sprzątanie i kaucja", "Inne"];

const yachtLabel = (c: Pick<CruiseItem, "yachtName" | "yachtModel" | "yachtLengthMeters">) =>
  [c.yachtName, c.yachtModel, c.yachtLengthMeters ? `${c.yachtLengthMeters.toLocaleString("pl-PL")} m` : null].filter(Boolean).join(", ");

const signed = (n: number) => (n > 0 ? `+${zl(n)}` : zl(n));

/* ------------------------------------------------------------------ Lista */

export function CruisesPage() {
  const { data, error } = useQuery({ queryKey: ["cruises"], queryFn: () => api<CruiseItem[]>("/cruises") });
  const [editing, setEditing] = useState<string | "new" | null>(null);
  const [deleting, setDeleting] = useState<CruiseItem | null>(null);
  const [tab, setTab] = useState<"upcoming" | "past">("upcoming");
  const now = today();
  const shown = (data ?? [])
    .filter((c) => (tab === "upcoming" ? c.endDate >= now : c.endDate < now))
    .sort((a, b) => (tab === "upcoming" ? a.startDate.localeCompare(b.startDate) : b.startDate.localeCompare(a.startDate)));
  const addButton = (
    <NautivoButton icon={Plus} onClick={() => setEditing("new")}>
      Dodaj rejs
    </NautivoButton>
  );

  return (
    <>
      <PageHead title="Rejsy" actions={addButton} />
      {!data && <LoadState error={error} />}
      {data?.length === 0 && <EmptyState title="Nie ma jeszcze rejsów" text="Dodaj pierwszy rejs, a potem wpisz załogę i koszty." action={addButton} />}
      {data && data.length > 0 && (
        <div className="mb-4 inline-flex gap-1 rounded-xl bg-muted p-1" role="group" aria-label="Pokaż rejsy">
          {(
            [
              ["upcoming", "Nadchodzące"],
              ["past", "Zakończone"],
            ] as const
          ).map(([id, label]) => (
            <button key={id} type="button" aria-pressed={tab === id} onClick={() => setTab(id)} className={cn("rounded-lg px-4 py-1.5 text-sm font-medium transition-colors", tab === id ? "bg-white text-sea shadow-sm" : "text-muted-foreground hover:text-foreground")}>
              {label}
            </button>
          ))}
        </div>
      )}
      {data && data.length > 0 && shown.length === 0 && <EmptyState title={tab === "upcoming" ? "Nie ma nadchodzących rejsów" : "Nie ma zakończonych rejsów"} text={tab === "upcoming" ? "Dodaj rejs, aby zacząć zapisy." : "Tutaj pojawią się rejsy po ich zakończeniu."} />}
      {shown.length > 0 && (
        <ul className="card divide-y">
          {shown.map((c) => {
            const cancelled = c.status === "Cancelled";
            const result = c.expected - c.costs;
            return (
              <li key={c.id} className="flex items-center gap-2 pr-3 transition-colors hover:bg-muted/40">
                <Link to={`/rejsy/${c.id}`} className="grid min-w-0 flex-1 items-center gap-x-6 gap-y-2 p-4 md:grid-cols-[minmax(0,2.2fr)_minmax(0,1fr)_minmax(0,1fr)]">
                  <div className="min-w-0">
                    <h2 className="truncate text-base">
                      {c.name} {cancelled && <span className="text-sm font-medium text-late">odwołany</span>}
                    </h2>
                    <p className="mt-0.5 flex flex-wrap items-center gap-x-1.5 text-sm text-muted-foreground">
                      <span>{range(c.startDate, c.endDate)}</span>
                      <span aria-hidden>·</span>
                      <span className="inline-flex items-center gap-1">
                        {c.departurePort} <ArrowRight className="size-3.5" aria-label="do" /> {c.arrivalPort}
                      </span>
                      {yachtLabel(c) && (
                        <>
                          <span aria-hidden>·</span>
                          <span>{yachtLabel(c)}</span>
                        </>
                      )}
                    </p>
                  </div>
                  <div>
                    <div className="text-sm">
                      {c.confirmed} z {c.capacity} osób
                    </div>
                    <div className="mt-1 h-1.5 rounded-full bg-muted">
                      <div className="h-1.5 rounded-full bg-primary" style={{ width: `${Math.min(100, (c.confirmed / c.capacity) * 100)}%` }} />
                    </div>
                  </div>
                  <div className="text-sm md:text-right">
                    <div>
                      {zl(c.collected)} <span className="text-muted-foreground">z {zl(c.expected)}</span>
                    </div>
                    <div className={cn("text-xs", result >= 0 ? "text-paid" : "text-late")}>Wynik: {signed(result)}</div>
                    {c.overdue > 0 && <div className="text-xs text-late">Zaległe: {c.overdue}</div>}
                  </div>
                </Link>
                <NautivoIconButton action="edit" label={`rejs ${c.name}`} onClick={() => setEditing(c.id)} />
                <NautivoIconButton action="delete" label={`rejs ${c.name}`} onClick={() => setDeleting(c)} />
              </li>
            );
          })}
        </ul>
      )}
      {editing && <CruiseForm key={editing} cruiseId={editing === "new" ? undefined : editing} onClose={() => setEditing(null)} />}
      <DeleteCruiseDialog cruise={deleting} onClose={() => setDeleting(null)} />
    </>
  );
}

function DeleteCruiseDialog({ cruise, onClose, onDeleted }: { cruise: CruiseItem | null; onClose: () => void; onDeleted?: () => void }) {
  const remove = useAction((id: string) => api(`/cruises/${id}`, { method: "DELETE" }), "Usunięto rejs");
  const details = cruise && cruise.confirmed + cruise.collected + cruise.costs > 0;
  return (
    <ConfirmDialog
      open={!!cruise}
      title={`Usunąć rejs „${cruise?.name ?? ""}”?`}
      text={
        details
          ? `Razem z rejsem znikną: ${plural(cruise!.confirmed, "zapis załoganta", "zapisy załogantów", "zapisów załogantów")}, wpłaty (${zl(cruise!.collected)}) i koszty (${zl(cruise!.costs)}). Osoby zostaną w bazie uczestników. Tego nie da się cofnąć.`
          : "Rejs nie ma załogi, wpłat ani kosztów. Tego nie da się cofnąć."
      }
      confirmLabel="Usuń rejs"
      onConfirm={async () => {
        await remove.mutateAsync(cruise!.id);
        onDeleted?.();
      }}
      onClose={onClose}
    />
  );
}

/* ------------------------------------------------------------ Formularz */

type DraftPoint = RoutePoint & { source?: "geo" | "manual"; lookedUp?: string; searching?: boolean; missing?: boolean };

/** Dodawanie i edycja rejsu. Przy edycji najpierw wczytuje pełne dane (trasa, opis). */
export function CruiseForm({ cruiseId, preset, onClose }: { cruiseId?: string; preset?: { vesselId?: string; startDate?: string }; onClose: () => void }) {
  const { data, error } = useQuery({ queryKey: ["cruise", cruiseId], queryFn: () => api<CruiseDetail>(`/cruises/${cruiseId}`), enabled: !!cruiseId });
  if (cruiseId && !data) {
    return (
      <Dialog open onOpenChange={(o) => !o && onClose()}>
        <DialogContent>
          <DialogTitle>Edytuj rejs</DialogTitle>
          <LoadState error={error} />
        </DialogContent>
      </Dialog>
    );
  }
  return <CruiseFormBody detail={data} preset={preset} onClose={onClose} />;
}

function CruiseFormBody({ detail, preset, onClose }: { detail?: CruiseDetail; preset?: { vesselId?: string; startDate?: string }; onClose: () => void }) {
  const navigate = useNavigate();
  const { hasModule } = useSession();
  const { data: vessels } = useQuery({ queryKey: ["vessels"], queryFn: () => api<Vessel[]>("/vessels"), enabled: hasModule("flota") });
  const cruise = detail?.cruise;
  const start = preset?.startDate ?? addDays(today(), 60);
  const presetVessel = vessels?.find((v) => v.id === preset?.vesselId);

  const [form, setForm] = useState({
    name: cruise?.name ?? "",
    startDate: cruise?.startDate ?? start,
    endDate: cruise?.endDate ?? addDays(start, 7),
    capacity: String(cruise?.capacity ?? 8),
    price: String(cruise?.price ?? ""),
    vesselId: cruise?.vesselId ?? preset?.vesselId ?? "",
    yachtName: cruise?.yachtName ?? "",
    yachtModel: cruise?.yachtModel ?? "",
    yachtLength: cruise?.yachtLengthMeters ? String(cruise.yachtLengthMeters) : "",
    deposit: cruise?.deposit ? String(cruise.deposit) : "",
    balanceDueDate: cruise && cruise.balanceDueDate !== cruise.startDate ? cruise.balanceDueDate : "",
    status: cruise?.status ?? "Planned",
    description: detail?.description ?? "",
  });
  const [points, setPoints] = useState<DraftPoint[]>(
    detail?.routePoints.length ? detail.routePoints.map((p) => ({ ...p, source: p.lat != null ? "manual" : undefined, lookedUp: p.name })) : [{ name: "", lat: null, lng: null }, { name: "", lat: null, lng: null }],
  );
  const [placing, setPlacing] = useState<number | null>(null);
  const set = (key: keyof typeof form) => (e: { target: { value: string } }) => setForm((f) => ({ ...f, [key]: e.target.value }));

  // Jacht wybrany w kalendarzu: uzupełnij dane, gdy flota się wczyta.
  if (presetVessel && !cruise && !form.yachtName) {
    setForm((f) => ({ ...f, yachtName: presetVessel.name, yachtModel: presetVessel.model, yachtLength: presetVessel.lengthMeters ? String(presetVessel.lengthMeters) : "" }));
  }

  const update = (index: number, patch: Partial<DraftPoint>) => setPoints((list) => list.map((p, i) => (i === index ? { ...p, ...patch } : p)));

  const locate = async (index: number, name: string) => {
    const query = name.trim();
    if (!query) return;
    update(index, { searching: true, missing: false });
    const hit = await geocode(query);
    setPoints((list) =>
      list.map((p, i) => {
        if (i !== index || p.name.trim() !== query) return p;
        return hit ? { ...p, ...hit, source: "geo", lookedUp: query, searching: false, missing: false } : { ...p, lat: null, lng: null, source: undefined, lookedUp: query, searching: false, missing: true };
      }),
    );
  };

  // Szukamy po wyjściu z pola: bez współrzędnych albo po zmianie nazwy znalezionego portu. Punkt wskazany ręcznie zostaje.
  const onNameBlur = (index: number) => {
    const p = points[index];
    if (!p.name.trim() || p.name.trim() === p.lookedUp) return;
    if (p.lat == null || p.source === "geo") void locate(index, p.name);
  };

  const locateAll = async () => {
    for (const [i, p] of points.entries()) {
      if (p.lat == null && p.name.trim()) await locate(i, p.name);
    }
  };

  const onPick = (lat: number, lng: number) => {
    if (placing != null) {
      update(placing, { lat, lng, source: "manual", missing: false });
      setPlacing(null);
      return;
    }
    setPoints((list) => {
      const via = list.length - 1;
      const next = [...list];
      next.splice(via, 0, { name: `Punkt ${via}`, lat, lng, source: "manual", lookedUp: `Punkt ${via}` });
      return next;
    });
  };

  const addVia = () => setPoints((list) => [...list.slice(0, -1), { name: "", lat: null, lng: null }, list[list.length - 1]]);
  const removeVia = (index: number) => {
    setPoints((list) => list.filter((_, i) => i !== index));
    setPlacing(null);
  };

  const chooseVessel = (id: string) => {
    const v = vessels?.find((x) => x.id === id);
    setForm((f) => ({ ...f, vesselId: id, ...(v ? { yachtName: v.name, yachtModel: v.model, yachtLength: v.lengthMeters ? String(v.lengthMeters) : "" } : {}) }));
  };

  const save = useAction(
    () => {
      const body = {
        name: form.name,
        startDate: form.startDate,
        endDate: form.endDate,
        capacity: Number(form.capacity),
        price: Number(form.price || 0),
        routePoints: points.map((p) => ({ name: p.name, lat: p.lat, lng: p.lng })),
        vesselId: form.vesselId || null,
        yachtName: form.yachtName || null,
        yachtModel: form.yachtModel || null,
        yachtLengthMeters: form.yachtLength ? Number(form.yachtLength) : null,
        deposit: Number(form.deposit || 0),
        balanceDueDate: form.balanceDueDate || null,
        status: form.status,
        description: form.description || null,
      };
      return cruise ? api(`/cruises/${cruise.id}`, { method: "PUT", body }) : api<{ id: string }>("/cruises", { method: "POST", body });
    },
    cruise ? "Zapisano zmiany w rejsie" : `Dodano rejs: ${form.name}`,
  );
  const { submit, error, busy } = useFormSubmit(async () => {
    const result = (await save.mutateAsync(undefined)) as { id: string } | undefined;
    onClose();
    if (!cruise && result?.id) navigate(`/rejsy/${result.id}`);
  });

  const last = points.length - 1;
  const missingCoords = points.some((p) => p.name.trim() && p.lat == null);

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-h-[92svh] overflow-y-auto sm:max-w-2xl">
        <form className="grid gap-5" onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>{cruise ? "Edytuj rejs" : "Dodaj rejs"}</DialogTitle>
          </DialogHeader>

          <Field label="Nazwa rejsu">
            <Input autoFocus required maxLength={200} value={form.name} onChange={set("name")} placeholder="np. Chorwacja: Split, Hvar, Vis" />
          </Field>
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
            <Field label="Początek">
              <Input type="date" required value={form.startDate} onChange={set("startDate")} />
            </Field>
            <Field label="Koniec">
              <Input type="date" required min={form.startDate} value={form.endDate} onChange={set("endDate")} />
            </Field>
            <Field label="Liczba osób">
              <Input type="number" min={1} max={500} required value={form.capacity} onChange={set("capacity")} />
            </Field>
            <Field label="Cena za załoganta (zł)">
              <Input type="number" min={0} step={0.01} required value={form.price} onChange={set("price")} placeholder="np. 2750" />
            </Field>
          </div>

          <fieldset className="grid gap-3">
            <legend className="mb-2 text-sm font-semibold">Trasa</legend>
            <ol className="grid gap-2">
              {points.map((p, i) => {
                const edge = i === 0 || i === last;
                const label = i === 0 ? "Skąd" : i === last ? "Dokąd" : `Port pośredni ${i}`;
                return (
                  <li key={i} className="flex items-center gap-2">
                    <span className={cn("grid size-6 shrink-0 place-items-center rounded-full text-xs font-bold text-white", edge ? "bg-sea" : "bg-primary")} aria-hidden>
                      {i + 1}
                    </span>
                    <div className="relative flex-1">
                      <Input
                        aria-label={label}
                        placeholder={i === 0 ? "Skąd, np. Split" : i === last ? "Dokąd, np. Split" : "Port pośredni (opcjonalnie)"}
                        required={edge}
                        maxLength={100}
                        value={p.name}
                        onChange={(e) => update(i, { name: e.target.value, missing: false })}
                        onBlur={() => onNameBlur(i)}
                        className="pr-28"
                      />
                      <span className="pointer-events-none absolute top-1/2 right-3 -translate-y-1/2 text-xs">
                        {p.searching ? (
                          <span className="inline-flex items-center gap-1 text-muted-foreground">
                            <Loader2 className="size-3 animate-spin" aria-hidden /> szukam
                          </span>
                        ) : p.lat != null ? (
                          <span className="text-paid">{p.source === "manual" ? "wskazany" : "na mapie"}</span>
                        ) : p.missing ? (
                          <span className="text-late">nie znaleziono</span>
                        ) : null}
                      </span>
                    </div>
                    <NautivoIconButton action="locate" label={label} active={placing === i} onClick={() => setPlacing(placing === i ? null : i)} />
                    {edge ? <span className="size-9 shrink-0" aria-hidden /> : <NautivoIconButton action="close" label={label} onClick={() => removeVia(i)} />}
                  </li>
                );
              })}
            </ol>
            <div className="flex flex-wrap gap-2">
              <NautivoButton type="button" variant="secondary" size="sm" icon={Plus} onClick={addVia} disabled={points.length >= 20}>
                Dodaj port pośredni
              </NautivoButton>
              {missingCoords && (
                <NautivoButton type="button" variant="secondary" size="sm" icon={Search} onClick={() => void locateAll()}>
                  Znajdź porty na mapie
                </NautivoButton>
              )}
            </div>
            <RouteMap points={points} onPick={onPick} highlight={placing} className="h-64" />
            <p className="text-xs text-muted-foreground" aria-live="polite">
              {placing != null
                ? `Kliknij na mapie miejsce dla punktu ${placing + 1}.`
                : "Porty wyszukujemy na mapie po wpisaniu nazwy. Kliknięcie w mapę dodaje punkt trasy, a przycisk z pinezką pozwala wskazać położenie portu ręcznie."}
            </p>
          </fieldset>

          <fieldset className="grid gap-3">
            <legend className="mb-2 text-sm font-semibold">
              Jacht <span className="font-normal text-muted-foreground">(opcjonalnie)</span>
            </legend>
            {vessels && vessels.length > 0 && (
              <Field label="Z floty">
                <select className={selectClass} value={form.vesselId} onChange={(e) => chooseVessel(e.target.value)}>
                  <option value="">Spoza floty, wpiszę ręcznie</option>
                  {vessels.map((v) => (
                    <option key={v.id} value={v.id}>
                      {v.name} ({v.model}, {v.berths} miejsc)
                    </option>
                  ))}
                </select>
              </Field>
            )}
            <div className="grid grid-cols-2 gap-3 sm:grid-cols-[2fr_2fr_1fr]">
              <Field label="Nazwa">
                <Input maxLength={100} value={form.yachtName} onChange={set("yachtName")} placeholder="np. Zefir" />
              </Field>
              <Field label="Model">
                <Input maxLength={100} value={form.yachtModel} onChange={set("yachtModel")} placeholder="np. Bavaria 46" />
              </Field>
              <Field label="Długość (m)">
                <Input type="number" min={0} max={999} step={0.01} value={form.yachtLength} onChange={set("yachtLength")} />
              </Field>
            </div>
          </fieldset>

          <details className="group rounded-[10px] border p-3" open={!!(form.deposit || form.balanceDueDate || form.description)}>
            <summary className="cursor-pointer text-sm font-semibold">
              Zaliczka, termin dopłaty i opis <span className="font-normal text-muted-foreground">(opcjonalnie)</span>
            </summary>
            <div className="mt-3 grid gap-3">
              <div className="grid grid-cols-2 gap-3">
                <Field label="Zaliczka (zł)">
                  <Input type="number" min={0} step={0.01} value={form.deposit} onChange={set("deposit")} placeholder="0" />
                </Field>
                <Field label="Dopłata do" hint="Bez daty: dzień startu. Po terminie nieopłacona kwota jest zaległa.">
                  <Input type="date" max={form.startDate} value={form.balanceDueDate} onChange={set("balanceDueDate")} />
                </Field>
              </div>
              <Field label="Opis">
                <Textarea maxLength={4000} value={form.description} onChange={set("description")} />
              </Field>
            </div>
          </details>

          {cruise && (
            <Field label="Status">
              <select className={selectClass} value={form.status} onChange={set("status")}>
                <option value="Planned">Planowany</option>
                <option value="Cancelled">Odwołany</option>
              </select>
            </Field>
          )}

          <FormError message={error} />
          <DialogFooter>
            <NautivoButton type="button" variant="secondary" onClick={onClose}>
              Anuluj
            </NautivoButton>
            <NautivoButton type="submit" loading={busy}>
              {cruise ? "Zapisz zmiany" : "Dodaj rejs"}
            </NautivoButton>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

/* ------------------------------------------------------------- Załoga */

/**
 * Dodanie lub edycja osoby zapisanej na rejs albo szkolenie. `enrollPath` to adres zapisu,
 * `noun` to biernik rzeczownika w przyciskach („załoganta”, „kursanta”).
 */
export function EnrollmentDialog({ enrollPath, price, noun, member, onClose }: { enrollPath: string; price: number; noun: string; member?: Enrollment; onClose: () => void }) {
  const { hasModule } = useSession();
  const { data: people } = useQuery({ queryKey: ["participants"], queryFn: () => api<ParticipantItem[]>("/participants"), enabled: !member && hasModule("uczestnicy") });
  const [mode, setMode] = useState<"new" | "existing">("new");
  const [participantId, setParticipantId] = useState("");
  const [person, setPerson] = useState({ fullName: member?.fullName ?? "", email: member?.email ?? "", phone: member?.phone ?? "", total: String(member?.total ?? price) });
  const set = (key: keyof typeof person) => (e: { target: { value: string } }) => setPerson((p) => ({ ...p, [key]: e.target.value }));

  const save = useAction(
    () => {
      const total = Number(person.total || 0);
      if (member) return api(`/enrollments/${member.id}`, { method: "PUT", body: { fullName: person.fullName, email: person.email || null, phone: person.phone || null, total } });
      return api(enrollPath, {
        method: "POST",
        body: mode === "existing" ? { participantId, total } : { participant: { fullName: person.fullName, email: person.email || null, phone: person.phone || null }, total },
      });
    },
    member ? `Zapisano zmiany: ${person.fullName}` : `Dodano ${noun}`,
  );
  const { submit, error, busy } = useFormSubmit(async () => {
    await save.mutateAsync(undefined);
    onClose();
  });

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <form className="grid gap-4" onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>{member ? `Edytuj: ${member.fullName}` : `Dodaj ${noun}`}</DialogTitle>
            <DialogDescription>
              {member ? "Imię, nazwisko i kontakt zmienią się też w bazie klientów i przy innych zapisach tej osoby." : `Cena: ${zl(price)}. Kwotę możesz zmienić, np. przy rabacie.`}
            </DialogDescription>
          </DialogHeader>
          {!member && people && people.length > 0 && (
            <div className="flex gap-4 text-sm">
              <label className="flex items-center gap-1.5">
                <input type="radio" name="mode" checked={mode === "new"} onChange={() => setMode("new")} /> Nowa osoba
              </label>
              <label className="flex items-center gap-1.5">
                <input type="radio" name="mode" checked={mode === "existing"} onChange={() => setMode("existing")} /> Z bazy uczestników
              </label>
            </div>
          )}
          {mode === "existing" && !member ? (
            <Field label="Uczestnik">
              <select className={selectClass} required value={participantId} onChange={(e) => setParticipantId(e.target.value)}>
                <option value="">Wybierz osobę</option>
                {people?.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.fullName}
                  </option>
                ))}
              </select>
            </Field>
          ) : (
            <>
              <Field label="Imię i nazwisko">
                <Input autoFocus required maxLength={200} value={person.fullName} onChange={set("fullName")} />
              </Field>
              <div className="grid grid-cols-2 gap-3">
                <Field label="E-mail">
                  <Input type="email" maxLength={254} value={person.email} onChange={set("email")} />
                </Field>
                <Field label="Telefon">
                  <Input type="tel" maxLength={40} value={person.phone} onChange={set("phone")} />
                </Field>
              </div>
            </>
          )}
          <Field label="Do zapłaty (zł)">
            <Input type="number" min={0} step={0.01} required value={person.total} onChange={set("total")} />
          </Field>
          <FormError message={error} />
          <DialogFooter>
            <NautivoButton type="button" variant="secondary" onClick={onClose}>
              Anuluj
            </NautivoButton>
            <NautivoButton type="submit" loading={busy}>
              {member ? "Zapisz zmiany" : `Dodaj ${noun}`}
            </NautivoButton>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

/* ------------------------------------------------------------- Wykresy */

function Bar({ label, value, max, className, note }: { label: string; value: number; max: number; className: string; note?: string }) {
  const width = max > 0 ? Math.max(value > 0 ? 1.5 : 0, (value / max) * 100) : 0;
  return (
    <div className="grid gap-1">
      <div className="flex justify-between gap-2 text-sm">
        <span>{label}</span>
        <span className="font-semibold tabular-nums">
          {zl(value)}
          {note && <span className="ml-1 font-normal text-muted-foreground">{note}</span>}
        </span>
      </div>
      <div className="h-2.5 rounded-full bg-muted">
        <div className={cn("h-2.5 rounded-full", className)} style={{ width: `${width}%` }} />
      </div>
    </div>
  );
}

/** Dane do wykresu finansów: pasują i rejs, i szkolenie. */
export type FinanceStats = Pick<CruiseItem, "expected" | "capacity" | "confirmed" | "price" | "costs" | "collected">;

export function FinanceChart({ cruise, title = "Finanse rejsu" }: { cruise: FinanceStats; title?: string }) {
  const full = cruise.expected + Math.max(0, cruise.capacity - cruise.confirmed) * cruise.price;
  const max = Math.max(full, cruise.costs, 1);
  const planned = cruise.expected - cruise.costs;
  const breakEven = cruise.price > 0 ? Math.ceil(cruise.costs / cruise.price) : null;
  return (
    <section className="card grid content-start gap-4 p-5" aria-labelledby="finance">
      <h2 id="finance" className="text-lg">
        {title}
      </h2>
      <div className="grid gap-3">
        <Bar label="Przychód przy komplecie miejsc" value={full} max={max} className="bg-primary/30" />
        <Bar label="Przychód z obecnych zapisów" value={cruise.expected} max={max} className="bg-primary" />
        <Bar label="Wpłacono" value={cruise.collected} max={max} className="bg-paid" />
        <Bar label="Koszty" value={cruise.costs} max={max} className="bg-late" />
      </div>
      <dl className="grid grid-cols-2 gap-3 border-t pt-4 text-sm">
        <div>
          <dt className="text-muted-foreground">Wynik z obecnych zapisów</dt>
          <dd className={cn("text-xl font-bold", planned >= 0 ? "text-paid" : "text-late")}>{signed(planned)}</dd>
        </div>
        <div>
          <dt className="text-muted-foreground">Wynik na dziś (wpłaty minus koszty)</dt>
          <dd className={cn("text-xl font-bold", cruise.collected - cruise.costs >= 0 ? "text-paid" : "text-late")}>{signed(cruise.collected - cruise.costs)}</dd>
        </div>
        <div>
          <dt className="text-muted-foreground">Wynik przy komplecie</dt>
          <dd className="font-semibold">{signed(full - cruise.costs)}</dd>
        </div>
        <div>
          <dt className="text-muted-foreground">Próg opłacalności</dt>
          <dd className="font-semibold">
            {breakEven == null ? "brak ceny" : breakEven === 0 ? "brak kosztów" : breakEven > cruise.capacity ? `${breakEven} osób, więcej niż miejsc` : `${plural(breakEven, "osoba", "osoby", "osób")} (jest ${cruise.confirmed})`}
          </dd>
        </div>
      </dl>
    </section>
  );
}

const STATE_ORDER: PaymentState[] = ["Paid", "Overpaid", "Partial", "None", "Overdue"];

const RADIUS = 40;
const CIRCUMFERENCE = 2 * Math.PI * RADIUS;

/** Co pokazać przy osobie na liście pod wykresem. */
function stateDetail(e: Enrollment) {
  if (e.state === "Overpaid") return `nadpłata ${zl(e.paid - e.total)}`;
  if (e.state === "Paid") return `zapłacono ${zl(e.paid)}`;
  return `brakuje ${zl(e.remaining)}`;
}

/** Wykres kołowy stanów płatności. Kliknięcie koloru (na wykresie lub w legendzie) pokazuje osoby w tym stanie. */
export function PaymentStatusChart({ crew, onPay, title = "Płatności załogi", noun = "załogantów" }: { crew: Enrollment[]; onPay: (enrollmentId: string) => void; title?: string; noun?: string }) {
  const [selected, setSelected] = useState<PaymentState | null>(null);
  const groups = STATE_ORDER.map((state) => ({ state, people: crew.filter((e) => e.state === state) })).filter((x) => x.people.length > 0);
  const active = groups.find((g) => g.state === selected) ?? null;
  const toCollect = crew.reduce((sum, e) => sum + e.remaining, 0);
  const overpaid = crew.reduce((sum, e) => sum + Math.max(0, e.paid - e.total), 0);
  const toggle = (state: PaymentState) => setSelected((current) => (current === state ? null : state));

  // Odstęp między wycinkami tylko wtedy, gdy jest więcej niż jeden kolor.
  const gap = groups.length > 1 ? 1.2 : 0;
  let offset = 0;
  const slices = groups.map((g) => {
    const length = (g.people.length / Math.max(crew.length, 1)) * CIRCUMFERENCE;
    const slice = { ...g, length: Math.max(length - gap, 0.1), offset };
    offset += length;
    return slice;
  });

  return (
    <section className="card grid content-start gap-4 p-5" aria-labelledby="payments">
      <h2 id="payments" className="text-lg">
        {title}
      </h2>
      {crew.length === 0 ? (
        <p className="text-sm text-muted-foreground">Wykres pojawi się po dodaniu {noun}.</p>
      ) : (
        <>
          <div className="grid items-center gap-5 sm:grid-cols-[180px_1fr]">
            <div className="relative mx-auto size-[180px]">
              <svg viewBox="0 0 100 100" className="size-full -rotate-90" role="img" aria-label={groups.map((g) => `${STATE[g.state].label}: ${g.people.length}`).join(", ")}>
                <circle cx="50" cy="50" r={RADIUS} fill="none" className="stroke-muted" strokeWidth="14" />
                {slices.map((s) => (
                  <circle
                    key={s.state}
                    cx="50"
                    cy="50"
                    r={RADIUS}
                    fill="none"
                    strokeWidth={selected === s.state ? 18 : 14}
                    strokeDasharray={`${s.length} ${CIRCUMFERENCE - s.length}`}
                    strokeDashoffset={-s.offset}
                    className={cn("cursor-pointer transition-all duration-200", STATE[s.state].stroke, selected && selected !== s.state && "opacity-30")}
                    onClick={() => toggle(s.state)}
                  >
                    <title>
                      {STATE[s.state].label}: {s.people.length}
                    </title>
                  </circle>
                ))}
              </svg>
              <div className="pointer-events-none absolute inset-0 grid place-content-center text-center">
                <span className="text-3xl font-bold tabular-nums">{active ? active.people.length : crew.length}</span>
                <span className="text-xs text-muted-foreground">{active ? STATE[active.state].label.toLowerCase() : noun}</span>
              </div>
            </div>
            <ul className="grid gap-1 text-sm">
              {groups.map((g) => (
                <li key={g.state}>
                  <button
                    type="button"
                    aria-pressed={selected === g.state}
                    onClick={() => toggle(g.state)}
                    className={cn("flex w-full items-center justify-between gap-2 rounded-lg px-2 py-1.5 text-left transition-colors hover:bg-muted", selected === g.state && "bg-muted font-semibold")}
                  >
                    <span className="flex items-center gap-2">
                      <span className={cn("size-3 rounded-full", STATE[g.state].bar)} aria-hidden />
                      {STATE[g.state].label}
                    </span>
                    <span className="tabular-nums">{plural(g.people.length, "osoba", "osoby", "osób")}</span>
                  </button>
                </li>
              ))}
            </ul>
          </div>
          <div aria-live="polite">
            {active ? (
              <div className="rounded-lg border">
                <div className="flex items-center justify-between gap-2 border-b px-3 py-2 text-sm">
                  <span className="flex items-center gap-2 font-semibold">
                    <span className={cn("size-2.5 rounded-full", STATE[active.state].bar)} aria-hidden />
                    {STATE[active.state].label}
                  </span>
                  <NautivoIconButton action="close" size="sm" label="lista osób" onClick={() => setSelected(null)} />
                </div>
                <ul className="divide-y text-sm">
                  {active.people.map((e) => (
                    <li key={e.id} className="flex items-center justify-between gap-3 px-3 py-2">
                      <span className="min-w-0">
                        <span className="block truncate font-medium">{e.fullName}</span>
                        <span className="text-xs text-muted-foreground">
                          {zl(e.paid)} z {zl(e.total)}, {stateDetail(e)}
                        </span>
                      </span>
                      <NautivoButton size="sm" variant="secondary" onClick={() => onPay(e.id)}>
                        Wpłaty
                      </NautivoButton>
                    </li>
                  ))}
                </ul>
              </div>
            ) : (
              <p className="text-xs text-muted-foreground">Kliknij kolor na wykresie lub w legendzie, aby zobaczyć osoby w danym stanie.</p>
            )}
          </div>
          <dl className="grid grid-cols-2 gap-3 border-t pt-4 text-sm">
            <div>
              <dt className="text-muted-foreground">Do zebrania</dt>
              <dd className="text-xl font-bold">{zl(toCollect)}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Nadpłaty do zwrotu</dt>
              <dd className="text-xl font-bold">{zl(overpaid)}</dd>
            </div>
          </dl>
        </>
      )}
    </section>
  );
}

/* ------------------------------------------------------------- Koszty */

function CostEditor({ addPath, cost, onDone }: { addPath: string; cost?: CruiseCost; onDone: () => void }) {
  const [draft, setDraft] = useState({ category: cost?.category ?? "", description: cost?.description ?? "", amount: cost ? String(cost.amount) : "" });
  const save = useAction(() => {
    const body = { category: draft.category, description: draft.description || null, amount: Number(draft.amount) };
    return cost ? api(`/costs/${cost.id}`, { method: "PUT", body }) : api(addPath, { method: "POST", body });
  }, cost ? "Zapisano koszt" : "Dodano koszt");
  const { submit, error, busy } = useFormSubmit(async () => {
    await save.mutateAsync(undefined);
    if (cost) onDone();
    else setDraft({ category: "", description: "", amount: "" });
  });
  const formId = `cost-${cost?.id ?? "new"}`;
  return (
    <>
      <tr className={cost ? "bg-accent/40" : undefined}>
        <td className="p-2">
          <form id={formId} onSubmit={submit} />
          <Input form={formId} list="cost-categories" required maxLength={60} aria-label="Kategoria" placeholder="Kategoria" value={draft.category} onChange={(e) => setDraft((d) => ({ ...d, category: e.target.value }))} autoFocus={!!cost} />
        </td>
        <td className="p-2">
          <Input form={formId} maxLength={300} aria-label="Opis" placeholder="Opis (opcjonalnie)" value={draft.description} onChange={(e) => setDraft((d) => ({ ...d, description: e.target.value }))} />
        </td>
        <td className="p-2" colSpan={2}>
          <Input form={formId} type="number" min={0} step={0.01} required aria-label="Kwota w złotych" placeholder="Kwota (zł)" className="text-right" value={draft.amount} onChange={(e) => setDraft((d) => ({ ...d, amount: e.target.value }))} />
        </td>
        <td className="p-2">
          <div className="flex justify-end gap-1">
            {cost ? (
              <>
                <NautivoButton form={formId} type="submit" size="sm" icon={Check} loading={busy}>
                  Zapisz
                </NautivoButton>
                <NautivoIconButton action="close" label="edycja kosztu" onClick={onDone} />
              </>
            ) : (
              <NautivoButton form={formId} type="submit" icon={Plus} loading={busy}>
                Dodaj
              </NautivoButton>
            )}
          </div>
        </td>
      </tr>
      {error && (
        <tr>
          <td colSpan={5} className="px-2 pb-2">
            <FormError message={error} />
          </td>
        </tr>
      )}
    </>
  );
}

/** Tabela kosztów z dodawaniem i edycją w miejscu. `headcount` służy do kosztu na osobę. */
export function CostsTable({ addPath, headcount, costs, title = "Koszty rejsu", personLabel = "załoganta", categories = COST_CATEGORIES }: { addPath: string; headcount: number; costs: CruiseCost[]; title?: string; personLabel?: string; categories?: string[] }) {
  const [editing, setEditing] = useState<string | null>(null);
  const [deleting, setDeleting] = useState<CruiseCost | null>(null);
  const remove = useAction((id: string) => api(`/costs/${id}`, { method: "DELETE" }), "Usunięto koszt");
  const total = costs.reduce((sum, c) => sum + c.amount, 0);
  const perPerson = headcount > 0 ? total / headcount : null;
  const costColumns: NautivoColumn<CruiseCost>[] = [
    { key: "category", header: "Kategoria", cell: (c) => <span className="font-medium">{c.category}</span> },
    { key: "description", header: "Opis", cell: (c) => <span className="text-muted-foreground">{c.description}</span> },
    { key: "amount", header: "Kwota", align: "right", cell: (c) => <span className="tabular-nums">{zl(c.amount)}</span> },
    { key: "share", header: "Udział", align: "right", hideBelow: "sm", cell: (c) => <span className="text-muted-foreground tabular-nums">{total > 0 ? `${Math.round((c.amount / total) * 100)}%` : ""}</span> },
    {
      key: "actions",
      header: "Działania",
      srOnlyHeader: true,
      align: "right",
      cell: (c) => (
        <NautivoRowActions>
          <NautivoIconButton action="edit" label={`koszt ${c.category}`} onClick={() => setEditing(c.id)} />
          <NautivoIconButton action="delete" label={`koszt ${c.category}`} onClick={() => setDeleting(c)} />
        </NautivoRowActions>
      ),
    },
  ];
  return (
    <section className="mt-8" aria-labelledby="costs">
      <div className="mb-2 flex flex-wrap items-end justify-between gap-2">
        <h2 id="costs" className="text-xl">
          {title}
        </h2>
        {perPerson != null && total > 0 && <p className="text-sm text-muted-foreground">Koszt na {personLabel}: {zl(Math.round(perPerson * 100) / 100)}</p>}
      </div>
      <datalist id="cost-categories">
        {categories.map((c) => (
          <option key={c} value={c} />
        ))}
      </datalist>
      <NautivoTable
        columns={costColumns}
        rows={costs}
        rowKey={(c) => c.id}
        caption={title}
        renderRow={(c) => (editing === c.id ? <CostEditor addPath={addPath} cost={c} onDone={() => setEditing(null)} /> : undefined)}
        extraRows={<CostEditor addPath={addPath} onDone={() => undefined} />}
        footer={
          <tr>
            <td className="p-3" colSpan={2}>
              Suma kosztów
            </td>
            <td className="p-3 text-right tabular-nums">{zl(total)}</td>
            <td colSpan={2} />
          </tr>
        }
      />
      <ConfirmDialog
        open={!!deleting}
        title={`Usunąć koszt „${deleting?.category ?? ""}”?`}
        text={`Kwota ${zl(deleting?.amount ?? 0)} zniknie z rozliczenia rejsu.`}
        confirmLabel="Usuń koszt"
        onConfirm={async () => {
          await remove.mutateAsync(deleting!.id);
        }}
        onClose={() => setDeleting(null)}
      />
    </section>
  );
}

/* ----------------------------------------------------------- Szczegóły */

function Info({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div>
      <dt className="text-sm text-muted-foreground">{label}</dt>
      <dd className="mt-0.5 font-semibold">{children}</dd>
    </div>
  );
}

export function CruiseDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data, error } = useQuery({ queryKey: ["cruise", id], queryFn: () => api<CruiseDetail>(`/cruises/${id}`) });
  const [editing, setEditing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [crewDialog, setCrewDialog] = useState<Enrollment | "new" | null>(null);
  const [paying, setPaying] = useState<string | null>(null);
  const [removing, setRemoving] = useState<Enrollment | null>(null);
  const removeMember = useAction((enrollmentId: string) => api(`/enrollments/${enrollmentId}`, { method: "DELETE" }), "Usunięto załoganta z rejsu");

  if (!data) {
    return error ? (
      <EmptyState title="Nie można otworzyć rejsu" text={error.message} action={<NautivoButton variant="secondary" onClick={() => navigate("/rejsy")}>Wróć do rejsów</NautivoButton>} />
    ) : (
      <LoadState />
    );
  }

  const { cruise, enrollments: crew, routePoints, costs } = data;
  const isCancelled = cruise.status === "Cancelled";
  const full = crew.length >= cruise.capacity;
  const paymentTarget = crew.find((e) => e.id === paying);
  const located = routePoints.filter((p) => p.lat != null);
  const crewColumns: NautivoColumn<Enrollment>[] = [
    {
      key: "name",
      header: "Załogant",
      cell: (e) => (
        <>
          <Link to={`/uczestnicy/${e.participantId}`} className="font-medium hover:underline">
            {e.fullName}
          </Link>
          <div className="text-xs text-muted-foreground">{[e.email, e.phone].filter(Boolean).join(", ")}</div>
        </>
      ),
    },
    {
      key: "state",
      header: "Status płatności",
      cell: (e) => (
        <>
          <StatusPill state={e.state} />
          {e.state === "Overpaid" && <div className="mt-1 text-xs text-primary">+{zl(e.paid - e.total)}</div>}
        </>
      ),
    },
    { key: "paid", header: "Wpłacono", cell: (e) => <PaidBar paid={e.paid} total={e.total} /> },
    {
      key: "remaining",
      header: "Do zapłaty",
      align: "right",
      cell: (e) => (
        <span className="tabular-nums">
          {zl(e.remaining)}
          {e.total !== cruise.price && <span className="block text-xs text-muted-foreground">cena indywidualna: {zl(e.total)}</span>}
        </span>
      ),
    },
    {
      key: "actions",
      header: "Działania",
      srOnlyHeader: true,
      align: "right",
      cell: (e) => (
        <NautivoRowActions>
          <NautivoButton size="sm" variant="secondary" onClick={() => setPaying(e.id)}>
            Wpłaty
          </NautivoButton>
          <NautivoIconButton action="edit" label={e.fullName} onClick={() => setCrewDialog(e)} />
          <NautivoIconButton action="delete" label={`${e.fullName} z rejsu`} onClick={() => setRemoving(e)} />
        </NautivoRowActions>
      ),
    },
  ];

  return (
    <>
      <PageHead
        back={<NautivoBackButton fallback="/rejsy" label="Wróć do rejsów" />}
        title={cruise.name}
        subtitle={`${range(cruise.startDate, cruise.endDate)}, ${cruise.departurePort} → ${cruise.arrivalPort}${isCancelled ? ", rejs odwołany" : ""}`}
        actions={
          <>
            <NautivoButton variant="secondary" icon={Pencil} onClick={() => setEditing(true)}>
              Edytuj
            </NautivoButton>
            <NautivoButton variant="danger" icon={Trash2} onClick={() => setDeleting(true)}>
              Usuń
            </NautivoButton>
          </>
        }
      />

      <dl className="card mb-6 grid grid-cols-2 gap-x-8 gap-y-4 p-5 md:grid-cols-3 lg:grid-cols-6">
        <Info label="Termin">{range(cruise.startDate, cruise.endDate)}</Info>
        <Info label="Załoga">
          {cruise.confirmed} z {cruise.capacity} osób
        </Info>
        <Info label="Cena za załoganta">{zl(cruise.price)}</Info>
        <Info label="Zaliczka">{cruise.deposit > 0 ? zl(cruise.deposit) : "brak"}</Info>
        <Info label="Dopłata do">{fullDay(cruise.balanceDueDate)}</Info>
        <Info label="Jacht">{yachtLabel(cruise) || <span className="font-normal text-muted-foreground">nie podano</span>}</Info>
      </dl>

      {data.description && <p className="mb-6 max-w-3xl whitespace-pre-wrap">{data.description}</p>}

      <div className="mb-8 grid gap-6 lg:grid-cols-2">
        <FinanceChart cruise={cruise} />
        <PaymentStatusChart crew={crew} onPay={setPaying} />
      </div>

      <section className="mb-8" aria-labelledby="crew">
        <div className="mb-2 flex flex-wrap items-end justify-between gap-2">
          <h2 id="crew" className="text-xl">
            Załoga
          </h2>
          {!isCancelled && (
            <div className="flex items-center gap-3">
              {full && <span className="text-sm text-muted-foreground">Komplet załogi</span>}
              <NautivoButton icon={Plus} onClick={() => setCrewDialog("new")} disabled={full}>
                Dodaj załoganta
              </NautivoButton>
            </div>
          )}
        </div>
        {crew.length === 0 ? (
          <EmptyState title="Nikt się jeszcze nie zapisał" text="Dodaj pierwszego załoganta, aby śledzić jego wpłaty." />
        ) : (
          <NautivoTable columns={crewColumns} rows={crew} rowKey={(e) => e.id} caption="Załoga rejsu" minWidth={760} />
        )}
      </section>

      <CostsTable addPath={`/cruises/${cruise.id}/costs`} headcount={cruise.confirmed} costs={costs} />

      <section className="mt-8" aria-labelledby="route">
        <h2 id="route" className="mb-2 flex items-center gap-2 text-xl">
          <MapPinned className="size-5 text-primary" aria-hidden /> Trasa
        </h2>
        <ol className="mb-3 flex flex-wrap items-center gap-x-2 gap-y-1 text-sm">
          {routePoints.map((p, i) => (
            <li key={i} className="flex items-center gap-2">
              {i > 0 && <ArrowRight className="size-3.5 text-muted-foreground" aria-hidden />}
              <span className={cn(p.lat == null && "text-muted-foreground")}>{p.name}</span>
            </li>
          ))}
        </ol>
        {located.length > 0 ? (
          <RouteMap points={routePoints} className="h-80" />
        ) : (
          <EmptyState title="Trasa nie ma jeszcze punktów na mapie" text="Edytuj rejs, aby wyszukać porty albo wskazać je na mapie." />
        )}
        <WeatherForecast points={routePoints} startDate={cruise.startDate} endDate={cruise.endDate} />
      </section>

      {editing && <CruiseForm cruiseId={cruise.id} onClose={() => setEditing(false)} />}
      {crewDialog && <EnrollmentDialog enrollPath={`/cruises/${cruise.id}/enrollments`} price={cruise.price} noun="załoganta" member={crewDialog === "new" ? undefined : crewDialog} onClose={() => setCrewDialog(null)} />}
      {paymentTarget && <PaymentDialog key={paymentTarget.id} enrollment={paymentTarget} onClose={() => setPaying(null)} />}
      <ConfirmDialog
        open={!!removing}
        title={`Usunąć z rejsu: ${removing?.fullName ?? ""}?`}
        text={removing && removing.paid > 0 ? `Razem z zapisem znikną wpłaty tej osoby na ten rejs (${zl(removing.paid)}). Osoba zostanie w bazie uczestników.` : "Miejsce wróci do puli. Osoba zostanie w bazie uczestników."}
        confirmLabel="Usuń z rejsu"
        onConfirm={async () => {
          await removeMember.mutateAsync(removing!.id);
        }}
        onClose={() => setRemoving(null)}
      />
      <DeleteCruiseDialog cruise={deleting ? cruise : null} onClose={() => setDeleting(false)} onDeleted={() => navigate("/rejsy")} />
    </>
  );
}
