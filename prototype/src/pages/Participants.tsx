import { useQuery } from "@tanstack/react-query";
import { Award, CalendarClock, Download, FileDown, GraduationCap, Mail, Phone, Plus, Sailboat, ShieldCheck, StickyNote, Trash2, UserX, Users, Wallet, type LucideIcon } from "lucide-react";
import { useState, type ReactNode } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { api, type Activity, type ActivityType, type ContactStage, type CourseType, type ContactTask, type ParticipantDetail, type ParticipantHistory, type ParticipantItem } from "@/api";
import { NautivoBackButton, NautivoButton, NautivoIconButton, NautivoTable, type NautivoColumn } from "@/components/nautivo";
import { addDays, ConfirmDialog, EmptyState, Field, FormError, fullDay, LoadState, PageHead, PaidBar, plural, range, selectClass, StatusPill, today, useAction, useFormSubmit, zl } from "@/components/shared";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { cn } from "@/lib/utils";
import { COURSE_TYPE, COURSE_TYPES, CourseBadge } from "@/pages/Courses";

/* ------------------------------------------------------------ Słowniki */

const STAGE: Record<ContactStage, { label: string; plural: string; hint: string; className: string }> = {
  Lead: { label: "Potencjalny", plural: "Potencjalni", hint: "Kontakt bez rejsu", className: "bg-muted text-muted-foreground" },
  Customer: { label: "Klient", plural: "Klienci", hint: "Jeden rejs", className: "bg-accent text-primary" },
  Regular: { label: "Stały klient", plural: "Stali klienci", hint: "Co najmniej dwa rejsy", className: "bg-paid-soft text-paid" },
  Lapsed: { label: "Do odzyskania", plural: "Do odzyskania", hint: "Ostatni rejs ponad rok temu", className: "bg-part-soft text-part" },
};
const STAGES: ContactStage[] = ["Lead", "Customer", "Regular", "Lapsed"];

const ACTIVITY: Record<ActivityType, { label: string; icon: LucideIcon }> = {
  Note: { label: "Notatka", icon: StickyNote },
  Call: { label: "Telefon", icon: Phone },
  Email: { label: "E-mail", icon: Mail },
  Meeting: { label: "Spotkanie", icon: Users },
};

/**
 * Propozycja ścieżki szkoleń: kto jest naturalnym kandydatem na dane szkolenie.
 * Wymagania formalne (staż, wiek) sprawdza organizator; to tylko podpowiedź do kampanii.
 */
const CANDIDATES: Partial<Record<CourseType, { requires?: CourseType; hint: string }>> = {
  Zj: { hint: "osoby bez patentu ŻJ" },
  Jsm: { requires: "Zj", hint: "osoby z ŻJ, bez JSM" },
  Src: { hint: "osoby bez SRC" },
  Stcw: { hint: "osoby bez STCW" },
  Smw: { hint: "osoby bez patentu motorowodnego" },
};

function isCandidate(qualifications: CourseType[], target: CourseType) {
  const rule = CANDIDATES[target];
  return !!rule && !qualifications.includes(target) && (!rule.requires || qualifications.includes(rule.requires));
}

function Qualifications({ types }: { types: CourseType[] }) {
  if (types.length === 0) return null;
  return (
    <span className="flex flex-wrap gap-1">
      {types.map((t) => (
        <CourseBadge key={t} type={t} />
      ))}
    </span>
  );
}

const SOURCES = ["Polecenie", "Instagram", "Facebook", "Strona www", "Google", "Targi żeglarskie", "Wraca do nas", "Inne"];

const dateTime = new Intl.DateTimeFormat("pl-PL", { day: "numeric", month: "short", year: "numeric", hour: "2-digit", minute: "2-digit" });
const when = (iso: string) => dateTime.format(new Date(iso));
const dateOf = (iso: string) => fullDay(iso.slice(0, 10));

function StageBadge({ stage }: { stage: ContactStage }) {
  return <span className={cn("inline-block rounded-full px-2.5 py-0.5 text-xs font-semibold whitespace-nowrap", STAGE[stage].className)}>{STAGE[stage].label}</span>;
}

function Tags({ tags }: { tags: string[] }) {
  if (tags.length === 0) return null;
  return (
    <span className="flex flex-wrap gap-1">
      {tags.map((t) => (
        <span key={t} className="rounded-md bg-secondary px-1.5 py-0.5 text-xs">
          {t}
        </span>
      ))}
    </span>
  );
}

function dueTone(due: string) {
  const now = today();
  return due < now ? "text-late font-semibold" : due === now ? "text-part font-semibold" : "text-muted-foreground";
}

function dueLabel(due: string) {
  const now = today();
  if (due < now) return `po terminie (${fullDay(due)})`;
  if (due === now) return "dziś";
  if (due === addDays(now, 1)) return "jutro";
  return fullDay(due);
}

function download(name: string, content: string, type: string) {
  const url = URL.createObjectURL(new Blob([content], { type }));
  const link = document.createElement("a");
  link.href = url;
  link.download = name;
  link.click();
  URL.revokeObjectURL(url);
}

