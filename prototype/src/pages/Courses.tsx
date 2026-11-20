import { useQuery } from "@tanstack/react-query";
import { BookOpen, Check, CircleCheck, ClipboardList, ExternalLink, Flag, GraduationCap, ListChecks, Pencil, Plus, RotateCcw, Trash2 } from "lucide-react";
import { useState, type ReactNode } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { api, type CourseDetail, type CourseItem, type CourseSession, type CourseTest, type CourseType, type ExamResult, type SessionKind, type Trainee } from "@/api";
import { NautivoBackButton, NautivoButton, NautivoIconButton, NautivoRowActions, NautivoTable, type NautivoColumn } from "@/components/nautivo";
import { addDays, ConfirmDialog, day, EmptyState, Field, FormError, fullDay, LoadState, PageHead, PaidBar, PaymentDialog, plural, range, selectClass, StatusPill, today, useAction, useFormSubmit, zl } from "@/components/shared";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { cn } from "@/lib/utils";
import { CostsTable, EnrollmentDialog, FinanceChart, PaymentStatusChart } from "@/pages/Cruises";

/* ------------------------------------------------------------ Słowniki */

/** Rodzaje szkoleń. Nazwy są robocze; wymagania egzaminacyjne ustala organizator. */
export const COURSE_TYPE: Record<CourseType, { short: string; label: string; className: string }> = {
  Zj: { short: "ŻJ", label: "Żeglarz jachtowy", className: "bg-accent text-primary" },
  Jsm: { short: "JSM", label: "Jachtowy sternik morski", className: "bg-sea text-white" },
  Stcw: { short: "STCW", label: "Szkolenie STCW", className: "bg-part-soft text-part" },
  Src: { short: "SRC", label: "Radio VHF (SRC)", className: "bg-paid-soft text-paid" },
  Smw: { short: "SMW", label: "Sternik motorowodny", className: "bg-late-soft text-late" },
  Other: { short: "Inne", label: "Inne szkolenie", className: "bg-muted text-muted-foreground" },
};
export const COURSE_TYPES = Object.keys(COURSE_TYPE) as CourseType[];

const KIND: Record<SessionKind, { label: string; className: string }> = {
  Theory: { label: "Teoria", className: "bg-accent text-primary" },
  Practice: { label: "Praktyka", className: "bg-paid-soft text-paid" },
  Exam: { label: "Egzamin", className: "bg-part-soft text-part" },
};

const EXAM: Record<ExamResult, { label: string; className: string }> = {
  Pending: { label: "Przed egzaminem", className: "bg-muted text-muted-foreground" },
  Passed: { label: "Zdany", className: "bg-paid-soft text-paid" },
  Failed: { label: "Niezdany", className: "bg-late-soft text-late" },
  Absent: { label: "Nieobecny", className: "bg-part-soft text-part" },
};

const COURSE_COSTS = ["Instruktor", "Wynajem łodzi", "Czarter jachtu", "Wynajem sali", "Opłaty egzaminacyjne", "Materiały szkoleniowe", "Paliwo", "Zakwaterowanie", "Wyżywienie", "Inne"];

export function CourseBadge({ type }: { type: CourseType }) {
  return (
    <span className={cn("inline-block rounded-md px-1.5 py-0.5 text-xs font-bold", COURSE_TYPE[type].className)} title={COURSE_TYPE[type].label}>
      {COURSE_TYPE[type].short}
    </span>
  );
}

function Pill({ className, children }: { className: string; children: ReactNode }) {
  return <span className={cn("inline-block rounded-full px-2.5 py-0.5 text-xs font-semibold whitespace-nowrap", className)}>{children}</span>;
}

const signed = (n: number) => (n > 0 ? `+${zl(n)}` : zl(n));
const percent = (part: number, whole: number) => (whole > 0 ? Math.round((part / whole) * 100) : 0);
const time = (t: string | null) => (t ? t.slice(0, 5) : "");
function hours(s: CourseSession) {
  if (!s.startTime || !s.endTime) return 0;
  const [h1, m1] = s.startTime.split(":").map(Number);
  const [h2, m2] = s.endTime.split(":").map(Number);
  return Math.max(0, h2 * 60 + m2 - (h1 * 60 + m1)) / 60;
}
const hoursLabel = (h: number) => `${h.toLocaleString("pl-PL", { maximumFractionDigits: 1 })} h`;

/* --------------------------------------------------------------- Lista */

