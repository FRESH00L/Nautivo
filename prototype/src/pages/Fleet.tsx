import { useQuery } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import { useState } from "react";
import { api, type Vessel, type VesselKind } from "@/api";
import { addDays, ConfirmDialog, EmptyState, Field, FormError, fullDay, LoadState, PageHead, plural, range, selectClass, today, useAction, useFormSubmit, zl } from "@/components/shared";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { cn } from "@/lib/utils";

export function FleetPage() {
  const { data, error } = useQuery({ queryKey: ["vessels"], queryFn: () => api<Vessel[]>("/vessels") });
  const [editing, setEditing] = useState<Vessel | "new" | null>(null);
  const [blockingId, setBlockingId] = useState<string | null>(null);
  const [deleting, setDeleting] = useState<Vessel | null>(null);
  const remove = useAction((id: string) => api(`/vessels/${id}`, { method: "DELETE" }), "Usunięto jednostkę");
  const removeBlock = useAction(({ vesselId, blockId }: { vesselId: string; blockId: string }) => api(`/vessels/${vesselId}/blocks/${blockId}`, { method: "DELETE" }), "Usunięto blokadę");
  const soon = addDays(today(), 30);
  const blocking = data?.find((v) => v.id === blockingId);
  const addButton = (
    <Button onClick={() => setEditing("new")}>
      <Plus /> Dodaj jednostkę
    </Button>
  );

  return (
    <>
      <PageHead title="Flota" subtitle="Jednostki, przeglądy i okresy niedostępności, np. serwis." actions={addButton} />
      {!data && <LoadState error={error} />}
      {data?.length === 0 && <EmptyState title="Nie ma jeszcze jednostek" text="Dodaj jacht lub łódź, aby przypisywać je do rejsów." action={addButton} />}
      {data && data.length > 0 && (
        <ul className="grid gap-4 lg:grid-cols-2">
          {data.map((v) => (
            <li key={v.id} className="card p-4">
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <h2 className="text-xl">{v.name}</h2>
                  <p className="text-sm text-muted-foreground">
                    {[KIND_LABEL[v.kind], v.model, v.lengthMeters ? `${v.lengthMeters.toLocaleString("pl-PL")} m` : null, `${v.berths} miejsc`, v.registrationNumber].filter(Boolean).join(", ")}
                  </p>
                </div>
                <span className={cn("rounded-full px-2.5 py-0.5 text-xs font-semibold", v.status === "InService" ? "bg-paid-soft text-paid" : "bg-late-soft text-late")}>{v.status === "InService" ? "Dostępna" : "Niedostępna"}</span>
              </div>
              <dl className="mt-3 grid grid-cols-2 gap-2 text-sm">
                <div>
                  <dt className="text-muted-foreground">Przegląd do</dt>
                  <dd className={cn(v.inspectionDue && v.inspectionDue <= soon && "font-medium text-late")}>{v.inspectionDue ? fullDay(v.inspectionDue) : "Nie podano"}</dd>
                </div>
                <div>
                  <dt className="text-muted-foreground">Nadchodzące rejsy</dt>
                  <dd>{v.upcomingCruises}</dd>
                </div>
                {v.charterDayRate !== null && (
                  <div>
                    <dt className="text-muted-foreground">Czarter</dt>
                    <dd>
                      {zl(v.charterDayRate)} / dzień, {plural(v.upcomingCharters, "rezerwacja", "rezerwacje", "rezerwacji")}
                    </dd>
                  </div>
                )}
              </dl>
              {v.notes && <p className="mt-2 text-sm">{v.notes}</p>}
              {v.blocks.length > 0 && (
                <ul className="mt-3 divide-y border-t text-sm">
                  {v.blocks.map((b) => (
                    <li key={b.id} className="flex items-center justify-between gap-2 py-1.5">
                      <span>
                        {range(b.startDate, b.endDate)}: {b.reason}
                      </span>
                      <Button size="xs" variant="ghost" disabled={removeBlock.isPending} onClick={() => removeBlock.mutate({ vesselId: v.id, blockId: b.id })}>
                        Usuń
                      </Button>
                    </li>
                  ))}
                </ul>
              )}
              <div className="mt-3 flex flex-wrap gap-2">
                <Button size="sm" variant="outline" onClick={() => setEditing(v)}>
                  Edytuj
                </Button>
                <Button size="sm" variant="outline" onClick={() => setBlockingId(v.id)}>
                  Zablokuj termin
                </Button>
                <Button size="sm" variant="ghost" onClick={() => setDeleting(v)}>
                  Usuń
                </Button>
              </div>
            </li>
          ))}
        </ul>
      )}
      {editing && <VesselForm vessel={editing === "new" ? undefined : editing} onClose={() => setEditing(null)} />}
      {blocking && <BlockForm vessel={blocking} onClose={() => setBlockingId(null)} />}
      <ConfirmDialog
        open={!!deleting}
        title={`Usunąć jednostkę: ${deleting?.name ?? ""}?`}
        text="Jednostki z przypisanymi rejsami lub rezerwacjami czarterowymi nie można usunąć."
        confirmLabel="Usuń jednostkę"
        onConfirm={async () => {
          await remove.mutateAsync(deleting!.id);
        }}
        onClose={() => setDeleting(null)}
      />
    </>
  );
}

export const KIND_LABEL: Record<VesselKind, string> = {
  Yacht: "Jacht",
  Sailboat: "Łódź żaglowa",
  Motorboat: "Motorówka",
  Jetski: "Skuter wodny",
  Kayak: "Kajak / SUP",
  Other: "Inna",
};