/** CSV z separatorem średnika i BOM, żeby polski Excel od razu poprawnie otworzył plik. */
function toCsv(rows: ParticipantItem[]) {
  const cell = (v: string | number | null) => `"${String(v ?? "").replace(/"/g, '""')}"`;
  const header = ["Imię i nazwisko", "E-mail", "Telefon", "Etap", "Tagi", "Uprawnienia", "Źródło", "Zgoda marketingowa", "Rejsy i szkolenia", "Wpłacono", "Do zapłaty"];
  const lines = rows.map((r) => [r.fullName, r.email, r.phone, STAGE[r.stage].label, r.tags.join(", "), r.qualifications.map((q) => COURSE_TYPE[q].short).join(", "), r.source, r.marketingConsent ? "tak" : "nie", r.cruises, r.paid, r.remaining].map(cell).join(";"));
  return "﻿" + [header.map(cell).join(";"), ...lines].join("\r\n");
}

/* --------------------------------------------------------------- Lista */

export function ParticipantsPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState("");
  const [stage, setStage] = useState<ContactStage | null>(null);
  const [tag, setTag] = useState("");
  const [consentOnly, setConsentOnly] = useState(false);
  const [debtOnly, setDebtOnly] = useState(false);
  const [candidateFor, setCandidateFor] = useState<CourseType | "">("");
  const [creating, setCreating] = useState(false);
  const { data, error } = useQuery({
    queryKey: ["participants", search],
    queryFn: () => api<ParticipantItem[]>(`/participants?search=${encodeURIComponent(search)}`),
    placeholderData: (previous) => previous,
  });

  const people = (data ?? []).filter((p) => !p.anonymized);
  const allTags = [...new Set(people.flatMap((p) => p.tags))].sort((a, b) => a.localeCompare(b, "pl"));
  const shown = people.filter((p) => (!stage || p.stage === stage) && (!tag || p.tags.includes(tag)) && (!consentOnly || p.marketingConsent) && (!debtOnly || p.remaining > 0) && (!candidateFor || isCandidate(p.qualifications, candidateFor)));
  const filtered = !!(stage || tag || consentOnly || debtOnly || search || candidateFor);

  const columns: NautivoColumn<ParticipantItem>[] = [
    {
      key: "name",
      header: "Klient",
      cell: (p) => (
        <>
          <Link to={`/uczestnicy/${p.id}`} className="font-medium hover:underline">
            {p.fullName}
          </Link>
          <div className="text-xs text-muted-foreground">{[p.email, p.phone].filter(Boolean).join(", ") || "Bez kontaktu"}</div>
        </>
      ),
    },
    { key: "stage", header: "Etap", cell: (p) => <StageBadge stage={p.stage} /> },
    { key: "tags", header: "Tagi", hideBelow: "md", cell: (p) => <Tags tags={p.tags} /> },
    { key: "qualifications", header: "Uprawnienia", hideBelow: "md", cell: (p) => <Qualifications types={p.qualifications} /> },
    {
      key: "cruises",
      header: "Zapisy",
      cell: (p) => (
        <>
          <div>{p.cruises}</div>
          <div className="text-xs text-muted-foreground">{p.nextCruiseStart ? `następny ${fullDay(p.nextCruiseStart)}` : p.lastCruiseEnd ? `ostatni ${fullDay(p.lastCruiseEnd)}` : ""}</div>
        </>
      ),
    },
    { key: "paid", header: "Wpłacono", align: "right", cell: (p) => <span className="tabular-nums">{zl(p.paid)}</span> },
    { key: "remaining", header: "Do zapłaty", align: "right", cell: (p) => <span className={cn("tabular-nums", p.remaining > 0 ? "font-semibold" : "text-muted-foreground")}>{zl(p.remaining)}</span> },
    {
      key: "task",
      header: "Zadanie",
      hideBelow: "sm",
      cell: (p) => (p.nextTaskDue ? <span className={cn("text-xs", dueTone(p.nextTaskDue))}>{dueLabel(p.nextTaskDue)}</span> : <span className="text-xs text-muted-foreground">brak</span>),
    },
  ];

  const addButton = (
    <NautivoButton icon={Plus} onClick={() => setCreating(true)}>
      Dodaj klienta
    </NautivoButton>
  );

  return (
    <>
      <PageHead
        title="Klienci"
        subtitle="Kontakty, historia rejsów, notatki i to, co jest do zrobienia."
        actions={
          <>
            <NautivoButton variant="secondary" icon={Download} disabled={shown.length === 0} title="Plik zawiera dane osobowe. Przechowuj go bezpiecznie." onClick={() => download(`klienci-${today()}.csv`, toCsv(shown), "text/csv;charset=utf-8")}>
              Eksport CSV
            </NautivoButton>
            {addButton}
          </>
        }
      />

      <div className="mb-5 grid grid-cols-2 gap-3 lg:grid-cols-4" role="group" aria-label="Filtruj według etapu">
        {STAGES.map((s) => {
          const count = people.filter((p) => p.stage === s).length;
          return (
            <button
              key={s}
              type="button"
              aria-pressed={stage === s}
              onClick={() => setStage(stage === s ? null : s)}
              className={cn("card p-4 text-left transition-shadow hover:shadow-md", stage === s && "ring-2 ring-primary")}
            >
              <span className={cn("inline-block rounded-full px-2 py-0.5 text-xs font-semibold", STAGE[s].className)}>{STAGE[s].plural}</span>
              <span className="mt-2 block text-2xl font-bold tabular-nums">{count}</span>
              <span className="text-xs text-muted-foreground">{STAGE[s].hint}</span>
            </button>
          );
        })}
      </div>

      <TasksPanel />

      <div className="mb-3 flex flex-wrap items-center gap-3">
        <Input className="max-w-sm" aria-label="Szukaj klienta" placeholder="Szukaj po nazwisku, e-mailu lub telefonie" value={search} onChange={(e) => setSearch(e.target.value)} />
        {allTags.length > 0 && (
          <select className={cn(selectClass, "w-auto")} aria-label="Tag" value={tag} onChange={(e) => setTag(e.target.value)}>
            <option value="">Wszystkie tagi</option>
            {allTags.map((t) => (
              <option key={t}>{t}</option>
            ))}
          </select>
        )}
        <select className={cn(selectClass, "w-auto")} aria-label="Kandydaci na szkolenie" value={candidateFor} onChange={(e) => setCandidateFor(e.target.value as CourseType | "")}>
          <option value="">Kandydaci na szkolenie…</option>
          {COURSE_TYPES.filter((t) => CANDIDATES[t]).map((t) => (
            <option key={t} value={t}>
              {COURSE_TYPE[t].short}: {CANDIDATES[t]!.hint}
            </option>
          ))}
        </select>
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" checked={consentOnly} onChange={(e) => setConsentOnly(e.target.checked)} /> Ze zgodą marketingową
        </label>
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" checked={debtOnly} onChange={(e) => setDebtOnly(e.target.checked)} /> Z niedopłatą
        </label>
        {filtered && <span className="text-sm text-muted-foreground">{plural(shown.length, "osoba", "osoby", "osób")}</span>}
      </div>
      {candidateFor && (
        <p className="mb-3 rounded-lg bg-accent p-3 text-sm">
          Kandydaci na {COURSE_TYPE[candidateFor].label}: {plural(shown.length, "osoba", "osoby", "osób")}, w tym {shown.filter((p) => p.marketingConsent).length} ze zgodą marketingową. Ofertę wysyłaj tylko osobom ze zgodą. Wymagania formalne (np. staż) sprawdź sam.
        </p>
      )}

      {!data && <LoadState error={error} />}
      {data && people.length === 0 && !search && <EmptyState title="Nie ma jeszcze klientów" text="Dodaj osobę tutaj albo zapisz ją bezpośrednio na rejs." action={addButton} />}
      {data && (people.length > 0 || search) && shown.length === 0 && <EmptyState title="Brak wyników" text="Zmień filtry albo wyczyść pole wyszukiwania." />}
      {shown.length > 0 && <NautivoTable columns={columns} rows={shown} rowKey={(p) => p.id} caption="Lista klientów" minWidth={820} rowClassName={() => "hover:bg-muted/40"} />}

      {creating && <ContactForm tagSuggestions={allTags} onClose={() => setCreating(false)} onSaved={(id) => id && navigate(`/uczestnicy/${id}`)} />}
    </>
  );
}