export function CoursesPage() {
  const { data, error } = useQuery({ queryKey: ["courses"], queryFn: () => api<CourseItem[]>("/courses") });
  const [editing, setEditing] = useState<string | "new" | null>(null);
  const [deleting, setDeleting] = useState<CourseItem | null>(null);
  const [tab, setTab] = useState<"upcoming" | "past">("upcoming");
  const [type, setType] = useState<CourseType | null>(null);
  const now = today();
  const shown = (data ?? [])
    // Kurs zakończony ręcznie jest zakończony, nawet jeśli termin jeszcze trwa.
    .filter((c) => (tab === "upcoming" ? !c.completedAt && c.endDate >= now : !!c.completedAt || c.endDate < now) && (!type || c.type === type))
    .sort((a, b) => (tab === "upcoming" ? a.startDate.localeCompare(b.startDate) : b.startDate.localeCompare(a.startDate)));
  const types = COURSE_TYPES.filter((t) => data?.some((c) => c.type === t));
  const addButton = (
    <NautivoButton icon={Plus} onClick={() => setEditing("new")}>
      Dodaj szkolenie
    </NautivoButton>
  );

  return (
    <>
      <PageHead title="Szkolenia" subtitle="Kursy na patenty i uprawnienia: zapisy, harmonogram, frekwencja, egzaminy i rozliczenie." actions={addButton} />
      {!data && <LoadState error={error} />}
      {data?.length === 0 && <EmptyState title="Nie ma jeszcze szkoleń" text="Dodaj pierwsze szkolenie, np. kurs na żeglarza jachtowego." action={addButton} />}
      {data && data.length > 0 && (
        <div className="mb-4 flex flex-wrap items-center gap-3">
          <div className="inline-flex gap-1 rounded-xl bg-muted p-1" role="group" aria-label="Pokaż szkolenia">
            {(
              [
                ["upcoming", "Nadchodzące i trwające"],
                ["past", "Zakończone"],
              ] as const
            ).map(([id, label]) => (
              <button key={id} type="button" aria-pressed={tab === id} onClick={() => setTab(id)} className={cn("rounded-lg px-4 py-1.5 text-sm font-medium transition-colors", tab === id ? "bg-white text-sea shadow-sm" : "text-muted-foreground hover:text-foreground")}>
                {label}
              </button>
            ))}
          </div>
          {types.length > 1 && (
            <div className="flex flex-wrap gap-1" role="group" aria-label="Rodzaj szkolenia">
              {types.map((t) => (
                <button key={t} type="button" aria-pressed={type === t} onClick={() => setType(type === t ? null : t)} className={cn("rounded-lg border px-2.5 py-1 text-sm transition-colors", type === t ? "border-primary bg-accent font-semibold text-primary" : "hover:bg-muted")}>
                  {COURSE_TYPE[t].short}
                </button>
              ))}
            </div>
          )}
        </div>
      )}
      {data && data.length > 0 && shown.length === 0 && <EmptyState title="Brak szkoleń" text="Zmień filtr albo dodaj nowe szkolenie." />}
      {shown.length > 0 && (
        <ul className="card divide-y">
          {shown.map((c) => {
            const result = c.expected - c.costs;
            return (
              <li key={c.id} className="flex items-center gap-2 pr-3 transition-colors hover:bg-muted/40">
                <Link to={`/szkolenia/${c.id}`} className="grid min-w-0 flex-1 items-center gap-x-6 gap-y-2 p-4 md:grid-cols-[minmax(0,2.2fr)_minmax(0,1fr)_minmax(0,1fr)_minmax(0,1fr)]">
                  <div className="min-w-0">
                    <h2 className="flex items-center gap-2 truncate text-base">
                      <CourseBadge type={c.type} /> <span className="truncate">{c.name}</span>
                      {c.status === "Cancelled" && <span className="text-sm font-medium text-late">odwołane</span>}
                      {c.completedAt && <Pill className="bg-paid-soft text-paid">zakończone</Pill>}
                    </h2>
                    <p className="mt-0.5 text-sm text-muted-foreground">{[range(c.startDate, c.endDate), c.location, c.instructor].filter(Boolean).join(" · ")}</p>
                  </div>
                  <Meter label={`${c.confirmed} z ${c.capacity} kursantów`} value={percent(c.confirmed, c.capacity)} />
                  <Meter label={c.sessions ? `Zajęcia ${c.sessionsDone} z ${c.sessions}` : "Brak harmonogramu"} value={percent(c.sessionsDone, c.sessions)} tone="bg-paid" />
                  <div className="text-sm md:text-right">
                    <div>
                      {zl(c.collected)} <span className="text-muted-foreground">z {zl(c.expected)}</span>
                    </div>
                    <div className={cn("text-xs", result >= 0 ? "text-paid" : "text-late")}>Wynik: {signed(result)}</div>
                    {c.overdue > 0 && <div className="text-xs text-late">Zaległe: {c.overdue}</div>}
                  </div>
                </Link>
                <NautivoIconButton action="edit" label={`szkolenie ${c.name}`} onClick={() => setEditing(c.id)} />
                <NautivoIconButton action="delete" label={`szkolenie ${c.name}`} onClick={() => setDeleting(c)} />
              </li>
            );
          })}
        </ul>
      )}
      {editing && <CourseForm key={editing} courseId={editing === "new" ? undefined : editing} onClose={() => setEditing(null)} />}
      <DeleteCourseDialog course={deleting} onClose={() => setDeleting(null)} />
    </>
  );
}

function Meter({ label, value, tone = "bg-primary" }: { label: string; value: number; tone?: string }) {
  return (
    <div>
      <div className="text-sm">{label}</div>
      <div className="mt-1 h-1.5 rounded-full bg-muted">
        <div className={cn("h-1.5 rounded-full", tone)} style={{ width: `${Math.min(100, value)}%` }} />
      </div>
    </div>
  );
}

function DeleteCourseDialog({ course, onClose, onDeleted }: { course: CourseItem | null; onClose: () => void; onDeleted?: () => void }) {
  const remove = useAction((id: string) => api(`/courses/${id}`, { method: "DELETE" }), "Usunięto szkolenie");
  return (
    <ConfirmDialog
      open={!!course}
      title={`Usunąć szkolenie „${course?.name ?? ""}”?`}
      text={
        course && course.confirmed + course.collected + course.costs > 0
          ? `Razem ze szkoleniem znikną: ${plural(course.confirmed, "zapis kursanta", "zapisy kursantów", "zapisów kursantów")}, wpłaty (${zl(course.collected)}), koszty (${zl(course.costs)}), harmonogram, obecności, testy i materiały. Osoby zostaną w bazie klientów, ale stracą uprawnienie z tego szkolenia. Tego nie da się cofnąć.`
          : "Szkolenie zniknie razem z harmonogramem i materiałami. Tego nie da się cofnąć."
      }
      confirmLabel="Usuń szkolenie"
      onConfirm={async () => {
        await remove.mutateAsync(course!.id);
        onDeleted?.();
      }}
      onClose={onClose}
    />
  );
}

/* ----------------------------------------------------------- Formularz */

export function CourseForm({ courseId, onClose }: { courseId?: string; onClose: () => void }) {
  const { data, error } = useQuery({ queryKey: ["course", courseId], queryFn: () => api<CourseDetail>(`/courses/${courseId}`), enabled: !!courseId });
  if (courseId && !data) {
    return (
      <Dialog open onOpenChange={(o) => !o && onClose()}>
        <DialogContent>
          <DialogTitle>Edytuj szkolenie</DialogTitle>
          <LoadState error={error} />
        </DialogContent>
      </Dialog>
    );
  }
  return <CourseFormBody detail={data} onClose={onClose} />;
}

