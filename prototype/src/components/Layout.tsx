import { useQuery } from "@tanstack/react-query";
import { AlertCircle, Anchor, Bell, Building2, CalendarDays, Check, ChevronDown, GraduationCap, LayoutDashboard, LogOut, Plus, Sailboat, Search, Settings, ShipWheel, Users, Wrench, type LucideIcon } from "lucide-react";
import { startTransition, useEffect, useRef, useState } from "react";
import { Link, NavLink, Outlet, useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { api, type CruiseItem, type Dashboard, type Me, type OrganizationInfo, type ParticipantItem } from "@/api";
import { useSession } from "@/auth";
import { Logo } from "@/components/Brand";
import { Field, FormError, fullDay, plural, range, useFormSubmit, zl } from "@/components/shared";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { DropdownMenu, DropdownMenuContent, DropdownMenuGroup, DropdownMenuItem, DropdownMenuLabel, DropdownMenuSeparator, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";

const NAV: { to: string; label: string; icon: LucideIcon; module?: string }[] = [
  { to: "/", label: "Dashboard", icon: LayoutDashboard },
  { to: "/rejsy", label: "Rejsy", icon: ShipWheel, module: "rejsy" },
  { to: "/flota", label: "Flota", icon: Sailboat, module: "flota" },
  { to: "/czartery", label: "Czartery", icon: Anchor, module: "czartery" },
  { to: "/kalendarz", label: "Kalendarz", icon: CalendarDays, module: "kalendarz" },
  { to: "/szkolenia", label: "Szkolenia", icon: GraduationCap, module: "szkolenia" },
  { to: "/uczestnicy", label: "Klienci", icon: Users, module: "uczestnicy" },
];

export function Layout() {
  const { me, organization, hasModule, signIn, selectOrganization } = useSession();
  const navigate = useNavigate();
  const [addingOrg, setAddingOrg] = useState(false);
  const items = NAV.filter((i) => !i.module || hasModule(i.module));

  return (
    <div className="flex min-h-svh">
      <aside className="sticky top-0 hidden h-svh w-60 shrink-0 flex-col bg-gradient-to-b from-sea to-[#071a3e] py-5 text-white md:flex">
        <Link to="/" className="mb-7 px-5" aria-label="Nautivo, dashboard">
          <Logo light />
        </Link>
        <nav aria-label="Główna nawigacja" className="flex flex-1 flex-col gap-1 pl-3">
          {items.map(({ to, label, icon: Icon }) => (
            <NavLink
              key={to}
              to={to}
              end={to === "/"}
              className={({ isActive }) =>
                cn("flex items-center gap-3 rounded-l-xl px-3.5 py-2.5 text-sm font-medium transition-colors", isActive ? "bg-primary text-white shadow-[0_6px_16px_-6px_rgb(11_107_222/0.8)]" : "text-[#c8d7ee] hover:bg-white/8 hover:text-white")
              }
            >
              <Icon className="size-[18px]" aria-hidden />
              {label}
            </NavLink>
          ))}
        </nav>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col pb-16 md:pb-0">
        <Topbar onAddOrganization={() => setAddingOrg(true)} />
        <main className="flex-1 px-4 py-6 md:px-8">
          <div className="mx-auto max-w-[1240px]">
            {organization ? (
              <Outlet key={organization.id} />
            ) : (
              <div className="card p-10 text-center">
                <h1 className="text-xl">Nie ma jeszcze żadnej organizacji</h1>
                <p className="mt-1 text-muted-foreground">Dodaj organizację, aby zacząć pracę z rejsami i flotą.</p>
                {me.isAdmin && (
                  <Button className="mt-4" onClick={() => setAddingOrg(true)}>
                    Dodaj organizację
                  </Button>
                )}
              </div>
            )}
          </div>
        </main>
      </div>

      <nav aria-label="Nawigacja mobilna" className="fixed inset-x-0 bottom-0 z-30 flex border-t bg-white md:hidden">
        {items.map(({ to, label, icon: Icon }) => (
          <NavLink key={to} to={to} end={to === "/"} className={({ isActive }) => cn("flex min-w-0 flex-1 flex-col items-center gap-0.5 py-2 text-[0.68rem] font-medium", isActive ? "text-primary" : "text-muted-foreground")}>
            <Icon className="size-5" aria-hidden />
            <span className="max-w-full truncate">{label}</span>
          </NavLink>
        ))}
      </nav>

      {addingOrg && (
        <NewOrganization
          onClose={() => setAddingOrg(false)}
          onCreated={(info) => {
            const next: Me = { ...me, organizations: [...me.organizations, info] };
            signIn(next);
            selectOrganization(info.id);
            navigate("/");
          }}
        />
      )}
    </div>
  );
}

function Topbar({ onAddOrganization }: { onAddOrganization: () => void }) {
  return (
    <header className="sticky top-0 z-20 flex h-[68px] items-center gap-3 border-b bg-white/85 px-4 backdrop-blur md:px-8">
      <div className="md:hidden">
        <Logo size="sm" />
      </div>
      <GlobalSearch />
      <div className="ml-auto flex items-center gap-1.5 sm:gap-3">
        <Notifications />
        <UserMenu onAddOrganization={onAddOrganization} />
      </div>
    </header>
  );
}

function GlobalSearch() {
  const { hasModule } = useSession();
  const navigate = useNavigate();
  const [text, setText] = useState("");
  const [open, setOpen] = useState(false);
  const [debounced, setDebounced] = useState("");
  const box = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(text.trim()), 200);
    return () => clearTimeout(timer);
  }, [text]);

  useEffect(() => {
    const close = (e: PointerEvent) => {
      if (!box.current?.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("pointerdown", close);
    return () => document.removeEventListener("pointerdown", close);
  }, []);

  const active = open && debounced.length >= 2;
  const cruises = useQuery({ queryKey: ["cruises"], queryFn: () => api<CruiseItem[]>("/cruises"), enabled: active && hasModule("rejsy") });
  const people = useQuery({
    queryKey: ["participants", debounced],
    queryFn: () => api<ParticipantItem[]>(`/participants?search=${encodeURIComponent(debounced)}`),
    enabled: active && hasModule("uczestnicy"),
  });
  const needle = debounced.toLowerCase();
  const cruiseHits = (cruises.data ?? []).filter((c) => `${c.name} ${c.departurePort} ${c.arrivalPort} ${c.yachtName ?? ""}`.toLowerCase().includes(needle)).slice(0, 5);
  const peopleHits = (people.data ?? []).slice(0, 5);

  const go = (to: string) => {
    setOpen(false);
    setText("");
    navigate(to);
  };

  return (
    <div ref={box} className="relative hidden w-full max-w-md sm:block">
      <Search className="pointer-events-none absolute top-1/2 left-3.5 size-4 -translate-y-1/2 text-muted-foreground" aria-hidden />
      <input
        type="search"
        aria-label="Szukaj rejsów i uczestników"
        placeholder="Szukaj rejsów, uczestników…"
        className="h-10 w-full rounded-xl bg-muted pr-3 pl-10 text-sm outline-none placeholder:text-muted-foreground focus:bg-white focus:ring-2 focus:ring-primary/30"
        value={text}
        onChange={(e) => {
          setText(e.target.value);
          setOpen(true);
        }}
        onFocus={() => setOpen(true)}
        onKeyDown={(e) => e.key === "Escape" && setOpen(false)}
      />
      {active && (
        <div className="card absolute inset-x-0 top-12 z-40 max-h-96 overflow-y-auto p-2 shadow-xl">
          {cruiseHits.length > 0 && <p className="px-2 pt-1 pb-1 text-xs font-semibold text-muted-foreground">Rejsy</p>}
          {cruiseHits.map((c) => (
            <button key={c.id} type="button" className="block w-full rounded-lg px-2 py-2 text-left text-sm hover:bg-accent" onClick={() => go(`/rejsy/${c.id}`)}>
              <span className="font-medium">{c.name}</span>
              <span className="block text-xs text-muted-foreground">{range(c.startDate, c.endDate)}</span>
            </button>
          ))}
          {peopleHits.length > 0 && <p className="px-2 pt-2 pb-1 text-xs font-semibold text-muted-foreground">Klienci</p>}
          {peopleHits.map((p) => (
            <button key={p.id} type="button" className="block w-full rounded-lg px-2 py-2 text-left text-sm hover:bg-accent" onClick={() => go(`/uczestnicy/${p.id}`)}>
              <span className="font-medium">{p.fullName}</span>
              <span className="block text-xs text-muted-foreground">{p.email ?? p.phone ?? "Bez kontaktu"}</span>
            </button>
          ))}
          {cruiseHits.length + peopleHits.length === 0 && !cruises.isFetching && !people.isFetching && <p className="p-3 text-sm text-muted-foreground">Brak wyników dla „{debounced}”.</p>}
        </div>
      )}
    </div>
  );
}

/** Powiadomienia wynikają z bieżącego stanu danych: zaległości, flota i listy rezerwowe. */
function Notifications() {
  const navigate = useNavigate();
  const { data } = useQuery({ queryKey: ["dashboard"], queryFn: () => api<Dashboard>("/dashboard") });
  const overdue = data?.overdue ?? [];
  const vessels = data?.vesselAlerts ?? [];
  const waitlist = data?.waitlistCount ?? 0;
  const count = (data?.overdueCount ?? 0) + vessels.length + (waitlist > 0 ? 1 : 0);

  return (
    <DropdownMenu>
      <DropdownMenuTrigger aria-label={`Powiadomienia: ${count}`} className="relative grid size-10 place-items-center rounded-xl text-foreground transition-colors hover:bg-muted">
        <Bell className="size-5" aria-hidden />
        {count > 0 && <span className="absolute top-1 right-1 grid h-[18px] min-w-[18px] place-items-center rounded-full bg-late px-1 text-[0.65rem] font-bold text-white">{count > 99 ? "99+" : count}</span>}
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="max-h-[70svh] w-[340px] overflow-y-auto p-2">
        <DropdownMenuGroup>
          <DropdownMenuLabel className="text-sm font-semibold text-foreground">Powiadomienia</DropdownMenuLabel>
          {count === 0 && <p className="px-2 py-3 text-sm text-muted-foreground">Wszystko w porządku. Nic nie wymaga Twojej uwagi.</p>}
          {overdue.map((o) => (
            <DropdownMenuItem key={o.enrollmentId} className="items-start gap-2.5 py-2" onClick={() => navigate(`/rejsy/${o.cruiseId}`)}>
              <AlertCircle className="mt-0.5 size-4 shrink-0 text-late" aria-hidden />
              <span className="min-w-0 text-sm">
                <span className="font-medium">
                  {o.fullName}: zaległe {zl(o.remaining)}
                </span>
                <span className="block text-xs text-muted-foreground">
                  {o.cruiseName}, termin {fullDay(o.dueDate)}
                </span>
              </span>
            </DropdownMenuItem>
          ))}
          {data && data.overdueCount > overdue.length && <p className="px-2 py-1 text-xs text-muted-foreground">Pozostałe zaległości: {data.overdueCount - overdue.length}</p>}
          {vessels.map((v) => (
            <DropdownMenuItem key={v.id} className="items-start gap-2.5 py-2" onClick={() => navigate("/flota")}>
              <Wrench className="mt-0.5 size-4 shrink-0 text-part" aria-hidden />
              <span className="min-w-0 text-sm">
                <span className="font-medium">{v.name}</span>
                <span className="block text-xs text-muted-foreground">{v.status === "OutOfService" ? `Niedostępna${v.notes ? `: ${v.notes}` : ""}` : `Przegląd do ${fullDay(v.inspectionDue!)}`}</span>
              </span>
            </DropdownMenuItem>
          ))}
          {waitlist > 0 && (
            <DropdownMenuItem className="items-start gap-2.5 py-2" onClick={() => navigate("/rejsy")}>
              <Users className="mt-0.5 size-4 shrink-0 text-primary" aria-hidden />
              <span className="text-sm font-medium">{plural(waitlist, "osoba czeka", "osoby czekają", "osób czeka")} na liście rezerwowej</span>
            </DropdownMenuItem>
          )}
        </DropdownMenuGroup>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

function UserMenu({ onAddOrganization }: { onAddOrganization: () => void }) {
  const { me, organization, selectOrganization, signOut } = useSession();
  const navigate = useNavigate();
  const initials = (organization?.name ?? me.fullName)
    .replace(/\(.*?\)/g, "")
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((w) => w[0]?.toUpperCase())
    .join("");

  return (
    <DropdownMenu>
      <DropdownMenuTrigger aria-label="Menu konta" className="flex items-center gap-2.5 rounded-xl py-1 pr-2 pl-1 transition-colors hover:bg-muted">
        <span className="grid size-9 place-items-center rounded-full bg-gradient-to-br from-primary to-sea text-xs font-bold text-white" aria-hidden>
          {initials || <Building2 className="size-4" />}
        </span>
        <span className="hidden max-w-60 truncate text-sm font-semibold sm:block">{organization?.name ?? me.fullName}</span>
        <ChevronDown className="size-4 text-muted-foreground" aria-hidden />
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-72 p-2">
        <DropdownMenuGroup>
          <DropdownMenuLabel className="text-foreground">
            <span className="block text-sm font-semibold">{me.fullName}</span>
            <span className="block truncate text-xs font-normal text-muted-foreground">{me.isAdmin ? "Administrator" : me.email}</span>
          </DropdownMenuLabel>
        </DropdownMenuGroup>
        <DropdownMenuSeparator />
        <DropdownMenuItem className="gap-2.5 py-2" onClick={() => navigate("/ustawienia")}>
          <Settings className="size-4" aria-hidden /> Ustawienia firmy
        </DropdownMenuItem>
        {me.organizations.length > 1 && (
          <>
            <DropdownMenuSeparator />
            <DropdownMenuGroup>
              <DropdownMenuLabel>Organizacja</DropdownMenuLabel>
              {me.organizations.map((o) => (
                <DropdownMenuItem
                  key={o.id}
                  className="gap-2.5 py-2"
                  onClick={() =>
                    startTransition(() => {
                      navigate("/");
                      selectOrganization(o.id);
                    })
                  }
                >
                  <Check className={cn("size-4", o.id !== organization?.id && "invisible")} aria-hidden />
                  <span className="truncate">{o.name}</span>
                </DropdownMenuItem>
              ))}
            </DropdownMenuGroup>
          </>
        )}
        {me.isAdmin && (
          <DropdownMenuItem className="gap-2.5 py-2" onClick={onAddOrganization}>
            <Plus className="size-4" aria-hidden /> Dodaj organizację
          </DropdownMenuItem>
        )}
        <DropdownMenuSeparator />
        <DropdownMenuItem className="gap-2.5 py-2" onClick={() => signOut().catch((e: Error) => toast.error(e.message))}>
          <LogOut className="size-4" aria-hidden /> Wyloguj się
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

function NewOrganization({ onClose, onCreated }: { onClose: () => void; onCreated: (organization: OrganizationInfo) => void }) {
  const [name, setName] = useState("");
  const { submit, error, busy } = useFormSubmit(async () => {
    const created = await api<OrganizationInfo>("/admin/organizations", { method: "POST", body: { name } });
    toast.success(`Dodano organizację: ${created.name}`);
    onCreated(created);
    onClose();
  });
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent>
        <form className="grid gap-4" onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>Dodaj organizację</DialogTitle>
          </DialogHeader>
          <Field label="Nazwa">
            <Input autoFocus value={name} onChange={(e) => setName(e.target.value)} />
          </Field>
          <FormError message={error} />
          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              Anuluj
            </Button>
            <Button type="submit" disabled={busy || !name.trim()}>
              Dodaj organizację
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
