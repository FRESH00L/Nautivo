import { useQuery } from "@tanstack/react-query";
import { ArrowDownRight, ArrowRight, ArrowUpRight, CalendarDays, CheckCircle2, CircleDashed, MapPin, Sailboat, Users, Wallet, type LucideIcon } from "lucide-react";
import { useState } from "react";
import { Link } from "react-router-dom";
import { api, type Dashboard as Data, type TrendResponse } from "@/api";
import { useSession } from "@/auth";
import { CruiseThumb } from "@/components/Brand";
import { fullDay, LoadState, plural, range, zl } from "@/components/shared";
import { TrendChart } from "@/components/TrendChart";
import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const RANGES = [
  { days: 7, label: "7D", name: "7 dni" },
  { days: 30, label: "30D", name: "30 dni" },
  { days: 90, label: "90D", name: "90 dni" },
  { days: 365, label: "1R", name: "rok" },
];

function Delta({ current, previous }: { current: number; previous: number }) {
  if (previous <= 0) return <p className="mt-1 text-xs text-muted-foreground">Brak danych z poprzednich 30 dni</p>;
  const change = Math.round(((current - previous) / previous) * 100);
  const up = change >= 0;
  const Icon = up ? ArrowUpRight : ArrowDownRight;
  return (
    <p className={cn("mt-1 flex items-center gap-0.5 text-xs font-semibold", up ? "text-paid" : "text-late")} title="W porównaniu z poprzednimi 30 dniami">
      <Icon className="size-3.5" aria-hidden />
      {up ? "+" : ""}
      {change}% <span className="ml-1 font-normal text-muted-foreground">do poprzednich 30 dni</span>
    </p>
  );
}

function Stat({ icon: Icon, value, label, children }: { icon: LucideIcon; value: string; label: string; children: React.ReactNode }) {
  return (
    <div className="card flex items-start gap-3.5 p-5">
      <span className="grid size-11 shrink-0 place-items-center rounded-xl bg-accent text-primary">
        <Icon className="size-6" aria-hidden />
      </span>
      <div className="min-w-0">
        <p className="text-[1.65rem] leading-tight font-bold tracking-tight">{value}</p>
        <p className="mt-0.5 text-sm text-muted-foreground">{label}</p>
        {children}
      </div>
    </div>
  );
}