/** Formularz jednostki wspólny dla floty i czarterów. W czarterach cena za dzień jest wymagana. */
export function VesselForm({ vessel, onClose, forCharter = false }: { vessel?: Vessel; onClose: () => void; forCharter?: boolean }) {
  const [form, setForm] = useState({
    name: vessel?.name ?? "",
    kind: vessel?.kind ?? (forCharter ? "Motorboat" : "Yacht"),
    charterDayRate: vessel?.charterDayRate != null ? String(vessel.charterDayRate) : "",
    model: vessel?.model ?? "",
    lengthMeters: String(vessel?.lengthMeters ?? ""),
    berths: String(vessel?.berths ?? (forCharter ? 4 : 8)),
    registrationNumber: vessel?.registrationNumber ?? "",
    inspectionDue: vessel?.inspectionDue ?? "",
    status: vessel?.status ?? "InService",
    notes: vessel?.notes ?? "",
  });
  const set = (key: keyof typeof form) => (e: { target: { value: string } }) => setForm((f) => ({ ...f, [key]: e.target.value }));
  const save = useAction(
    () => {
      const body = {
        ...form,
        lengthMeters: Number(form.lengthMeters || 0),
        berths: Number(form.berths),
        inspectionDue: form.inspectionDue || null,
        registrationNumber: form.registrationNumber || null,
        notes: form.notes || null,
        charterDayRate: form.charterDayRate === "" ? null : Number(form.charterDayRate),
      };
      return vessel ? api(`/vessels/${vessel.id}`, { method: "PUT", body }) : api("/vessels", { method: "POST", body });
    },
    vessel ? "Zapisano zmiany w jednostce" : `Dodano jednostkę: ${form.name}`,
  );
  const { submit, error, busy } = useFormSubmit(async () => {
    await save.mutateAsync(undefined);
    onClose();
  });
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="sm:max-w-lg">
        <form className="grid gap-4" onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>{vessel ? "Edytuj jednostkę" : "Dodaj jednostkę"}</DialogTitle>
          </DialogHeader>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Nazwa">
              <Input autoFocus required maxLength={100} value={form.name} onChange={set("name")} />
            </Field>
            <Field label="Rodzaj">
              <select className={selectClass} value={form.kind} onChange={set("kind")}>
                {Object.entries(KIND_LABEL).map(([value, label]) => (
                  <option key={value} value={value}>
                    {label}
                  </option>
                ))}
              </select>
            </Field>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Model">
              <Input maxLength={100} value={form.model} onChange={set("model")} placeholder="np. Delphia 40" />
            </Field>
            <Field label="Cena czarteru za dzień (zł)" hint={forCharter ? undefined : "Puste: jednostka nie jest wynajmowana."}>
              <Input type="number" min={0} step={0.01} required={forCharter} value={form.charterDayRate} onChange={set("charterDayRate")} />
            </Field>
          </div>
          <div className="grid grid-cols-3 gap-3">
            <Field label="Długość (m)">
              <Input type="number" min={0} step={0.01} value={form.lengthMeters} onChange={set("lengthMeters")} />
            </Field>
            <Field label="Miejsca">
              <Input type="number" min={0} required value={form.berths} onChange={set("berths")} />
            </Field>
            <Field label="Rejestracja">
              <Input value={form.registrationNumber} onChange={set("registrationNumber")} />
            </Field>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Przegląd do">
              <Input type="date" value={form.inspectionDue} onChange={set("inspectionDue")} />
            </Field>
            <Field label="Status">
              <select className={selectClass} value={form.status} onChange={set("status")}>
                <option value="InService">Dostępna</option>
                <option value="OutOfService">Niedostępna</option>
              </select>
            </Field>
          </div>
          <Field label="Notatki">
            <Textarea value={form.notes} onChange={set("notes")} placeholder="np. wyposażenie lub ustalenia serwisowe" />
          </Field>
          <FormError message={error} />
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              Anuluj
            </Button>
            <Button type="submit" disabled={busy}>
              {vessel ? "Zapisz zmiany" : "Dodaj jednostkę"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function BlockForm({ vessel, onClose }: { vessel: Vessel; onClose: () => void }) {
  const [form, setForm] = useState({ startDate: today(), endDate: addDays(today(), 7), reason: "" });
  const add = useAction(() => api(`/vessels/${vessel.id}/blocks`, { method: "POST", body: form }), `Zablokowano termin: ${vessel.name}`);
  const { submit, error, busy } = useFormSubmit(async () => {
    await add.mutateAsync(undefined);
    onClose();
  });
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <form className="grid gap-4" onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>Zablokuj termin: {vessel.name}</DialogTitle>
            <DialogDescription>W tym okresie nie da się przypisać jednostki do rejsu.</DialogDescription>
          </DialogHeader>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Od">
              <Input type="date" required value={form.startDate} onChange={(e) => setForm({ ...form, startDate: e.target.value })} />
            </Field>
            <Field label="Do">
              <Input type="date" required value={form.endDate} onChange={(e) => setForm({ ...form, endDate: e.target.value })} />
            </Field>
          </div>
          <Field label="Powód">
            <Input autoFocus required value={form.reason} onChange={(e) => setForm({ ...form, reason: e.target.value })} placeholder="np. Serwis silnika" />
          </Field>
          <FormError message={error} />
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              Anuluj
            </Button>
            <Button type="submit" disabled={busy}>
              Zablokuj termin
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
