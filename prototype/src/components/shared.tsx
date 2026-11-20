import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState, type FormEvent, type ReactNode } from "react";
import { toast } from "sonner";
import { api, type Enrollment, type PaymentState } from "@/api";
import { NautivoButton, NautivoIconButton } from "@/components/nautivo";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";

const money = new Intl.NumberFormat("pl-PL", { maximumFractionDigits: 2 });
export const zl = (n: number) => `${money.format(n)} zł`;

const dayFormat = new Intl.DateTimeFormat("pl-PL", { day: "numeric", month: "short", timeZone: "UTC" });
const fullFormat = new Intl.DateTimeFormat("pl-PL", { day: "numeric", month: "short", year: "numeric", timeZone: "UTC" });
export const day = (iso: string) => dayFormat.format(new Date(`${iso}T12:00:00Z`));
export const fullDay = (iso: string) => fullFormat.format(new Date(`${iso}T12:00:00Z`));
export const range = (a: string, b: string) => `${day(a)} – ${fullDay(b)}`;

const pad = (n: number) => String(n).padStart(2, "0");
export const today = () => {
  const now = new Date();
  return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;
};
export function addDays(iso: string, days: number) {
  const d = new Date(`${iso}T12:00:00Z`);
  d.setUTCDate(d.getUTCDate() + days);
  return d.toISOString().slice(0, 10);
}

export function plural(n: number, one: string, few: string, many: string) {
  if (n === 1) return `1 ${one}`;
  const last = n % 10;
  const tens = n % 100;
  return `${n} ${last >= 2 && last <= 4 && (tens < 12 || tens > 14) ? few : many}`;
}

export const STATE: Record<PaymentState, { label: string; className: string; bar: string; stroke: string }> = {
  Paid: { label: "Zapłacone", className: "bg-paid-soft text-paid", bar: "bg-paid", stroke: "stroke-paid" },
  Partial: { label: "Zaliczka", className: "bg-part-soft text-part", bar: "bg-part", stroke: "stroke-part" },
  None: { label: "Nieopłacone", className: "bg-muted text-muted-foreground", bar: "bg-muted-foreground/40", stroke: "stroke-muted-foreground/40" },
  Overdue: { label: "Zaległe", className: "bg-late-soft text-late", bar: "bg-late", stroke: "stroke-late" },
  Overpaid: { label: "Nadpłata", className: "bg-accent text-primary", bar: "bg-primary", stroke: "stroke-primary" },
};

export function StatusPill({ state }: { state: PaymentState }) {
  return <span className={cn("inline-block rounded-full px-2.5 py-0.5 text-xs font-semibold whitespace-nowrap", STATE[state].className)}>{STATE[state].label}</span>;
}

export function PageHead({ title, subtitle, actions, back }: { title: string; subtitle?: string; actions?: ReactNode; back?: ReactNode }) {
  return (
    <div className="mb-5 flex flex-wrap items-end justify-between gap-3">
      <div className="flex items-start gap-2">
        {back && <div className="-ml-2 pt-0.5">{back}</div>}
        <div>
          <h1 className="text-3xl">{title}</h1>
          {subtitle && <p className="mt-1 max-w-2xl text-muted-foreground">{subtitle}</p>}
        </div>
      </div>
      {actions && <div className="flex flex-wrap gap-2">{actions}</div>}
    </div>
  );
}

export function Field({ label, children, hint }: { label: string; children: ReactNode; hint?: string }) {
  return (
    <div className="grid gap-1.5">
      {/* Pole wewnątrz etykiety: kliknięcie i czytnik ekranu kojarzą je bez ręcznego id. */}
      <Label className="grid gap-1.5">
        <span>{label}</span>
        {children}
      </Label>
      {hint && <p className="text-xs text-muted-foreground">{hint}</p>}
    </div>
  );
}

export const selectClass = "h-10 w-full rounded-[10px] border border-input bg-card px-3 text-sm";

export function EmptyState({ title, text, action }: { title: string; text: string; action?: ReactNode }) {
  return (
    <div className="rounded-xl border border-dashed border-input bg-card p-10 text-center">
      <h2 className="text-lg">{title}</h2>
      <p className="mx-auto mt-1 max-w-md text-muted-foreground">{text}</p>
      {action && <div className="mt-4">{action}</div>}
    </div>
  );
}

export function LoadState({ error, text = "Wczytuję dane…" }: { error?: Error | null; text?: string }) {
  if (error) {
    return (
      <p className="rounded-sm bg-late-soft p-3 text-late" role="alert">
        {error.message}
      </p>
    );
  }
  return <p className="text-muted-foreground">{text}</p>;
}

/** Zmiana danych: po sukcesie odświeża wszystko poza profilem i pokazuje komunikat. */
export function useAction<V>(run: (variables: V) => Promise<unknown>, success?: string | ((variables: V) => string)) {
  const client = useQueryClient();
  return useMutation({
    mutationFn: run,
    onSuccess: async (_data, variables) => {
      await client.invalidateQueries({ predicate: (q) => q.queryKey[0] !== "me" });
      if (success) toast.success(typeof success === "function" ? success(variables) : success);
    },
  });
}

