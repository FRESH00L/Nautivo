import "leaflet/dist/leaflet.css";
import { latLngBounds, type LatLngExpression } from "leaflet";
import { useEffect } from "react";
import { CircleMarker, MapContainer, Polyline, TileLayer, Tooltip, useMap, useMapEvents } from "react-leaflet";
import type { RoutePoint } from "@/api";
import { cn } from "@/lib/utils";

type Located = RoutePoint & { lat: number; lng: number; index: number };

const located = (points: RoutePoint[]) =>
  points.flatMap((p, index) => (p.lat != null && p.lng != null ? [{ ...p, lat: p.lat, lng: p.lng, index } as Located] : []));

/** Dopasowuje widok do punktów trasy, ale tylko gdy zmienią się ich współrzędne. */
function FitToPoints({ points }: { points: Located[] }) {
  const map = useMap();
  const key = points.map((p) => `${p.lat},${p.lng}`).join("|");
  useEffect(() => {
    if (points.length === 0) return;
    if (points.length === 1) {
      map.setView([points[0].lat, points[0].lng], 10);
      return;
    }
    map.fitBounds(latLngBounds(points.map((p) => [p.lat, p.lng] as [number, number])), { padding: [30, 30], maxZoom: 11 });
    // key opisuje współrzędne; sama tablica zmienia się przy każdym renderze.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [map, key]);
  return null;
}

/** Okno dialogowe otwiera się z animacją, więc Leaflet musi przeliczyć rozmiar mapy po pojawieniu się. */
function ResizeAfterOpen() {
  const map = useMap();
  useEffect(() => {
    const timer = setTimeout(() => map.invalidateSize(), 250);
    const observer = new ResizeObserver(() => map.invalidateSize());
    observer.observe(map.getContainer());
    return () => {
      clearTimeout(timer);
      observer.disconnect();
    };
  }, [map]);
  return null;
}

function ClickToPick({ onPick }: { onPick: (lat: number, lng: number) => void }) {
  useMapEvents({ click: (e) => onPick(Number(e.latlng.lat.toFixed(5)), Number(e.latlng.lng.toFixed(5))) });
  return null;
}

/**
 * Mapa trasy rejsu: punkty w kolejności połączone linią. Podkład OpenStreetMap.
 * Z `onPick` kliknięcie w mapę zwraca współrzędne (dodanie punktu albo wskazanie portu).
 */
export function RouteMap({ points, onPick, className, highlight }: { points: RoutePoint[]; onPick?: (lat: number, lng: number) => void; className?: string; highlight?: number | null }) {
  const shown = located(points);
  const line: LatLngExpression[] = shown.map((p) => [p.lat, p.lng]);
  const last = points.length - 1;
  return (
    <div className={cn("relative isolate overflow-hidden rounded-xl border", onPick && "cursor-crosshair", className)}>
      <MapContainer center={[54.5, 18.6]} zoom={6} scrollWheelZoom={false} className="h-full w-full" attributionControl>
        <TileLayer attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>' url="https://tile.openstreetmap.org/{z}/{x}/{y}.png" />
        {line.length > 1 && <Polyline positions={line} pathOptions={{ color: "#0b6bde", weight: 3, dashArray: "6 6" }} />}
        {shown.map((p) => {
          const edge = p.index === 0 || p.index === last;
          // Punkt w tym samym miejscu co wcześniejszy (np. meta = start) dopisujemy do jego etykiety zamiast zasłaniać go drugim znacznikiem.
          if (shown.some((q) => q.index < p.index && q.lat === p.lat && q.lng === p.lng)) return null;
          const same = shown.filter((q) => q.index > p.index && q.lat === p.lat && q.lng === p.lng);
          const numbers = [p.index, ...same.map((q) => q.index)].map((i) => i + 1).join(" i ");
          return (
            <CircleMarker
              key={`${p.index}-${p.lat}-${p.lng}`}
              center={[p.lat, p.lng]}
              radius={edge ? 8 : 6}
              pathOptions={{ color: "#ffffff", weight: 2, fillColor: highlight === p.index ? "#c4322f" : edge ? "#0a2455" : "#0b6bde", fillOpacity: 1 }}
            >
              <Tooltip direction="top" offset={[0, -6]} permanent={shown.length <= 8}>
                {numbers}. {p.name || "Punkt bez nazwy"}
              </Tooltip>
            </CircleMarker>
          );
        })}
        <FitToPoints points={shown} />
        <ResizeAfterOpen />
        {onPick && <ClickToPick onPick={onPick} />}
      </MapContainer>
    </div>
  );
}

const cache = new Map<string, { lat: number; lng: number } | null>();
let queue: Promise<unknown> = Promise.resolve();

/**
 * Szuka portu w Nominatim (OpenStreetMap). Zapytania idą po kolei, co najmniej sekundę od siebie,
 * zgodnie z zasadami korzystania z publicznego Nominatim. Wyniki trzymamy w pamięci strony.
 */
export function geocode(name: string): Promise<{ lat: number; lng: number } | null> {
  const query = name.trim();
  if (!query) return Promise.resolve(null);
  const key = query.toLowerCase();
  if (cache.has(key)) return Promise.resolve(cache.get(key)!);
  const run = queue.then(async () => {
    if (cache.has(key)) return cache.get(key)!;
    try {
      const url = `https://nominatim.openstreetmap.org/search?format=jsonv2&limit=1&accept-language=pl&q=${encodeURIComponent(query)}`;
      const response = await fetch(url, { headers: { Accept: "application/json" } });
      const rows = response.ok ? ((await response.json()) as { lat: string; lon: string }[]) : [];
      const hit = rows[0] ? { lat: Number(Number(rows[0].lat).toFixed(5)), lng: Number(Number(rows[0].lon).toFixed(5)) } : null;
      if (response.ok) cache.set(key, hit);
      return hit;
    } catch {
      return null;
    } finally {
      await new Promise((r) => setTimeout(r, 1000));
    }
  });
  queue = run.catch(() => null);
  return run;
}