/** Otwarte zadania całej firmy: po terminie, na dziś i najbliższe. */
function TasksPanel() {
  const { data } = useQuery({ queryKey: ["tasks"], queryFn: () => api<ContactTask[]>("/tasks") });
  const [all, setAll] = useState(false);
  const done = useAction((id: string) => api(`/tasks/${id}/done`, { method: "PUT", body: { done: true } }), "Zadanie wykonane");
  if (!data || data.length === 0) return null;
  const urgent = data.filter((t) => t.dueDate <= today()).length;
  const shown = all ? data : data.slice(0, 5);
  return (
    <section className="card mb-6 p-4" aria-labelledby="tasks">
      <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
        <h2 id="tasks" className="flex items-center gap-2 text-lg">
          <CalendarClock className="size-5 text-primary" aria-hidden /> Do zrobienia
        </h2>
        <span className="text-sm text-muted-foreground">
          {plural(data.length, "zadanie", "zadania", "zadań")}
          {urgent > 0 && <span className="font-semibold text-late">, {urgent} na dziś lub po terminie</span>}
        </span>
      </div>
      <ul className="divide-y text-sm">
        {shown.map((t) => (
          <li key={t.id} className="flex items-center gap-3 py-2">
            <input type="checkbox" className="size-4" aria-label={`Oznacz jako wykonane: ${t.title}`} disabled={done.isPending} onChange={() => done.mutate(t.id)} />
            <span className="min-w-0 flex-1">
              <span className="font-medium">{t.title}</span>
              <span className="text-muted-foreground">
                {" · "}
                <Link to={`/uczestnicy/${t.participantId}`} className="hover:underline">
                  {t.participantName}
                </Link>
              </span>
            </span>
            <span className={cn("text-xs whitespace-nowrap", dueTone(t.dueDate))}>{dueLabel(t.dueDate)}</span>
          </li>
        ))}
      </ul>
      {data.length > 5 && (
        <NautivoButton variant="ghost" size="sm" className="mt-1" onClick={() => setAll(!all)}>
          {all ? "Pokaż mniej" : `Pokaż wszystkie (${data.length})`}
        </NautivoButton>
      )}
    </section>
  );
}

