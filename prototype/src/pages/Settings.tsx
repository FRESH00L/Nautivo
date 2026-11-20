import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { api, type OrganizationSettings } from "@/api";
import { useSession } from "@/auth";
import { Field, FormError, LoadState, PageHead, useFormSubmit } from "@/components/shared";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

export function SettingsPage() {
  const { organization } = useSession();
  const { data, error } = useQuery({ queryKey: ["organization", organization?.id], queryFn: () => api<OrganizationSettings>("/organization") });
  return (
    <>
      <PageHead title="Ustawienia firmy" subtitle="Dane Twojej firmy widoczne w panelu." />
      {data ? <CompanyForm key={data.id} settings={data} /> : <LoadState error={error} />}
    </>
  );
}

function CompanyForm({ settings }: { settings: OrganizationSettings }) {
  const client = useQueryClient();
  const { me } = useSession();
  const [form, setForm] = useState({
    name: settings.name,
    contactEmail: settings.contactEmail ?? "",
    phone: settings.phone ?? "",
    city: settings.city ?? "",
    taxId: settings.taxId ?? "",
    website: settings.website ?? "",
  });
  const set = (key: keyof typeof form) => (e: { target: { value: string } }) => setForm((f) => ({ ...f, [key]: e.target.value }));
  const { submit, error, busy } = useFormSubmit(async () => {
    await api("/organization", { method: "PUT", body: { ...form, contactEmail: form.contactEmail || null } });
    await client.invalidateQueries();
    toast.success("Zapisano ustawienia firmy");
  });
  const readOnly = !settings.canEdit;

  return (
    <div className="grid gap-4 lg:grid-cols-[minmax(0,2fr)_minmax(0,1fr)]">
      <form className="card grid gap-4 p-6" onSubmit={submit}>
        <h2 className="text-lg">Dane firmy</h2>
        {readOnly && <p className="rounded-lg bg-part-soft p-3 text-sm text-part">Ustawienia może zmieniać tylko właściciel organizacji.</p>}
        <Field label="Nazwa firmy">
          <Input required disabled={readOnly} value={form.name} onChange={set("name")} />
        </Field>
        <div className="grid gap-4 sm:grid-cols-2">
          <Field label="E-mail kontaktowy">
            <Input type="email" disabled={readOnly} value={form.contactEmail} onChange={set("contactEmail")} />
          </Field>
          <Field label="Telefon">
            <Input type="tel" disabled={readOnly} value={form.phone} onChange={set("phone")} />
          </Field>
          <Field label="Miejscowość">
            <Input disabled={readOnly} value={form.city} onChange={set("city")} />
          </Field>
          <Field label="NIP">
            <Input disabled={readOnly} value={form.taxId} onChange={set("taxId")} />
          </Field>
        </div>
        <Field label="Strona internetowa">
          <Input type="url" disabled={readOnly} placeholder="https://" value={form.website} onChange={set("website")} />
        </Field>
        <FormError message={error} />
        {!readOnly && (
          <div>
            <Button type="submit" disabled={busy}>
              Zapisz ustawienia
            </Button>
          </div>
        )}
      </form>
      <aside className="card p-6 lg:self-start">
        <h2 className="text-lg">Twoje konto</h2>
        <dl className="mt-3 grid gap-3 text-sm">
          <div>
            <dt className="text-muted-foreground">Imię i nazwisko</dt>
            <dd className="font-medium">{me.fullName}</dd>
          </div>
          <div>
            <dt className="text-muted-foreground">E-mail</dt>
            <dd className="font-medium break-all">{me.email}</dd>
          </div>
          <div>
            <dt className="text-muted-foreground">Rola</dt>
            <dd className="font-medium">{me.isAdmin ? "Administrator" : "Użytkownik organizacji"}</dd>
          </div>
        </dl>
      </aside>
    </div>
  );
}