function CourseFormBody({ detail, onClose }: { detail?: CourseDetail; onClose: () => void }) {
  const navigate = useNavigate();
  const course = detail?.course;
  const start = addDays(today(), 45);
  const [form, setForm] = useState({
    type: course?.type ?? ("Zj" as CourseType),
    name: course?.name ?? COURSE_TYPE.Zj.label,
    startDate: course?.startDate ?? start,
    endDate: course?.endDate ?? addDays(start, 7),
    capacity: String(course?.capacity ?? 10),
    price: course ? String(course.price) : "",
    location: course?.location ?? "",
    instructor: course?.instructor ?? "",
    examDate: course?.examDate ?? "",
    examInfo: detail?.examInfo ?? "",
    documents: (detail?.requiredDocuments ?? []).join("\n"),
    deposit: course?.deposit ? String(course.deposit) : "",
    balanceDueDate: course && course.balanceDueDate !== course.startDate ? course.balanceDueDate : "",
    status: course?.status ?? "Planned",
    description: detail?.description ?? "",
  });
  const set = (key: keyof typeof form) => (e: { target: { value: string } }) => setForm((f) => ({ ...f, [key]: e.target.value }));
  const chooseType = (type: CourseType) =>
    // Nazwa podpowiada się z rodzaju, dopóki ktoś jej nie zmienił.
    setForm((f) => ({ ...f, type, name: COURSE_TYPES.some((t) => COURSE_TYPE[t].label === f.name) || !f.name ? COURSE_TYPE[type].label : f.name }));

  const save = useAction(
    () => {
      const body = {
        type: form.type,
        name: form.name,
        startDate: form.startDate,
        endDate: form.endDate,
        capacity: Number(form.capacity),
        price: Number(form.price || 0),
        location: form.location || null,
        instructor: form.instructor || null,
        examDate: form.examDate || null,
        examInfo: form.examInfo || null,
        requiredDocuments: form.documents.split("\n").map((d) => d.trim()).filter(Boolean),
        deposit: Number(form.deposit || 0),
        balanceDueDate: form.balanceDueDate || null,
        status: form.status,
        description: form.description || null,
      };
      return course ? api(`/courses/${course.id}`, { method: "PUT", body }) : api<{ id: string }>("/courses", { method: "POST", body });
    },
    course ? "Zapisano zmiany w szkoleniu" : `Dodano szkolenie: ${form.name}`,
  );
  const { submit, error, busy } = useFormSubmit(async () => {
    const result = (await save.mutateAsync(undefined)) as { id: string } | undefined;
    onClose();
    if (!course && result?.id) navigate(`/szkolenia/${result.id}`);
  });

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="max-h-[92svh] overflow-y-auto sm:max-w-2xl">
        <form className="grid gap-5" onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>{course ? "Edytuj szkolenie" : "Dodaj szkolenie"}</DialogTitle>
          </DialogHeader>
          <div className="grid gap-1.5">
            <span className="text-sm font-medium">Rodzaj szkolenia</span>
            <div className="flex flex-wrap gap-1.5" role="group" aria-label="Rodzaj szkolenia">
              {COURSE_TYPES.map((t) => (
                <button key={t} type="button" aria-pressed={form.type === t} onClick={() => chooseType(t)} className={cn("rounded-lg border px-3 py-1.5 text-sm transition-colors", form.type === t ? "border-primary bg-accent font-semibold text-primary" : "hover:bg-muted")}>
                  {COURSE_TYPE[t].short} <span className="hidden text-xs font-normal text-muted-foreground sm:inline">{COURSE_TYPE[t].label}</span>
                </button>
              ))}
            </div>
          </div>
          <Field label="Nazwa szkolenia">
            <Input required maxLength={200} value={form.name} onChange={set("name")} />
          </Field>
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
            <Field label="Początek">
              <Input type="date" required value={form.startDate} onChange={set("startDate")} />
            </Field>
            <Field label="Koniec">
              <Input type="date" required min={form.startDate} value={form.endDate} onChange={set("endDate")} />
            </Field>
            <Field label="Liczba miejsc">
              <Input type="number" min={1} max={500} required value={form.capacity} onChange={set("capacity")} />
            </Field>
            <Field label="Cena za kursanta (zł)">
              <Input type="number" min={0} step={0.01} required value={form.price} onChange={set("price")} placeholder="np. 1690" />
            </Field>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Miejsce">
              <Input maxLength={200} value={form.location} onChange={set("location")} placeholder="np. Giżycko, przystań" />
            </Field>
            <Field label="Prowadzący">
              <Input maxLength={200} value={form.instructor} onChange={set("instructor")} placeholder="np. Marta Czapla" />
            </Field>
          </div>
          <div className="grid gap-3 sm:grid-cols-[1fr_2fr]">
            <Field label="Data egzaminu">
              <Input type="date" min={form.startDate} value={form.examDate} onChange={set("examDate")} />
            </Field>
            <Field label="Informacje o egzaminie">
              <Input maxLength={1000} value={form.examInfo} onChange={set("examInfo")} placeholder="np. kto egzaminuje, co zabrać" />
            </Field>
          </div>
          <Field label="Wymagane dokumenty" hint="Każdy w osobnej linii, np. zdjęcie, kopia patentu ŻJ. Odhaczysz je przy kursancie. Nie wpisuj tu treści dokumentów ani danych o zdrowiu.">
            <Textarea rows={3} value={form.documents} onChange={set("documents")} placeholder={"Zdjęcie\nKopia patentu ŻJ"} />
          </Field>
          <details className="rounded-[10px] border p-3" open={!!(form.deposit || form.balanceDueDate || form.description)}>
            <summary className="cursor-pointer text-sm font-semibold">
              Zaliczka, termin dopłaty i opis <span className="font-normal text-muted-foreground">(opcjonalnie)</span>
            </summary>
            <div className="mt-3 grid gap-3">
              <div className="grid grid-cols-2 gap-3">
                <Field label="Zaliczka (zł)">
                  <Input type="number" min={0} step={0.01} value={form.deposit} onChange={set("deposit")} placeholder="0" />
                </Field>
                <Field label="Dopłata do" hint="Bez daty: dzień startu.">
                  <Input type="date" max={form.startDate} value={form.balanceDueDate} onChange={set("balanceDueDate")} />
                </Field>
              </div>
              <Field label="Opis">
                <Textarea maxLength={4000} value={form.description} onChange={set("description")} />
              </Field>
            </div>
          </details>
          {course && (
            <Field label="Status">
              <select className={selectClass} value={form.status} onChange={set("status")}>
                <option value="Planned">Planowane</option>
                <option value="Cancelled">Odwołane</option>
              </select>
            </Field>
          )}
          <FormError message={error} />
          <DialogFooter>
            <NautivoButton type="button" variant="secondary" onClick={onClose}>
              Anuluj
            </NautivoButton>
            <NautivoButton type="submit" loading={busy}>
              {course ? "Zapisz zmiany" : "Dodaj szkolenie"}
            </NautivoButton>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

/* ----------------------------------------------------- Postęp kursanta */

function ProgressDialog({ trainee, documents, onClose }: { trainee: Trainee; documents: string[]; onClose: () => void }) {
  const [delivered, setDelivered] = useState<string[]>(trainee.documentsDelivered);
  const [result, setResult] = useState<ExamResult>(trainee.examResult);
  const [certificate, setCertificate] = useState(trainee.certificateNumber ?? "");
  const save = useAction(
    () => api(`/enrollments/${trainee.enrollment.id}/progress`, { method: "PUT", body: { examResult: result, certificateNumber: certificate || null, documentsDelivered: delivered } }),
    `Zapisano postęp: ${trainee.enrollment.fullName}`,
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
            <DialogTitle>Postęp: {trainee.enrollment.fullName}</DialogTitle>
            <DialogDescription>Zdany egzamin doda uprawnienie na karcie klienta w CRM.</DialogDescription>
          </DialogHeader>
          {documents.length > 0 && (
            <fieldset className="grid gap-2">
              <legend className="mb-1 text-sm font-medium">Dostarczone dokumenty</legend>
              {documents.map((d) => (
                <label key={d} className="flex items-center gap-2 text-sm">
                  <input type="checkbox" className="size-4" checked={delivered.includes(d)} onChange={(e) => setDelivered((list) => (e.target.checked ? [...list, d] : list.filter((x) => x !== d)))} />
                  {d}
                </label>
              ))}
            </fieldset>
          )}
          <div className="grid grid-cols-2 gap-3">
            <Field label="Egzamin">
              <select className={selectClass} value={result} onChange={(e) => setResult(e.target.value as ExamResult)}>
                {(Object.keys(EXAM) as ExamResult[]).map((r) => (
                  <option key={r} value={r}>
                    {EXAM[r].label}
                  </option>
                ))}
              </select>
            </Field>
            <Field label="Nr patentu / świadectwa">
              <Input maxLength={60} value={certificate} onChange={(e) => setCertificate(e.target.value)} disabled={result !== "Passed"} placeholder={result === "Passed" ? "opcjonalnie" : "po zdanym egzaminie"} />
            </Field>
          </div>
          <FormError message={error} />
          <DialogFooter>
            <NautivoButton type="button" variant="secondary" onClick={onClose}>
              Anuluj
            </NautivoButton>
            <NautivoButton type="submit" loading={busy}>
              Zapisz
            </NautivoButton>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

/* ------------------------------------------------- Panel prowadzącego */

function SessionDialog({ courseId, session, defaultDate, onClose }: { courseId: string; session?: CourseSession; defaultDate: string; onClose: () => void }) {
  const [form, setForm] = useState({
    date: session?.date ?? defaultDate,
    startTime: time(session?.startTime ?? "09:00"),
    endTime: time(session?.endTime ?? "13:00"),
    kind: session?.kind ?? ("Theory" as SessionKind),
    topic: session?.topic ?? "",
    notes: session?.notes ?? "",
  });
  const set = (key: keyof typeof form) => (e: { target: { value: string } }) => setForm((f) => ({ ...f, [key]: e.target.value }));
  const save = useAction(() => {
    const body = { ...form, startTime: form.startTime || null, endTime: form.endTime || null, notes: form.notes || null };
    return session ? api(`/sessions/${session.id}`, { method: "PUT", body }) : api(`/courses/${courseId}/sessions`, { method: "POST", body });
  }, session ? "Zapisano zajęcia" : "Dodano zajęcia");
  const { submit, error, busy } = useFormSubmit(async () => {
    await save.mutateAsync(undefined);
    onClose();
  });
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="sm:max-w-lg">
        <form className="grid gap-4" onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>{session ? "Edytuj zajęcia" : "Dodaj zajęcia"}</DialogTitle>
          </DialogHeader>
          <div className="grid grid-cols-3 gap-3">
            <Field label="Data">
              <Input type="date" required value={form.date} onChange={set("date")} />
            </Field>
            <Field label="Od">
              <Input type="time" value={form.startTime} onChange={set("startTime")} />
            </Field>
            <Field label="Do">
              <Input type="time" value={form.endTime} onChange={set("endTime")} />
            </Field>
          </div>
          <div className="flex gap-1 rounded-xl bg-muted p-1" role="group" aria-label="Rodzaj zajęć">
            {(Object.keys(KIND) as SessionKind[]).map((k) => (
              <button key={k} type="button" aria-pressed={form.kind === k} onClick={() => setForm((f) => ({ ...f, kind: k }))} className={cn("flex-1 rounded-lg px-3 py-1 text-sm font-medium transition-colors", form.kind === k ? "bg-white text-sea shadow-sm" : "text-muted-foreground hover:text-foreground")}>
                {KIND[k].label}
              </button>
            ))}
          </div>
          <Field label="Temat">
            <Input required autoFocus maxLength={200} value={form.topic} onChange={set("topic")} placeholder="np. Manewry portowe" />
          </Field>
          <Field label="Plan lekcji" hint="Zagadnienia, ćwiczenia, potrzebny sprzęt. Widzi to tylko zespół organizatora.">
            <Textarea rows={5} maxLength={4000} value={form.notes} onChange={set("notes")} placeholder={"1. Odejście od kei\n2. Podejście do boi\n3. Człowiek za burtą"} />
          </Field>
          <FormError message={error} />
          <DialogFooter>
            <NautivoButton type="button" variant="secondary" onClick={onClose}>
              Anuluj
            </NautivoButton>
            <NautivoButton type="submit" loading={busy}>
              {session ? "Zapisz" : "Dodaj zajęcia"}
            </NautivoButton>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function Schedule({ detail }: { detail: CourseDetail }) {
  const [dialog, setDialog] = useState<CourseSession | "new" | null>(null);
  const [open, setOpen] = useState<string | null>(null);
  const remove = useAction((id: string) => api(`/sessions/${id}`, { method: "DELETE" }), "Usunięto zajęcia");
  const complete = useAction(
    ({ id, completed }: { id: string; completed: boolean }) => api(`/sessions/${id}/complete`, { method: "PUT", body: { completed } }),
    ({ completed }) => (completed ? "Lekcja ukończona" : "Cofnięto ukończenie lekcji"),
  );
  const now = today();
  const { course, sessions } = detail;
  const last = sessions[sessions.length - 1];
  return (
    <Panel title="Harmonogram i plan lekcji" icon={ClipboardList} action={<NautivoButton size="sm" variant="secondary" icon={Plus} onClick={() => setDialog("new")}>Dodaj zajęcia</NautivoButton>}>
      {sessions.length === 0 ? (
        <p className="text-sm text-muted-foreground">Dodaj zajęcia, aby śledzić postęp kursu i obecność.</p>
      ) : (
        <ol className="divide-y">
          {sessions.map((s) => {
            const done = !!s.completedAt;
            return (
              <li key={s.id} className="py-2.5">
                <div className="flex items-start gap-3">
                  <button
                    type="button"
                    aria-pressed={done}
                    aria-label={done ? `Cofnij ukończenie: ${s.topic}` : `Oznacz jako ukończoną: ${s.topic}`}
                    title={done ? "Ukończona. Kliknij, aby cofnąć." : "Oznacz jako ukończoną"}
                    disabled={complete.isPending}
                    onClick={() => complete.mutate({ id: s.id, completed: !done })}
                    className={cn("mt-0.5 grid size-5 shrink-0 place-items-center rounded-full border transition-colors", done ? "border-paid bg-paid text-white hover:bg-paid/80" : "bg-card hover:border-paid hover:bg-paid-soft")}
                  >
                    {done && <Check className="size-3" aria-hidden />}
                  </button>
                  <button type="button" className="min-w-0 flex-1 text-left" aria-expanded={open === s.id} onClick={() => setOpen(open === s.id ? null : s.id)}>
                    <span className="flex flex-wrap items-center gap-2">
                      <span className="font-medium">{s.topic}</span>
                      <Pill className={KIND[s.kind].className}>{KIND[s.kind].label}</Pill>
                    </span>
                    <span className="text-xs text-muted-foreground">
                      {fullDay(s.date)}
                      {s.startTime && `, ${time(s.startTime)}–${time(s.endTime)}`}
                      {hours(s) > 0 && ` (${hoursLabel(hours(s))})`}
                      {s.notes ? " · plan lekcji" : ""}
                    </span>
                  </button>
                  {done ? (
                    <span className="hidden text-xs font-semibold whitespace-nowrap text-paid sm:inline" title={`Ukończona ${fullDay(s.completedAt!.slice(0, 10))}`}>
                      Ukończona
                    </span>
                  ) : (
                    <NautivoButton size="sm" variant={s.date <= now ? "primary" : "secondary"} icon={CircleCheck} loading={complete.isPending && complete.variables?.id === s.id} onClick={() => complete.mutate({ id: s.id, completed: true })}>
                      Ukończ
                    </NautivoButton>
                  )}
                  <NautivoIconButton action="edit" size="sm" label={s.topic} onClick={() => setDialog(s)} />
                  <NautivoIconButton action="delete" size="sm" label={s.topic} onClick={() => remove.mutate(s.id)} />
                </div>
                {open === s.id && <p className="mt-2 ml-8 rounded-lg bg-muted p-3 text-sm whitespace-pre-wrap">{s.notes || "Brak planu lekcji. Dodaj go w edycji zajęć."}</p>}
              </li>
            );
          })}
        </ol>
      )}
      {dialog && <SessionDialog courseId={course.id} session={dialog === "new" ? undefined : dialog} defaultDate={last ? addDays(last.date, 1) : course.startDate} onClose={() => setDialog(null)} />}
    </Panel>
  );
}

function Attendance({ detail }: { detail: CourseDetail }) {
  const mark = useAction(({ enrollmentId, sessionId, present }: { enrollmentId: string; sessionId: string; present: boolean }) =>
    api(`/courses/${detail.course.id}/attendance`, { method: "PUT", body: { enrollmentId, sessionId, present } }),
  );
  if (detail.sessions.length === 0 || detail.trainees.length === 0) return null;
  const columns: NautivoColumn<Trainee>[] = [
    { key: "name", header: "Kursant", cell: (t) => <span className="font-medium whitespace-nowrap">{t.enrollment.fullName}</span> },
    ...detail.sessions.map<NautivoColumn<Trainee>>((s) => ({
      key: s.id,
      header: (
        <span className="block text-center text-xs leading-tight" title={s.topic}>
          {day(s.date)}
          <span className="block font-normal">{KIND[s.kind].label}</span>
        </span>
      ),
      className: "text-center",
      cell: (t) => (
        <input
          type="checkbox"
          className="size-4"
          aria-label={`${t.enrollment.fullName}: obecność ${fullDay(s.date)}, ${s.topic}`}
          checked={t.attendedSessions.includes(s.id)}
          onChange={(e) => mark.mutate({ enrollmentId: t.enrollment.id, sessionId: s.id, present: e.target.checked })}
        />
      ),
    })),
    { key: "total", header: "Obecność", align: "right", cell: (t) => <span className="font-semibold tabular-nums">{percent(t.attendedSessions.length, detail.sessions.length)}%</span> },
  ];
  return (
    <Panel title="Frekwencja" icon={ListChecks}>
      <NautivoTable columns={columns} rows={detail.trainees} rowKey={(t) => t.enrollment.id} caption="Obecność kursantów na zajęciach" minWidth={240 + detail.sessions.length * 70} />
    </Panel>
  );
}

function ScoreCell({ test, trainee, score }: { test: CourseTest; trainee: Trainee; score?: number }) {
  const [value, setValue] = useState(score == null ? "" : String(score));
  const save = useAction((next: number | null) => api(`/tests/${test.id}/results`, { method: "PUT", body: { enrollmentId: trainee.enrollment.id, score: next } }));
  const commit = () => {
    const next = value.trim() === "" ? null : Number(value.replace(",", "."));
    if (next === (score ?? null) || (next != null && Number.isNaN(next))) return;
    save.mutate(next);
  };
  const passed = score != null && score >= test.passScore;
  return (
    <span className="inline-flex items-center gap-2">
      <Input type="number" min={0} max={test.maxScore} step={0.5} className="h-8 w-20 text-right" aria-label={`${trainee.enrollment.fullName}: wynik testu ${test.title}`} value={value} onChange={(e) => setValue(e.target.value)} onBlur={commit} />
      {score != null && <span className={cn("text-xs font-semibold", passed ? "text-paid" : "text-late")}>{passed ? "zal." : "nzal."}</span>}
      {save.error && <span className="text-xs text-late">{save.error.message}</span>}
    </span>
  );
}

function Tests({ detail }: { detail: CourseDetail }) {
  const [draft, setDraft] = useState({ title: "", date: today(), maxScore: "", passScore: "" });
  const add = useAction(() => api(`/courses/${detail.course.id}/tests`, { method: "POST", body: { ...draft, maxScore: Number(draft.maxScore), passScore: Number(draft.passScore) } }), "Dodano test");
  const remove = useAction((id: string) => api(`/tests/${id}`, { method: "DELETE" }), "Usunięto test");
  const { submit, error, busy } = useFormSubmit(async () => {
    await add.mutateAsync(undefined);
    setDraft({ title: "", date: today(), maxScore: "", passScore: "" });
  });
  return (
    <Panel title="Testy sprawdzające" icon={GraduationCap}>
      <div className="grid gap-5">
        {detail.tests.map((test) => {
          const scores = test.results.map((r) => r.score);
          const average = scores.length ? scores.reduce((a, b) => a + b, 0) / scores.length : null;
          const passed = scores.filter((s) => s >= test.passScore).length;
          return (
            <div key={test.id} className="rounded-lg border">
              <div className="flex flex-wrap items-center justify-between gap-2 border-b px-3 py-2">
                <div>
                  <span className="font-medium">{test.title}</span>
                  <span className="ml-2 text-xs text-muted-foreground">
                    {fullDay(test.date)}, maks. {test.maxScore} pkt, próg {test.passScore} pkt
                  </span>
                </div>
                <div className="flex items-center gap-3 text-xs">
                  {average != null && (
                    <span>
                      Średnia: <strong>{percent(average, test.maxScore)}%</strong> · zaliczyło {passed} z {scores.length}
                    </span>
                  )}
                  <NautivoIconButton action="delete" size="sm" label={`test ${test.title}`} onClick={() => remove.mutate(test.id)} />
                </div>
              </div>
              {detail.trainees.length === 0 ? (
                <p className="p-3 text-sm text-muted-foreground">Brak kursantów.</p>
              ) : (
                <ul className="divide-y">
                  {detail.trainees.map((t) => (
                    <li key={t.enrollment.id} className="flex items-center justify-between gap-3 px-3 py-1.5 text-sm">
                      {t.enrollment.fullName}
                      <ScoreCell key={`${test.id}-${t.enrollment.id}`} test={test} trainee={t} score={test.results.find((r) => r.enrollmentId === t.enrollment.id)?.score} />
                    </li>
                  ))}
                </ul>
              )}
            </div>
          );
        })}
        <form className="grid gap-2 sm:grid-cols-[2fr_1fr_1fr_1fr_auto] sm:items-end" onSubmit={submit}>
          <Field label="Nowy test">
            <Input required maxLength={200} value={draft.title} onChange={(e) => setDraft((d) => ({ ...d, title: e.target.value }))} placeholder="np. Test próbny z locji" />
          </Field>
          <Field label="Data">
            <Input type="date" required value={draft.date} onChange={(e) => setDraft((d) => ({ ...d, date: e.target.value }))} />
          </Field>
          <Field label="Maks. pkt">
            <Input type="number" min={1} required value={draft.maxScore} onChange={(e) => setDraft((d) => ({ ...d, maxScore: e.target.value }))} />
          </Field>
          <Field label="Próg pkt">
            <Input type="number" min={0} required value={draft.passScore} onChange={(e) => setDraft((d) => ({ ...d, passScore: e.target.value }))} />
          </Field>
          <NautivoButton type="submit" variant="secondary" icon={Plus} loading={busy}>
            Dodaj
          </NautivoButton>
        </form>
        <FormError message={error} />
      </div>
    </Panel>
  );
}

function Materials({ detail }: { detail: CourseDetail }) {
  const [draft, setDraft] = useState({ title: "", url: "", note: "" });
  const add = useAction(() => api(`/courses/${detail.course.id}/materials`, { method: "POST", body: { title: draft.title, url: draft.url || null, note: draft.note || null } }), "Dodano materiał");
  const remove = useAction((id: string) => api(`/materials/${id}`, { method: "DELETE" }), "Usunięto materiał");
  const { submit, error, busy } = useFormSubmit(async () => {
    await add.mutateAsync(undefined);
    setDraft({ title: "", url: "", note: "" });
  });
  return (
    <Panel title="Materiały" icon={BookOpen}>
      {detail.materials.length > 0 && (
        <ul className="mb-4 divide-y text-sm">
          {detail.materials.map((m) => (
            <li key={m.id} className="flex items-start justify-between gap-3 py-2">
              <div className="min-w-0">
                {m.url ? (
                  <a href={m.url} target="_blank" rel="noreferrer noopener" className="inline-flex items-center gap-1 font-medium text-primary hover:underline">
                    {m.title} <ExternalLink className="size-3.5" aria-hidden />
                  </a>
                ) : (
                  <span className="font-medium">{m.title}</span>
                )}
                {m.note && <p className="text-xs text-muted-foreground">{m.note}</p>}
              </div>
              <NautivoIconButton action="delete" size="sm" label={`materiał ${m.title}`} onClick={() => remove.mutate(m.id)} />
            </li>
          ))}
        </ul>
      )}
      <form className="grid gap-2 sm:grid-cols-[1fr_1fr_auto] sm:items-end" onSubmit={submit}>
        <Field label="Nazwa">
          <Input required maxLength={200} value={draft.title} onChange={(e) => setDraft((d) => ({ ...d, title: e.target.value }))} placeholder="np. Prezentacja: locja" />
        </Field>
        <Field label="Link (opcjonalnie)">
          <Input type="url" maxLength={1000} value={draft.url} onChange={(e) => setDraft((d) => ({ ...d, url: e.target.value }))} placeholder="https://" />
        </Field>
        <NautivoButton type="submit" variant="secondary" icon={Plus} loading={busy}>
          Dodaj
        </NautivoButton>
      </form>
      <p className="mt-2 text-xs text-muted-foreground">Pliki trzymaj na swoim dysku (np. Google Drive) i wklej link. Aplikacja nie przechowuje plików.</p>
      <FormError message={error} />
    </Panel>
  );
}

/* ------------------------------------------------------------ Przegląd */

function Panel({ title, icon: Icon, action, children }: { title: string; icon?: typeof BookOpen; action?: ReactNode; children: ReactNode }) {
  return (
    <section className="card p-5">
      <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
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

function Tile({ label, value, note, progress, tone = "bg-primary" }: { label: string; value: string; note?: string; progress?: number; tone?: string }) {
  return (
    <div className="card p-4">
      <div className="text-sm text-muted-foreground">{label}</div>
      <div className="mt-1 text-2xl font-bold tabular-nums">{value}</div>
      {progress != null && (
        <div className="mt-2 h-1.5 rounded-full bg-muted">
          <div className={cn("h-1.5 rounded-full", tone)} style={{ width: `${Math.min(100, progress)}%` }} />
        </div>
      )}
      {note && <div className="mt-1 text-xs text-muted-foreground">{note}</div>}
    </div>
  );
}

/** Statystyki kursu: postęp, godziny, frekwencja, testy, zdawalność. */
function Overview({ detail }: { detail: CourseDetail }) {
  const { sessions, trainees, tests } = detail;
  const done = sessions.filter((s) => s.completedAt);
  const plannedHours = sessions.reduce((sum, s) => sum + hours(s), 0);
  const doneHours = done.reduce((sum, s) => sum + hours(s), 0);
  const possible = trainees.length * done.length;
  const present = trainees.reduce((sum, t) => sum + t.attendedSessions.filter((id) => done.some((s) => s.id === id)).length, 0);
  const allScores = tests.flatMap((t) => t.results.map((r) => r.score / t.maxScore));
  const decided = trainees.filter((t) => t.examResult === "Passed" || t.examResult === "Failed");
  const passed = trainees.filter((t) => t.examResult === "Passed").length;

  return (
    <div className="grid gap-6">
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
        <Tile label="Postęp kursu" value={`${percent(done.length, sessions.length)}%`} progress={percent(done.length, sessions.length)} note={sessions.length ? `${done.length} z ${sessions.length} zajęć${plannedHours ? `, ${hoursLabel(doneHours)} z ${hoursLabel(plannedHours)}` : ""}` : "Brak harmonogramu"} tone="bg-paid" />
        <Tile label="Średnia frekwencja" value={possible ? `${percent(present, possible)}%` : "–"} progress={possible ? percent(present, possible) : undefined} note={possible ? "na odbytych zajęciach" : "po pierwszych zajęciach"} />
        <Tile label="Średni wynik testów" value={allScores.length ? `${Math.round((allScores.reduce((a, b) => a + b, 0) / allScores.length) * 100)}%` : "–"} note={`${plural(tests.length, "test", "testy", "testów")}`} />
        <Tile label="Zdawalność egzaminu" value={decided.length ? `${percent(passed, decided.length)}%` : "–"} note={decided.length ? `zdało ${passed} z ${decided.length}` : "przed egzaminem"} tone="bg-paid" progress={decided.length ? percent(passed, decided.length) : undefined} />
      </div>
      {done.length > 0 && trainees.length > 0 && (
        <Panel title="Obecność na kolejnych zajęciach">
          <div className="flex h-40 items-end gap-2" role="img" aria-label={done.map((s) => `${fullDay(s.date)}: ${percent(trainees.filter((t) => t.attendedSessions.includes(s.id)).length, trainees.length)}%`).join(", ")}>
            {done.map((s) => {
              const value = percent(trainees.filter((t) => t.attendedSessions.includes(s.id)).length, trainees.length);
              return (
                <div key={s.id} className="flex min-w-0 flex-1 flex-col items-center gap-1" title={`${s.topic}: ${value}%`}>
                  <span className="text-xs tabular-nums">{value}%</span>
                  <div className="w-full max-w-12 rounded-t-md bg-primary" style={{ height: `${Math.max(value, 2) * 1.1}px` }} />
                  <span className="w-full truncate text-center text-[11px] text-muted-foreground">{day(s.date)}</span>
                </div>
              );
            })}
          </div>
        </Panel>
      )}
    </div>
  );
}

/* ----------------------------------------------------------- Szczegóły */

type Tab = "overview" | "trainees" | "instructor" | "finance";
const TABS: [Tab, string][] = [
  ["overview", "Przegląd"],
  ["trainees", "Kursanci"],
  ["instructor", "Panel prowadzącego"],
  ["finance", "Finanse"],
];

export function CourseDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { data, error } = useQuery({ queryKey: ["course", id], queryFn: () => api<CourseDetail>(`/courses/${id}`) });
  const [tab, setTab] = useState<Tab>("overview");
  const [editing, setEditing] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [dialog, setDialog] = useState<Trainee | "new" | null>(null);
  const [progress, setProgress] = useState<Trainee | null>(null);
  const [paying, setPaying] = useState<string | null>(null);
  const [removing, setRemoving] = useState<Trainee | null>(null);
  const [finishing, setFinishing] = useState(false);
  const [completeSessions, setCompleteSessions] = useState(true);
  const finish = useAction(
    (body: { completed: boolean; completeSessions: boolean }) => api(`/courses/${id}/complete`, { method: "PUT", body }),
    ({ completed }) => (completed ? "Kurs zakończony" : "Kurs wznowiony"),
  );
  const removeTrainee = useAction((enrollmentId: string) => api(`/enrollments/${enrollmentId}`, { method: "DELETE" }), "Usunięto kursanta ze szkolenia");

  if (!data) {
    return error ? <EmptyState title="Nie można otworzyć szkolenia" text={error.message} action={<NautivoButton variant="secondary" onClick={() => navigate("/szkolenia")}>Wróć do szkoleń</NautivoButton>} /> : <LoadState />;
  }

  const { course, trainees, requiredDocuments, sessions } = data;
  const crew = trainees.map((t) => t.enrollment);
  const full = trainees.length >= course.capacity;
  const cancelled = course.status === "Cancelled";
  const paymentTarget = crew.find((e) => e.id === paying);
  const costPerTrainee = trainees.length ? course.costs / trainees.length : 0;
  const done = sessions.filter((s) => s.completedAt).length;
  const completed = !!course.completedAt;
  const openSessions = sessions.length - done;

  const columns: NautivoColumn<Trainee>[] = [
    {
      key: "name",
      header: "Kursant",
      cell: (t) => (
        <>
          <Link to={`/uczestnicy/${t.enrollment.participantId}`} className="font-medium hover:underline">
            {t.enrollment.fullName}
          </Link>
          <div className="text-xs text-muted-foreground">{[t.enrollment.email, t.enrollment.phone].filter(Boolean).join(", ")}</div>
        </>
      ),
    },
    { key: "pay", header: "Płatność", cell: (t) => <StatusPill state={t.enrollment.state} /> },
    { key: "paid", header: "Wpłacono", hideBelow: "md", cell: (t) => <PaidBar paid={t.enrollment.paid} total={t.enrollment.total} /> },
    {
      key: "docs",
      header: "Dokumenty",
      cell: (t) =>
        requiredDocuments.length ? (
          <span className={cn("tabular-nums", t.documentsDelivered.length === requiredDocuments.length ? "text-paid" : "text-part")} title={requiredDocuments.filter((d) => !t.documentsDelivered.includes(d)).join(", ") || "Komplet"}>
            {t.documentsDelivered.length}/{requiredDocuments.length}
          </span>
        ) : (
          <span className="text-muted-foreground">–</span>
        ),
    },
    { key: "attendance", header: "Obecność", hideBelow: "sm", cell: (t) => <span className="tabular-nums">{done ? `${percent(t.attendedSessions.filter((s) => sessions.some((x) => x.id === s && x.completedAt)).length, done)}%` : "–"}</span> },
    {
      key: "exam",
      header: "Egzamin",
      cell: (t) => (
        <>
          <Pill className={EXAM[t.examResult].className}>{EXAM[t.examResult].label}</Pill>
          {t.certificateNumber && <div className="mt-0.5 text-xs text-muted-foreground">{t.certificateNumber}</div>}
        </>
      ),
    },
    {
      key: "actions",
      header: "Działania",
      srOnlyHeader: true,
      align: "right",
      cell: (t) => (
        <NautivoRowActions>
          <NautivoButton size="sm" variant="secondary" onClick={() => setProgress(t)}>
            Postęp
          </NautivoButton>
          <NautivoButton size="sm" variant="secondary" onClick={() => setPaying(t.enrollment.id)}>
            Wpłaty
          </NautivoButton>
          <NautivoIconButton action="edit" label={t.enrollment.fullName} onClick={() => setDialog(t)} />
          <NautivoIconButton action="delete" label={`${t.enrollment.fullName} ze szkolenia`} onClick={() => setRemoving(t)} />
        </NautivoRowActions>
      ),
    },
  ];

  return (
    <>
      <PageHead
        back={<NautivoBackButton fallback="/szkolenia" label="Wróć do szkoleń" />}
        title={course.name}
        subtitle={`${COURSE_TYPE[course.type].label} · ${range(course.startDate, course.endDate)}${course.location ? ` · ${course.location}` : ""}${cancelled ? " · odwołane" : ""}${completed ? ` · zakończone ${fullDay(course.completedAt!.slice(0, 10))}` : ""}`}
        actions={
          <>
            {!cancelled &&
              (completed ? (
                <NautivoButton variant="secondary" icon={RotateCcw} loading={finish.isPending} onClick={() => finish.mutate({ completed: false, completeSessions: false })}>
                  Wznów kurs
                </NautivoButton>
              ) : (
                <NautivoButton icon={Flag} onClick={() => setFinishing(true)}>
                  Zakończ kurs
                </NautivoButton>
              ))}
            <NautivoButton variant="secondary" icon={Pencil} onClick={() => setEditing(true)}>
              Edytuj
            </NautivoButton>
            <NautivoButton variant="danger" icon={Trash2} onClick={() => setDeleting(true)}>
              Usuń
            </NautivoButton>
          </>
        }
      />

      <dl className="card mb-5 grid grid-cols-2 gap-x-8 gap-y-4 p-5 md:grid-cols-3 lg:grid-cols-6">
        <Info label="Rodzaj">
          <CourseBadge type={course.type} />
        </Info>
        <Info label="Kursanci">
          {course.confirmed} z {course.capacity}
        </Info>
        <Info label="Cena za kursanta">{zl(course.price)}</Info>
        <Info label="Prowadzący">{course.instructor ?? <span className="font-normal text-muted-foreground">nie podano</span>}</Info>
        <Info label="Egzamin">{course.examDate ? fullDay(course.examDate) : <span className="font-normal text-muted-foreground">bez daty</span>}</Info>
        <Info label="Zysk (obecne zapisy)">
          <span className={course.expected - course.costs >= 0 ? "text-paid" : "text-late"}>{signed(course.expected - course.costs)}</span>
        </Info>
      </dl>
      {data.examInfo && <p className="mb-5 text-sm text-muted-foreground">Egzamin: {data.examInfo}</p>}

      <div className="mb-5 flex flex-wrap gap-1 rounded-xl bg-muted p-1" role="tablist" aria-label="Widok szkolenia">
        {TABS.map(([key, label]) => (
          <button key={key} type="button" role="tab" aria-selected={tab === key} onClick={() => setTab(key)} className={cn("rounded-lg px-4 py-1.5 text-sm font-medium transition-colors", tab === key ? "bg-white text-sea shadow-sm" : "text-muted-foreground hover:text-foreground")}>
            {label}
          </button>
        ))}
      </div>

      {tab === "overview" && (
        <div className="grid gap-6">
          <Overview detail={data} />
          <div className="grid gap-6 lg:grid-cols-2">
            <FinanceChart cruise={course} title="Finanse szkolenia" />
            <PaymentStatusChart crew={crew} onPay={setPaying} title="Płatności kursantów" noun="kursantów" />
          </div>
        </div>
      )}

      {tab === "trainees" && (
        <section aria-label="Kursanci">
          <div className="mb-2 flex flex-wrap items-center justify-end gap-3">
            {full && !completed && <span className="text-sm text-muted-foreground">Komplet kursantów</span>}
            {completed && <span className="text-sm text-muted-foreground">Kurs zakończony. Wznów go, aby dopisać kursanta.</span>}
            {!cancelled && (
              <NautivoButton icon={Plus} disabled={full || completed} onClick={() => setDialog("new")}>
                Dodaj kursanta
              </NautivoButton>
            )}
          </div>
          {trainees.length === 0 ? <EmptyState title="Nikt się jeszcze nie zapisał" text="Dodaj pierwszego kursanta albo wybierz go z bazy klientów." /> : <NautivoTable columns={columns} rows={trainees} rowKey={(t) => t.enrollment.id} caption="Kursanci" minWidth={900} />}
        </section>
      )}

      {tab === "instructor" && (
        <div className="grid gap-6">
          <div className="grid gap-6 xl:grid-cols-2">
            <Schedule detail={data} />
            <Materials detail={data} />
          </div>
          <Attendance detail={data} />
          <Tests detail={data} />
        </div>
      )}

      {tab === "finance" && (
        <div className="grid gap-6">
          <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
            <Tile label="Cena za kursanta" value={zl(course.price)} note={`przy ${plural(course.capacity, "miejscu", "miejscach", "miejscach")}: ${zl(course.price * course.capacity)}`} />
            <Tile label="Przychód z zapisów" value={zl(course.expected)} note={`wpłacono ${zl(course.collected)}`} />
            <Tile label="Koszt na kursanta" value={trainees.length ? zl(Math.round(costPerTrainee * 100) / 100) : "–"} note={`koszty razem ${zl(course.costs)}`} />
            <Tile label="Zysk na kursanta" value={trainees.length ? signed(Math.round((course.expected / trainees.length - costPerTrainee) * 100) / 100) : "–"} note={`zysk razem ${signed(course.expected - course.costs)}`} />
          </div>
          <FinanceChart cruise={course} title="Finanse szkolenia" />
          <CostsTable addPath={`/courses/${course.id}/costs`} headcount={trainees.length} costs={data.costs} title="Koszty szkolenia" personLabel="kursanta" categories={COURSE_COSTS} />
        </div>
      )}

      {editing && <CourseForm courseId={course.id} onClose={() => setEditing(false)} />}
      {dialog && <EnrollmentDialog enrollPath={`/courses/${course.id}/enrollments`} price={course.price} noun="kursanta" member={dialog === "new" ? undefined : dialog.enrollment} onClose={() => setDialog(null)} />}
      {progress && <ProgressDialog key={progress.enrollment.id} trainee={progress} documents={requiredDocuments} onClose={() => setProgress(null)} />}
      {paymentTarget && <PaymentDialog key={paymentTarget.id} enrollment={paymentTarget} onClose={() => setPaying(null)} />}
      <ConfirmDialog
        open={!!removing}
        title={`Usunąć ze szkolenia: ${removing?.enrollment.fullName ?? ""}?`}
        text={removing && removing.enrollment.paid > 0 ? `Razem z zapisem znikną wpłaty (${zl(removing.enrollment.paid)}), obecności i wyniki testów. Osoba zostanie w bazie klientów.` : "Znikną obecności i wyniki testów. Osoba zostanie w bazie klientów."}
        confirmLabel="Usuń ze szkolenia"
        onConfirm={async () => {
          await removeTrainee.mutateAsync(removing!.enrollment.id);
        }}
        onClose={() => setRemoving(null)}
      />
      <Dialog open={finishing} onOpenChange={(o) => !o && setFinishing(false)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Zakończyć kurs „{course.name}”?</DialogTitle>
            <DialogDescription>Zakończony kurs trafi do zakończonych i nie przyjmie nowych zapisów. Wyniki egzaminów, wpłaty i koszty możesz uzupełniać dalej. Kurs można wznowić.</DialogDescription>
          </DialogHeader>
          {openSessions > 0 && (
            <label className="flex items-start gap-3 rounded-[10px] border p-3 text-sm">
              <input type="checkbox" className="mt-0.5 size-4" checked={completeSessions} onChange={(e) => setCompleteSessions(e.target.checked)} />
              <span>
                Oznacz pozostałe zajęcia jako ukończone ({openSessions})
                <span className="block text-xs text-muted-foreground">Odznacz, jeśli część zajęć się nie odbyła.</span>
              </span>
            </label>
          )}
          <FormError message={finish.error?.message ?? null} />
          <DialogFooter>
            <NautivoButton type="button" variant="secondary" onClick={() => setFinishing(false)}>
              Anuluj
            </NautivoButton>
            <NautivoButton
              icon={Flag}
              loading={finish.isPending}
              onClick={async () => {
                await finish.mutateAsync({ completed: true, completeSessions: openSessions > 0 && completeSessions });
                setFinishing(false);
              }}
            >
              Zakończ kurs
            </NautivoButton>
          </DialogFooter>
        </DialogContent>
      </Dialog>
      <DeleteCourseDialog course={deleting ? course : null} onClose={() => setDeleting(false)} onDeleted={() => navigate("/szkolenia")} />
    </>
  );
}

function Info({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div>
      <dt className="text-sm text-muted-foreground">{label}</dt>
      <dd className="mt-0.5 font-semibold">{children}</dd>
    </div>
  );
}