/* ----------------------------------------------------------- Formularz */

function TagInput({ value, onChange, suggestions }: { value: string[]; onChange: (tags: string[]) => void; suggestions: string[] }) {
  const [draft, setDraft] = useState("");
  const add = (raw: string) => {
    const tag = raw.trim().replace(/,$/, "");
    if (tag && tag.length <= 30 && !value.some((t) => t.toLowerCase() === tag.toLowerCase()) && value.length < 10) onChange([...value, tag]);
    setDraft("");
  };
  return (
    <div className="flex min-h-10 flex-wrap items-center gap-1.5 rounded-[10px] border border-input bg-card px-2 py-1.5">
      {value.map((t) => (
        <span key={t} className="flex items-center gap-1 rounded-md bg-secondary py-0.5 pr-0.5 pl-1.5 text-xs">
          {t}
          <NautivoIconButton action="close" size="sm" label={`tag ${t}`} onClick={() => onChange(value.filter((x) => x !== t))} />
        </span>
      ))}
      <input
        list="tag-suggestions"
        aria-label="Dodaj tag"
        placeholder={value.length ? "" : "np. rodzina, firma, VIP (Enter dodaje)"}
        className="min-w-24 flex-1 bg-transparent text-sm outline-none"
        maxLength={30}
        value={draft}
        onChange={(e) => (e.target.value.endsWith(",") ? add(e.target.value) : setDraft(e.target.value))}
        onKeyDown={(e) => {
          if (e.key === "Enter") {
            e.preventDefault();
            add(draft);
          } else if (e.key === "Backspace" && !draft && value.length) onChange(value.slice(0, -1));
        }}
        onBlur={() => draft && add(draft)}
      />
      <datalist id="tag-suggestions">
        {suggestions.filter((s) => !value.includes(s)).map((s) => (
          <option key={s} value={s} />
        ))}
      </datalist>
    </div>
  );
}