export function Dashboard() {
  const { me } = useSession();
  const { data, error } = useQuery({ queryKey: ["dashboard"], queryFn: () => api<Data>("/dashboard") });
  const [days, setDays] = useState(30);
  const [metric, setMetric] = useState<"reservations" | "income">("reservations");
  const trend = useQuery({
    queryKey: ["trend", days],
    queryFn: () => api<TrendResponse>(`/dashboard/trend?days=${days}`),
    placeholderData: (previous) => previous,
  });

  if (!data) return <LoadState error={error} />;
  const period = data.last30Days;
  const firstName = me.fullName.trim().split(/\s+/)[0];

  return (
    <>
      <h1 className="text-3xl">Witaj, {firstName}!</h1>
      <p className="mt-1 text-muted-foreground">Oto podsumowanie Twojej działalności.</p>

      <section className="mt-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-4" aria-label="Najważniejsze liczby">
        <Stat icon={Sailboat} value={data.cruisesUpcoming.toLocaleString("pl-PL")} label="Aktywne rejsy">
          <p className="mt-1 text-xs text-muted-foreground">{plural(period.cruisesStarting, "startuje", "startują", "startuje")} w ciągu 30 dni</p>
        </Stat>
        <Stat icon={Users} value={data.participantsTotal.toLocaleString("pl-PL")} label="Klienci">
          {period.newClients > 0 ? (
            <p className="mt-1 flex items-center gap-0.5 text-xs font-semibold text-paid">
              <ArrowUpRight className="size-3.5" aria-hidden />+{period.newClients} <span className="ml-1 font-normal text-muted-foreground">w ciągu 30 dni</span>
            </p>
          ) : (
            <p className="mt-1 text-xs text-muted-foreground">Bez nowych w ciągu 30 dni</p>
          )}
        </Stat>
        <Stat icon={CalendarDays} value={period.reservations.toLocaleString("pl-PL")} label="Rezerwacje (30 dni)">
          <Delta current={period.reservations} previous={period.reservationsPrevious} />
        </Stat>
        <Stat icon={Wallet} value={zl(period.income)} label="Przychód (30 dni)">
          <Delta current={period.income} previous={period.incomePrevious} />
        </Stat>
      </section>

      <div className="mt-4 grid gap-4 lg:grid-cols-[minmax(0,1fr)_340px]">
        <section className="card p-5" aria-labelledby="trend-title">
          <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
            <div className="flex items-center gap-1">
              <h2 id="trend-title" className="sr-only">
                Trend
              </h2>
              {(
                [
                  ["reservations", "Trend rezerwacji"],
                  ["income", "Trend przychodu"],
                ] as const
              ).map(([id, label]) => (
                <button key={id} type="button" aria-pressed={metric === id} onClick={() => setMetric(id)} className={cn("rounded-lg px-2.5 py-1 text-base font-bold transition-colors", metric === id ? "text-foreground" : "text-muted-foreground/70 hover:text-muted-foreground")}>
                  {label}
                </button>
              ))}
            </div>
            <div className="flex gap-0.5" role="group" aria-label="Zakres wykresu">
              {RANGES.map((r) => (
                <button key={r.days} type="button" aria-label={r.name} aria-pressed={days === r.days} onClick={() => setDays(r.days)} className={cn("rounded-lg px-2.5 py-1 text-xs font-semibold transition-colors", days === r.days ? "bg-accent text-primary" : "text-muted-foreground hover:bg-muted")}>
                  {r.label}
                </button>
              ))}
            </div>
          </div>
          {trend.data ? <TrendChart points={trend.data.points} metric={metric} loading={trend.isFetching} /> : <LoadState error={trend.error} />}
        </section>

        <section className="card p-5" aria-labelledby="your-cruises">
          <div className="mb-3 flex items-center justify-between gap-2">
            <h2 id="your-cruises" className="text-base">
              Twoje rejsy
            </h2>
            <Link to="/rejsy" className="flex items-center gap-1 text-xs font-semibold text-primary hover:underline">
              Zobacz wszystkie <ArrowRight className="size-3.5" aria-hidden />
            </Link>
          </div>
          {data.yourCruises.length === 0 ? (
            <p className="py-6 text-center text-sm text-muted-foreground">Nie masz jeszcze nadchodzących rejsów.</p>
          ) : (
            <ul className="grid gap-1">
              {data.yourCruises.map((c) => (
                <li key={c.id}>
                  <Link to={`/rejsy/${c.id}`} className="flex items-center gap-3 rounded-xl p-1.5 transition-colors hover:bg-muted">
                    <CruiseThumb seed={c.id} className="h-12 w-[4.5rem]" />
                    <span className="min-w-0">
                      <span className="block truncate text-sm font-semibold">{c.name}</span>
                      <span className="block text-xs text-muted-foreground">
                        {plural(c.reservations, "rezerwacja", "rezerwacje", "rezerwacji")}, {fullDay(c.startDate)}
                      </span>
                    </span>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>

      <section className="card mt-4 p-5" aria-labelledby="upcoming">
        <div className="mb-2 flex items-center justify-between gap-2">
          <h2 id="upcoming" className="text-base">
            Nadchodzące rejsy
          </h2>
          <Link to="/rejsy" className="flex items-center gap-1 text-xs font-semibold text-primary hover:underline">
            Zobacz wszystkie <ArrowRight className="size-3.5" aria-hidden />
          </Link>
        </div>
        {data.nextCruises.length === 0 ? (
          <div className="py-8 text-center">
            <p className="text-muted-foreground">Dodaj pierwszy rejs, aby zobaczyć go tutaj.</p>
            <Link to="/rejsy" className={cn(buttonVariants(), "mt-3")}>
              Przejdź do rejsów
            </Link>
          </div>
        ) : (
          <ul className="divide-y">
            {data.nextCruises.map((c) => (
              <li key={c.id} className="grid items-center gap-x-5 gap-y-3 py-3.5 sm:grid-cols-[1fr_auto] lg:grid-cols-[minmax(0,1fr)_150px_90px_auto]">
                <div className="flex min-w-0 items-center gap-3.5">
                  <CruiseThumb seed={c.id} className="h-14 w-[5.25rem]" />
                  <div className="min-w-0">
                    <p className="truncate font-semibold">{c.name}</p>
                    <p className="mt-0.5 flex flex-wrap items-center gap-x-3 gap-y-0.5 text-xs text-muted-foreground">
                      <span className="flex items-center gap-1">
                        <CalendarDays className="size-3.5" aria-hidden /> {range(c.startDate, c.endDate)}
                      </span>
                      {c.location && (
                        <span className="flex items-center gap-1">
                          <MapPin className="size-3.5" aria-hidden /> {c.location}
                        </span>
                      )}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-4 lg:contents">
                  <div>
                    <span className={cn("inline-flex items-center gap-1 rounded-full px-2.5 py-1 text-xs font-semibold whitespace-nowrap", c.isConfirmed ? "bg-paid-soft text-paid" : "bg-part-soft text-part")}>
                      {c.isConfirmed ? <CheckCircle2 className="size-3.5" aria-hidden /> : <CircleDashed className="size-3.5" aria-hidden />}
                      {c.isConfirmed ? "Potwierdzony" : "Niepotwierdzony"}
                    </span>
                  </div>
                  <p className="flex items-center gap-1.5 text-sm font-semibold whitespace-nowrap" aria-label={`Zapisanych ${c.confirmed} z ${c.capacity} osób`}>
                    <Users className="size-4 text-muted-foreground" aria-hidden />
                    {c.confirmed} / {c.capacity}
                  </p>
                  <Link to={`/rejsy/${c.id}`} aria-label={`Zarządzaj: ${c.name}`} className={cn(buttonVariants({ variant: "outline" }), "ml-auto border-primary/40 font-semibold text-primary hover:bg-accent hover:text-primary lg:ml-0")}>
                    Zarządzaj
                  </Link>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>
    </>
  );
}
