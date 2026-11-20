import { useEffect, useRef, useState } from "react";
import type { TrendPoint } from "@/api";
import { day, zl } from "@/components/shared";

const HEIGHT = 250;
const PAD = { top: 16, right: 12, bottom: 28, left: 52 };
const STROKE = "#0b6bde";

function niceMax(value: number) {
  if (value <= 0) return 4;
  const power = 10 ** Math.floor(Math.log10(value));
  // Kroki 1, 2, 5 i 10 dają całkowite podziałki, więc liczba rezerwacji nie wychodzi ułamkowa.
  const step = [1, 2, 5, 10].find((s) => s * power * 4 >= value) ?? 10;
  return Math.max(4, step * power * 4);
}

const compact = (n: number) => (n >= 1000 ? `${(n / 1000).toLocaleString("pl-PL", { maximumFractionDigits: 1 })} tys.` : n.toLocaleString("pl-PL"));

export function TrendChart({ points, metric, loading }: { points: TrendPoint[]; metric: "reservations" | "income"; loading?: boolean }) {
  const box = useRef<HTMLDivElement>(null);
  const [width, setWidth] = useState(640);
  const [active, setActive] = useState<number | null>(null);

  useEffect(() => {
    if (!box.current) return;
    const observer = new ResizeObserver(([entry]) => setWidth(entry.contentRect.width));
    observer.observe(box.current);
    return () => observer.disconnect();
  }, []);

  const value = (p: TrendPoint) => (metric === "income" ? p.income : p.reservations);
  const format = (n: number) => (metric === "income" ? zl(n) : n.toLocaleString("pl-PL"));
  const label = (p: TrendPoint) => (p.date === p.end ? day(p.date) : `${day(p.date)} – ${day(p.end)}`);

  const max = niceMax(Math.max(0, ...points.map(value)));
  const plotW = Math.max(10, width - PAD.left - PAD.right);
  const plotH = HEIGHT - PAD.top - PAD.bottom;
  const x = (i: number) => PAD.left + (points.length > 1 ? (i / (points.length - 1)) * plotW : plotW / 2);
  const y = (v: number) => PAD.top + plotH - (v / max) * plotH;

  // Krzywa z poziomymi uchwytami nie przekracza wartości punktów, więc nie rysuje danych, których nie ma.
  const line = points.map((p, i) => {
    const px = x(i);
    const py = y(value(p));
    if (i === 0) return `M${px} ${py}`;
    const mid = (x(i - 1) + px) / 2;
    return `C${mid} ${y(value(points[i - 1]))} ${mid} ${py} ${px} ${py}`;
  }).join(" ");
  const area = points.length ? `${line} L${x(points.length - 1)} ${y(0)} L${x(0)} ${y(0)} Z` : "";

  const ticks = [0, 1, 2, 3, 4].map((t) => (max / 4) * t);
  const labelCount = Math.min(points.length, width < 480 ? 3 : 5);
  const labelIndexes = labelCount > 1 ? Array.from({ length: labelCount }, (_, i) => Math.round((i * (points.length - 1)) / (labelCount - 1))) : [0];

  const pick = (clientX: number, rect: DOMRect) => {
    if (points.length < 2) return setActive(points.length ? 0 : null);
    const ratio = (clientX - rect.left - PAD.left) / plotW;
    setActive(Math.max(0, Math.min(points.length - 1, Math.round(ratio * (points.length - 1)))));
  };

  const current = active !== null ? points[active] : null;
  const tooltipLeft = active !== null ? Math.max(70, Math.min(width - 70, x(active))) : 0;
  const total = points.reduce((sum, p) => sum + value(p), 0);

  return (
    <div ref={box} className="relative transition-opacity" style={{ opacity: loading ? 0.55 : 1 }}>
      <svg
        width="100%"
        height={HEIGHT}
        viewBox={`0 0 ${width} ${HEIGHT}`}
        role="img"
        tabIndex={0}
        aria-label={`Wykres trendu: ${metric === "income" ? "przychód" : "rezerwacje"}, razem ${format(total)}. Strzałkami przejdziesz po punktach.`}
        className="block touch-pan-y rounded-lg"
        onPointerMove={(e) => pick(e.clientX, e.currentTarget.getBoundingClientRect())}
        onPointerLeave={() => setActive(null)}
        onBlur={() => setActive(null)}
        onKeyDown={(e) => {
          if (e.key === "ArrowRight") setActive((i) => Math.min(points.length - 1, (i ?? -1) + 1));
          if (e.key === "ArrowLeft") setActive((i) => Math.max(0, (i ?? points.length) - 1));
        }}
      >
        <defs>
          <linearGradient id="trend-fill" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0" stopColor={STROKE} stopOpacity=".2" />
            <stop offset="1" stopColor={STROKE} stopOpacity=".01" />
          </linearGradient>
        </defs>
        {ticks.map((t) => (
          <g key={t}>
            <line x1={PAD.left} x2={width - PAD.right} y1={y(t)} y2={y(t)} stroke="var(--border)" strokeWidth="1" />
            <text x={PAD.left - 10} y={y(t) + 4} textAnchor="end" fontSize="11" fill="var(--muted-foreground)">
              {compact(t)}
            </text>
          </g>
        ))}
        {labelIndexes.map((i) => (
          <text key={i} x={x(i)} y={HEIGHT - 8} textAnchor={i === 0 ? "start" : i === points.length - 1 ? "end" : "middle"} fontSize="11" fill="var(--muted-foreground)">
            {points[i] && day(points[i].end)}
          </text>
        ))}
        <path d={area} fill="url(#trend-fill)" />
        <path d={line} fill="none" stroke={STROKE} strokeWidth="2" strokeLinejoin="round" strokeLinecap="round" />
        {current && active !== null && (
          <>
            <line x1={x(active)} x2={x(active)} y1={PAD.top} y2={y(0)} stroke="var(--input)" strokeWidth="1" />
            <circle cx={x(active)} cy={y(value(current))} r="5" fill={STROKE} stroke="#fff" strokeWidth="2" />
          </>
        )}
      </svg>
      {current && active !== null && (
        <div className="pointer-events-none absolute z-10 -translate-x-1/2 rounded-lg bg-sea px-3 py-1.5 text-center text-white shadow-lg" style={{ left: tooltipLeft, top: Math.max(0, y(value(current)) - 58) }} role="status">
          <div className="text-sm font-semibold whitespace-nowrap">{format(value(current))}</div>
          <div className="text-[0.7rem] whitespace-nowrap text-[#c8d7ee]">{label(current)}</div>
        </div>
      )}
      <table className="sr-only">
        <caption>Dane wykresu</caption>
        <thead>
          <tr>
            <th>Okres</th>
            <th>{metric === "income" ? "Przychód" : "Rezerwacje"}</th>
          </tr>
        </thead>
        <tbody>
          {points.map((p) => (
            <tr key={p.end}>
              <td>{label(p)}</td>
              <td>{format(value(p))}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