function ContactForm({ contact, tagSuggestions, onClose, onSaved }: { contact?: ParticipantDetail; tagSuggestions: string[]; onClose: () => void; onSaved?: (id?: string) => void }) {
  const [form, setForm] = useState({
    fullName: contact?.fullName ?? "",
    email: contact?.email ?? "",
    phone: contact?.phone ?? "",
    source: contact?.source ?? "",
    experience: contact?.experience ?? "",
    emergencyContact: contact?.emergencyContact ?? "",
    notes: contact?.notes ?? "",
  });
  const [tags, setTags] = useState<string[]>(contact?.tags ?? []);
  const [consent, setConsent] = useState(contact?.marketingConsent ?? false);
  const [manual, setManual] = useState<CourseType[]>(contact?.manualQualifications ?? []);
  const set = (key: keyof typeof form) => (e: { target: { value: string } }) => setForm((f) => ({ ...f, [key]: e.target.value }));
  const save = useAction(
    () => {
      const body = { ...Object.fromEntries(Object.entries(form).map(([k, v]) => [k, v.trim() || null])), fullName: form.fullName, tags, marketingConsent: consent, manualQualifications: manual };
      return contact ? api(`/participants/${contact.id}`, { method: "PUT", body }) : api<{ id: string }>("/participants", { method: "POST", body });
    },
    contact ? "Zapisano zmiany" : `Dodano klienta: ${form.fullName}`,
  );
  const { submit, error, busy } = useFormSubmit(async () => {
    const result = (await save.mutateAsync(undefined)) as { id: string } | undefined;
    onClose();
    onSaved?.(result?.id);
  });
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-h-[92svh] overflow-y-auto sm:max-w-lg">
        <form className="grid gap-4" onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>{contact ? "Edytuj klienta" : "Dodaj klienta"}</DialogTitle>
          </DialogHeader>
          <Field label="Imię i nazwisko">
            <Input autoFocus required maxLength={200} value={form.fullName} onChange={set("fullName")} />
          </Field>
          <div className="grid grid-cols-2 gap-3">
            <Field label="E-mail">
              <Input type="email" maxLength={254} value={form.email} onChange={set("email")} />
            </Field>
            <Field label="Telefon">
              <Input type="tel" maxLength={40} value={form.phone} onChange={set("phone")} />
            </Field>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Skąd o nas wie">
              <Input list="sources" maxLength={60} value={form.source} onChange={set("source")} placeholder="np. Polecenie" />
            </Field>
            <Field label="Doświadczenie">
              <Input maxLength={200} value={form.experience} onChange={set("experience")} placeholder="np. Żeglarz jachtowy" />
            </Field>
          </div>
          <datalist id="sources">
            {SOURCES.map((s) => (
              <option key={s} value={s} />
            ))}
          </datalist>
          <div className="grid gap-1.5">
            <span className="text-sm font-medium">Tagi</span>
            <TagInput value={tags} onChange={setTags} suggestions={tagSuggestions} />
          </div>
          <div className="grid gap-1.5">
            <span className="text-sm font-medium">Uprawnienia zdobyte gdzie indziej</span>
            <div className="flex flex-wrap gap-1.5" role="group" aria-label="Uprawnienia zdobyte gdzie indziej">
              {COURSE_TYPES.filter((t) => t !== "Other").map((t) => (
                <button key={t} type="button" aria-pressed={manual.includes(t)} onClick={() => setManual((list) => (list.includes(t) ? list.filter((x) => x !== t) : [...list, t]))} className={cn("rounded-lg border px-2.5 py-1 text-sm transition-colors", manual.includes(t) ? "border-primary bg-accent font-semibold text-primary" : "hover:bg-muted")}>
                  {COURSE_TYPE[t].short}
                </button>
              ))}
            </div>
            <p className="text-xs text-muted-foreground">Uprawnienia ze zdanych u Ciebie egzaminów dopiszą się same.</p>
          </div>
          <Field label="Kontakt alarmowy" hint="Dane innej osoby. Wpisuj je tylko wtedy, gdy są potrzebne na rejs.">
            <Input maxLength={200} value={form.emergencyContact} onChange={set("emergencyContact")} placeholder="np. Anna Nowak, +48 600 000 000" />
          </Field>
          <Field label="Notatki" hint="Nie wpisuj tu informacji o zdrowiu, diecie wynikającej z choroby ani innych danych wrażliwych.">
            <Textarea maxLength={4000} value={form.notes} onChange={set("notes")} />
          </Field>
          <label className="flex items-start gap-3 rounded-[10px] border p-3 text-sm">
            <input type="checkbox" className="mt-0.5 size-4" checked={consent} onChange={(e) => setConsent(e.target.checked)} />
            <span>
              <span className="font-medium">Zgoda na informacje marketingowe</span>
              <span className="block text-xs text-muted-foreground">Zaznacz tylko, jeśli osoba sama się zgodziła, np. w formularzu zapisu. Zmiana zapisze się w historii klienta.</span>
            </span>
          </label>
          <FormError message={error} />
          <DialogFooter>
            <NautivoButton type="button" variant="secondary" onClick={onClose}>
              Anuluj
            </NautivoButton>
            <NautivoButton type="submit" loading={busy}>
              {contact ? "Zapisz zmiany" : "Dodaj klienta"}
            </NautivoButton>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

/* --------------------------------------------------------- Karta klienta */

function Card({ title, icon: Icon, children, action }: { title: string; icon?: LucideIcon; children: ReactNode; action?: ReactNode }) {
  return (
    <section className="card p-5">
      <div className="mb-3 flex items-center justify-between gap-2">
        <h2 className="flex items-center gap-2 text-lg">
          {Icon && <Icon className="size-5 text-primary" aria-hidden />}
          {title}
        </h2>
        {action}
      </div>
      {children}
    </section>
  );
}

type TimelineItem = { key: string; at: string; icon: LucideIcon; title: string; text?: string; meta?: string; activity?: Activity };

/** Jedna oś czasu: wpisy z kontaktów oraz automatycznie zapisy na rejsy i wpłaty. */
function buildTimeline(person: ParticipantDetail): TimelineItem[] {
  const items: TimelineItem[] = person.activities.map((a) => ({ key: a.id, at: a.occurredAt, icon: ACTIVITY[a.type].icon, title: ACTIVITY[a.type].label, text: a.text, meta: a.isSystem ? "wpis automatyczny" : (a.author ?? undefined), activity: a }));
  for (const h of person.history) {
    items.push({ key: `e-${h.enrollmentId}`, at: h.enrolledAt, icon: h.courseId ? GraduationCap : Sailboat, title: `${h.courseId ? "Zapis na szkolenie" : "Zapis na rejs"}: ${h.cruiseName}`, meta: range(h.startDate, h.endDate) });
    h.payments.forEach((p, i) => items.push({ key: `p-${h.enrollmentId}-${i}`, at: `${p.paidOn}T12:00:00Z`, icon: Wallet, title: `Wpłata ${zl(p.amount)}`, meta: `${p.method}, ${h.cruiseName}` }));
  }
  return items.sort((a, b) => b.at.localeCompare(a.at));
}

function ActivityForm({ personId }: { personId: string }) {
  const [type, setType] = useState<ActivityType>("Note");
  const [text, setText] = useState("");
  const add = useAction(() => api(`/participants/${personId}/activities`, { method: "POST", body: { type, text } }), "Dodano wpis do historii");
  const { submit, error, busy } = useFormSubmit(async () => {
    await add.mutateAsync(undefined);
    setText("");
  });
  return (
    <form className="grid gap-2" onSubmit={submit}>
      <div className="flex flex-wrap gap-1 rounded-xl bg-muted p-1" role="group" aria-label="Rodzaj wpisu">
        {(Object.keys(ACTIVITY) as ActivityType[]).map((t) => {
          const Icon = ACTIVITY[t].icon;
          return (
            <button key={t} type="button" aria-pressed={type === t} onClick={() => setType(t)} className={cn("flex items-center gap-1.5 rounded-lg px-3 py-1 text-sm font-medium transition-colors", type === t ? "bg-white text-sea shadow-sm" : "text-muted-foreground hover:text-foreground")}>
              <Icon className="size-3.5" aria-hidden /> {ACTIVITY[t].label}
            </button>
          );
        })}
      </div>
      <Textarea required maxLength={2000} aria-label="Treść wpisu" placeholder={type === "Call" ? "O czym rozmawialiście?" : type === "Email" ? "Czego dotyczył e-mail?" : "Co warto zapamiętać o tym kliencie?"} value={text} onChange={(e) => setText(e.target.value)} />
      <FormError message={error} />
      <div>
        <NautivoButton type="submit" size="sm" icon={Plus} loading={busy}>
          Dodaj do historii
        </NautivoButton>
      </div>
    </form>
  );
}

function TaskList({ person }: { person: ParticipantDetail }) {
  const [title, setTitle] = useState("");
  const [due, setDue] = useState(addDays(today(), 3));
  const add = useAction(() => api(`/participants/${person.id}/tasks`, { method: "POST", body: { title, dueDate: due } }), "Dodano zadanie");
  const toggle = useAction((t: ContactTask) => api(`/tasks/${t.id}/done`, { method: "PUT", body: { done: !t.doneAt } }));
  const remove = useAction((id: string) => api(`/tasks/${id}`, { method: "DELETE" }), "Usunięto zadanie");
  const { submit, error, busy } = useFormSubmit(async () => {
    await add.mutateAsync(undefined);
    setTitle("");
  });
  return (
    <>
      {person.tasks.length === 0 ? (
        <p className="mb-3 text-sm text-muted-foreground">Brak zadań. Dodaj przypomnienie, np. „oddzwonić w sprawie rejsu”.</p>
      ) : (
        <ul className="mb-3 divide-y text-sm">
          {person.tasks.map((t) => (
            <li key={t.id} className="flex items-center gap-3 py-2">
              <input type="checkbox" className="size-4" checked={!!t.doneAt} aria-label={`Wykonane: ${t.title}`} onChange={() => toggle.mutate(t)} />
              <span className={cn("min-w-0 flex-1", t.doneAt && "text-muted-foreground line-through")}>{t.title}</span>
              <span className={cn("text-xs whitespace-nowrap", t.doneAt ? "text-muted-foreground" : dueTone(t.dueDate))}>{t.doneAt ? `wykonane ${dateOf(t.doneAt)}` : dueLabel(t.dueDate)}</span>
              <NautivoIconButton action="delete" size="sm" label={`zadanie ${t.title}`} onClick={() => remove.mutate(t.id)} />
            </li>
          ))}
        </ul>
      )}
      <form className="flex flex-wrap gap-2" onSubmit={submit}>
        <Input required maxLength={200} aria-label="Nowe zadanie" placeholder="Nowe zadanie" className="min-w-40 flex-1" value={title} onChange={(e) => setTitle(e.target.value)} />
        <Input type="date" required aria-label="Termin" className="w-40" value={due} onChange={(e) => setDue(e.target.value)} />
        <NautivoButton type="submit" variant="secondary" icon={Plus} loading={busy}>
          Dodaj
        </NautivoButton>
      </form>
      <FormError message={error} />
    </>
  );
}

const historyColumns: NautivoColumn<ParticipantHistory>[] = [
  {
    key: "cruise",
    header: "Rejs lub szkolenie",
    cell: (h) => (
      <>
        <Link to={h.courseId ? `/szkolenia/${h.courseId}` : `/rejsy/${h.cruiseId}`} className="font-medium hover:underline">
          {h.cruiseName}
        </Link>
        <div className="text-xs text-muted-foreground">{range(h.startDate, h.endDate)}</div>
      </>
    ),
  },
  { key: "state", header: "Płatność", cell: (h) => (h.status === "Confirmed" ? <StatusPill state={h.state} /> : <span className="text-xs text-muted-foreground">{h.status === "Waitlist" ? "Lista rezerwowa" : "Anulowany zapis"}</span>) },
  { key: "paid", header: "Wpłacono", cell: (h) => <PaidBar paid={h.paid} total={h.total} /> },
];

export function ContactPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data: person, error } = useQuery({ queryKey: ["participant", id], queryFn: () => api<ParticipantDetail>(`/participants/${id}`) });
  const { data: everyone } = useQuery({ queryKey: ["participants", ""], queryFn: () => api<ParticipantItem[]>("/participants?search=") });
  const [editing, setEditing] = useState(false);
  const [removing, setRemoving] = useState(false);
  const removeActivity = useAction((activityId: string) => api(`/activities/${activityId}`, { method: "DELETE" }), "Usunięto wpis");
  const remove = useAction(() => api(`/participants/${id}`, { method: "DELETE" }), "Usunięto klienta");
  const anonymize = useAction(() => api(`/participants/${id}/anonymize`, { method: "POST" }), "Dane osoby zostały zanonimizowane");

  if (!person) {
    return error ? <EmptyState title="Nie można otworzyć karty klienta" text={error.message} action={<NautivoButton variant="secondary" onClick={() => navigate("/uczestnicy")}>Wróć do klientów</NautivoButton>} /> : <LoadState />;
  }

  const anonymized = !!person.anonymizedAt;
  const hasHistory = person.history.length > 0;
  const sailed = person.history.filter((h) => h.status === "Confirmed");
  const paid = sailed.reduce((s, h) => s + h.paid, 0);
  const due = sailed.reduce((s, h) => s + Math.max(0, h.total - h.paid), 0);
  const now = today();
  const next = [...sailed].filter((h) => h.endDate >= now).sort((a, b) => a.startDate.localeCompare(b.startDate))[0];
  const last = sailed.filter((h) => h.endDate < now)[0];
  const timeline = buildTimeline(person);
  const allTags = [...new Set((everyone ?? []).flatMap((p) => p.tags))];

  const exportData = async () => {
    const data = await api(`/participants/${person.id}/export`);
    download(`dane-${person.fullName.replace(/\s+/g, "-").toLowerCase()}-${now}.json`, JSON.stringify(data, null, 2), "application/json");
  };

  return (
    <>
      <PageHead
        back={<NautivoBackButton fallback="/uczestnicy" label="Wróć do klientów" />}
        title={person.fullName}
        subtitle={anonymized ? `Dane zanonimizowane ${dateOf(person.anonymizedAt!)}` : `Klient od ${dateOf(person.createdAt)}${person.source ? `, źródło: ${person.source}` : ""}`}
        actions={
          !anonymized && (
            <>
              <NautivoButton variant="secondary" icon={FileDown} onClick={() => void exportData()} title="Wszystkie dane tej osoby w pliku JSON (na jej żądanie, art. 15 i 20 RODO)">
                Eksport danych
              </NautivoButton>
              <NautivoButton variant="secondary" onClick={() => setEditing(true)}>
                Edytuj
              </NautivoButton>
              <NautivoButton variant="danger" icon={hasHistory ? UserX : Trash2} onClick={() => setRemoving(true)}>
                {hasHistory ? "Anonimizuj" : "Usuń"}
              </NautivoButton>
            </>
          )
        }
      />
      {!anonymized && (
        <div className="-mt-2 mb-5 flex flex-wrap items-center gap-2">
          <StageBadge stage={person.stage} />
          <Tags tags={person.tags} />
        </div>
      )}

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.6fr)]">
        <div className="grid content-start gap-6">
          <Card title="Kontakt">
            <dl className="grid gap-3 text-sm">
              <Row label="E-mail">{person.email ? <a className="text-primary hover:underline" href={`mailto:${person.email}`}>{person.email}</a> : null}</Row>
              <Row label="Telefon">{person.phone ? <a className="text-primary hover:underline" href={`tel:${person.phone.replace(/\s/g, "")}`}>{person.phone}</a> : null}</Row>
              <Row label="Doświadczenie">{person.experience}</Row>
              <Row label="Kontakt alarmowy">{person.emergencyContact}</Row>
              <Row label="Zgoda marketingowa">
                {person.marketingConsent ? (
                  <span className="inline-flex items-center gap-1 text-paid">
                    <ShieldCheck className="size-4" aria-hidden /> tak{person.marketingConsentAt && `, od ${dateOf(person.marketingConsentAt)}`}
                  </span>
                ) : (
                  "nie"
                )}
              </Row>
            </dl>
            {person.notes && <p className="mt-4 rounded-lg bg-muted p-3 text-sm whitespace-pre-wrap">{person.notes}</p>}
          </Card>

          <Card title="Podsumowanie">
            <dl className="grid grid-cols-2 gap-4 text-sm">
              <Stat label="Rejsy i szkolenia">{sailed.length}</Stat>
              <Stat label="Wpłacono łącznie">{zl(paid)}</Stat>
              <Stat label="Do zapłaty" tone={due > 0 ? "text-late" : undefined}>
                {zl(due)}
              </Stat>
              <Stat label={next ? "Najbliższy termin" : "Ostatni termin"}>{next ? fullDay(next.startDate) : last ? fullDay(last.endDate) : "brak"}</Stat>
            </dl>
          </Card>

          {!anonymized && <QualificationsCard person={person} />}

          {!anonymized && (
            <Card title="Zadania" icon={CalendarClock}>
              <TaskList person={person} />
            </Card>
          )}
        </div>

        <div className="grid content-start gap-6">
          <Card title="Historia kontaktu">
            {!anonymized && (
              <div className="mb-5">
                <ActivityForm personId={person.id} />
              </div>
            )}
            {timeline.length === 0 ? (
              <p className="text-sm text-muted-foreground">Nic się jeszcze nie wydarzyło. Zapisz pierwszy kontakt powyżej.</p>
            ) : (
              <ol className="relative grid gap-4 border-l pl-6">
                {timeline.map((item) => {
                  const Icon = item.icon;
                  return (
                    <li key={item.key} className="relative">
                      <span className="absolute top-0 -left-[37px] grid size-6 place-items-center rounded-full border bg-card text-primary">
                        <Icon className="size-3.5" aria-hidden />
                      </span>
                      <div className="flex items-start justify-between gap-2">
                        <div className="min-w-0">
                          <div className="text-sm font-medium">{item.title}</div>
                          {item.text && <p className="mt-0.5 text-sm whitespace-pre-wrap">{item.text}</p>}
                          <div className="mt-0.5 text-xs text-muted-foreground">
                            {item.activity ? when(item.at) : dateOf(item.at)}
                            {item.meta && `, ${item.meta}`}
                          </div>
                        </div>
                        {item.activity && !item.activity.isSystem && !anonymized && <NautivoIconButton action="delete" size="sm" label="wpis z historii" onClick={() => removeActivity.mutate(item.activity!.id)} />}
                      </div>
                    </li>
                  );
                })}
              </ol>
            )}
          </Card>

          <section aria-labelledby="cruise-history">
            <h2 id="cruise-history" className="mb-2 text-lg">
              Rejsy i szkolenia
            </h2>
            {hasHistory ? <NautivoTable columns={historyColumns} rows={person.history} rowKey={(h) => h.enrollmentId} caption="Rejsy i szkolenia klienta" minWidth={520} /> : <EmptyState title="Bez zapisów" text="Zapisz tę osobę na rejs lub szkolenie z ich widoku." />}
          </section>
        </div>
      </div>

      {editing && <ContactForm contact={person} tagSuggestions={allTags} onClose={() => setEditing(false)} />}
      <ConfirmDialog
        open={removing}
        title={hasHistory ? `Zanonimizować dane: ${person.fullName}?` : `Usunąć: ${person.fullName}?`}
        text={
          hasHistory
            ? "Znikną imię i nazwisko, kontakt, notatki, tagi, zgody, historia kontaktu i zadania. Zostaną kwoty i daty wpłat potrzebne do rozliczeń. Tego nie da się cofnąć."
            : "Klient, jego historia kontaktu i zadania zostaną usunięte. Tego nie da się cofnąć."
        }
        confirmLabel={hasHistory ? "Anonimizuj" : "Usuń klienta"}
        onConfirm={async () => {
          if (hasHistory) {
            await anonymize.mutateAsync(undefined);
          } else {
            await remove.mutateAsync(undefined);
            navigate("/uczestnicy");
          }
        }}
        onClose={() => setRemoving(false)}
      />
    </>
  );
}