/** Wysyłka formularza z błędem pokazanym przy formularzu zamiast w dymku, który szybko znika. */
export function useFormSubmit(action: () => Promise<void>) {
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const submit = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await action();
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udało się zapisać.");
    } finally {
      setBusy(false);
    }
  };
  return { submit, error, busy };
}

export function FormError({ message }: { message: string | null }) {
  if (!message) return null;
  return (
    <p className="rounded-sm bg-late-soft p-2 text-sm text-late" role="alert">
      {message}
    </p>
  );
}

export function ConfirmDialog({
  open,
  title,
  text,
  confirmLabel,
  onConfirm,
  onClose,
}: {
  open: boolean;
  title: string;
  text: string;
  confirmLabel: string;
  onConfirm: () => Promise<void>;
  onClose: () => void;
}) {
  const { submit, error, busy } = useFormSubmit(async () => {
    await onConfirm();
    onClose();
  });
  return (
    <Dialog open={open} onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <form className="grid gap-4" onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>{title}</DialogTitle>
            <DialogDescription>{text}</DialogDescription>
          </DialogHeader>
          <FormError message={error} />
          <DialogFooter>
            <NautivoButton type="button" variant="secondary" onClick={onClose}>
              Wróć
            </NautivoButton>
            <NautivoButton type="submit" variant="danger" loading={busy}>
              {confirmLabel}
            </NautivoButton>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

export function PaidBar({ paid, total }: { paid: number; total: number }) {
  const percent = total ? Math.min(100, Math.round((paid / total) * 100)) : 0;
  return (
    <div className="min-w-28">
      <div className="text-sm">
        {zl(paid)} <span className="text-muted-foreground">z {zl(total)}</span>
      </div>
      <div className="mt-1 h-1 rounded-full bg-muted" role="presentation">
        <div className={cn("h-1 rounded-full", percent === 100 ? "bg-paid" : "bg-primary")} style={{ width: `${percent}%` }} />
      </div>
    </div>
  );
}

export function PaymentDialog({ enrollment, onClose }: { enrollment: Enrollment; onClose: () => void }) {
  const [amount, setAmount] = useState(String(enrollment.remaining || ""));
  const [paidOn, setPaidOn] = useState(today());
  const [method, setMethod] = useState("Przelew");
  const add = useAction(
    () => api(`/enrollments/${enrollment.id}/payments`, { method: "POST", body: { amount: Number(amount), paidOn, method } }),
    `Zapisano wpłatę: ${enrollment.fullName}`,
  );
  const remove = useAction((id: string) => api(`/payments/${id}`, { method: "DELETE" }), "Usunięto wpłatę");
  const { submit, error, busy } = useFormSubmit(async () => {
    await add.mutateAsync(undefined);
    onClose();
  });
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <form className="grid gap-4" onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>Wpłaty: {enrollment.fullName}</DialogTitle>
            <DialogDescription>
              Wpłacono {zl(enrollment.paid)} z {zl(enrollment.total)}
              {enrollment.paid > enrollment.total ? `, nadpłata ${zl(enrollment.paid - enrollment.total)}.` : `, pozostało ${zl(enrollment.remaining)}.`}
            </DialogDescription>
          </DialogHeader>
          {enrollment.payments.length > 0 && (
            <ul className="divide-y rounded-md border text-sm">
              {enrollment.payments.map((p) => (
                <li key={p.id} className="flex items-center justify-between gap-2 p-2">
                  <span>
                    {zl(p.amount)}, {fullDay(p.paidOn)}, {p.method}
                  </span>
                  <NautivoIconButton action="delete" size="sm" label={`wpłata ${zl(p.amount)}`} disabled={remove.isPending} onClick={() => remove.mutate(p.id)} />
                </li>
              ))}
            </ul>
          )}
          <div className="grid grid-cols-3 gap-3">
              <Field label="Kwota">
                <Input type="number" min={0.01} step={0.01} value={amount} onChange={(e) => setAmount(e.target.value)} />
              </Field>
              <Field label="Data">
                <Input type="date" max={today()} value={paidOn} onChange={(e) => setPaidOn(e.target.value)} />
              </Field>
              <Field label="Sposób">
                <select className={selectClass} value={method} onChange={(e) => setMethod(e.target.value)}>
                  {["Przelew", "Gotówka", "Karta", "BLIK"].map((x) => (
                    <option key={x}>{x}</option>
                  ))}
                </select>
              </Field>
          </div>
          {Number(amount) > enrollment.remaining && (
            <p className="rounded-sm bg-accent p-2 text-sm text-primary">Ta wpłata przekracza należność o {zl(Number(amount) - enrollment.remaining)}. Uczestnik będzie miał nadpłatę.</p>
          )}
          <FormError message={error} />
          <DialogFooter>
            <NautivoButton type="button" variant="secondary" onClick={onClose}>
              Zamknij
            </NautivoButton>
            <NautivoButton type="submit" loading={busy} disabled={!(Number(amount) > 0)}>
              Zapisz wpłatę
            </NautivoButton>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