/** Uprawnienia: ze zdanych u nas egzaminów (z numerem patentu) i wpisane ręcznie, plus podpowiedź kolejnego szkolenia. */
function QualificationsCard({ person }: { person: ParticipantDetail }) {
  const earned = person.earnedQualifications;
  const manual = person.manualQualifications.filter((q) => !earned.some((e) => e.type === q));
  const all = [...new Set([...earned.map((e) => e.type), ...person.manualQualifications])];
  const next = COURSE_TYPES.filter((t) => CANDIDATES[t]?.requires && isCandidate(all, t));
  return (
    <Card title="Uprawnienia" icon={Award}>
      {earned.length === 0 && manual.length === 0 ? (
        <p className="text-sm text-muted-foreground">Brak. Zdany egzamin na Twoim szkoleniu doda uprawnienie automatycznie.</p>
      ) : (
        <ul className="grid gap-2 text-sm">
          {earned.map((q) => (
            <li key={q.courseId} className="flex items-start gap-2">
              <CourseBadge type={q.type} />
              <span>
                {COURSE_TYPE[q.type].label}
                <span className="block text-xs text-muted-foreground">
                  <Link to={`/szkolenia/${q.courseId}`} className="hover:underline">
                    {q.courseName}
                  </Link>
                  , {fullDay(q.date)}
                  {q.certificateNumber && `, nr ${q.certificateNumber}`}
                </span>
              </span>
            </li>
          ))}
          {manual.map((q) => (
            <li key={q} className="flex items-start gap-2">
              <CourseBadge type={q} />
              <span>
                {COURSE_TYPE[q].label}
                <span className="block text-xs text-muted-foreground">wpisane ręcznie</span>
              </span>
            </li>
          ))}
        </ul>
      )}
      {next.length > 0 && <p className="mt-3 rounded-lg bg-accent p-2 text-sm">Kandydat na: {next.map((t) => COURSE_TYPE[t].label).join(", ")}</p>}
    </Card>
  );
}

function Row({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="grid grid-cols-[140px_1fr] gap-2">
      <dt className="text-muted-foreground">{label}</dt>
      <dd className="min-w-0 break-words">{children || <span className="text-muted-foreground">nie podano</span>}</dd>
    </div>
  );
}

function Stat({ label, children, tone }: { label: string; children: ReactNode; tone?: string }) {
  return (
    <div>
      <dt className="text-muted-foreground">{label}</dt>
      <dd className={cn("text-lg font-bold", tone)}>{children}</dd>
    </div>
  );
}

