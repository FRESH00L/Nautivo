import { cn } from "@/lib/utils";

export function LogoMark({ className, light = false }: { className?: string; light?: boolean }) {
  const id = light ? "sail-light" : "sail-dark";
  return (
    <svg viewBox="0 0 64 64" className={className} aria-hidden>
      <defs>
        <linearGradient id={`${id}-a`} x1="0" y1="0" x2="1" y2="1">
          <stop offset="0" stopColor={light ? "#7cc0ff" : "#1e88ee"} />
          <stop offset="1" stopColor={light ? "#2f8cf0" : "#0a3f96"} />
        </linearGradient>
        <linearGradient id={`${id}-b`} x1="0" y1="0" x2="1" y2="1">
          <stop offset="0" stopColor={light ? "#d8ecff" : "#6fb8fb"} />
          <stop offset="1" stopColor={light ? "#79baff" : "#1673e0"} />
        </linearGradient>
      </defs>
      <path d="M31 4C25 18 15 32 6 43c9-2 18-3 25-2 1-12 1-25 0-37z" fill={`url(#${id}-a)`} />
      <path d="M36 9c2 11 2 21 1 32 8-1 15 0 21 2-4-13-12-25-22-34z" fill={`url(#${id}-b)`} />
      <path d="M3 49c11-6 21-5 30 0 9 4 18 3 27-4-5 11-19 17-32 13-9-3-17-6-25-9z" fill={light ? "#ffffff" : "#0a2f73"} />
      <path d="M14 56c8 0 13 2 19 3 8 1 15-1 21-5-6 7-15 9-23 8-6-1-11-4-17-6z" fill={light ? "#7cc0ff" : "#1e88ee"} />
    </svg>
  );
}

/** Znak i nazwa. Mała żagielka zastępuje kropkę nad „i”, jak w logotypie. */
export function Logo({ light = false, slogan = false, className, size = "md" }: { light?: boolean; slogan?: boolean; className?: string; size?: "sm" | "md" | "lg" }) {
  const large = size === "lg";
  return (
    <div className={cn("flex items-center", large ? "gap-3.5" : "gap-2", className)}>
      <LogoMark light={light} className={large ? "size-20" : size === "sm" ? "size-8" : "size-10"} />
      <div>
        <div className={cn("leading-none font-extrabold tracking-tight", large ? "text-[2.6rem]" : size === "sm" ? "text-xl" : "text-2xl", light ? "text-white" : "text-sea")} aria-label="Nautivo">
          <span aria-hidden>
            Naut
            <span className="relative">
              ı
              <svg viewBox="0 0 10 12" className={cn("absolute left-1/2 -translate-x-1/2", large ? "-top-1 h-3.5" : "-top-0.5 h-2")} aria-hidden>
                <path d="M5 0 L9.5 12 H0.5 Z" fill={light ? "#7cc0ff" : "#0b6bde"} />
              </svg>
            </span>
            vo
          </span>
        </div>
        {slogan && <div className={cn("mt-2 text-[0.68rem] font-medium tracking-[0.32em] whitespace-nowrap", light ? "text-[#bcd6f3]" : "text-sea/80")}>SAIL • BOOK • EXPLORE</div>}
      </div>
    </div>
  );
}

const SCENES = [
  { sky: ["#8cc4f5", "#e9f3fc"], sea: ["#2b84d8", "#0b4c9c"], sun: "#fff3c4" },
  { sky: ["#f8c98e", "#fdeadb"], sea: ["#3a7fc4", "#0f3f80"], sun: "#ffe3a3" },
  { sky: ["#6db6e8", "#cfeaf8"], sea: ["#1a9bb5", "#0a5f85"], sun: "#ffffff" },
  { sky: ["#a9c9ef", "#eef4fc"], sea: ["#1f6fd0", "#0a2f73"], sun: "#ffeec2" },
];

/** Miniatura rejsu rysowana z kodu: stała dla danego rejsu i bez zewnętrznych zdjęć. */
export function CruiseThumb({ seed, className }: { seed: string; className?: string }) {
  const hash = seed.split("").reduce((sum, c) => sum + c.charCodeAt(0), 0);
  const scene = SCENES[hash % SCENES.length];
  const id = `thumb-${hash}`;
  const sunX = 18 + (hash % 50);
  return (
    <svg viewBox="0 0 96 64" preserveAspectRatio="xMidYMid slice" className={cn("shrink-0 overflow-hidden rounded-lg", className)} aria-hidden>
      <defs>
        <linearGradient id={`${id}-sky`} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor={scene.sky[0]} />
          <stop offset="1" stopColor={scene.sky[1]} />
        </linearGradient>
        <linearGradient id={`${id}-sea`} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0" stopColor={scene.sea[0]} />
          <stop offset="1" stopColor={scene.sea[1]} />
        </linearGradient>
      </defs>
      <rect width="96" height="64" fill={`url(#${id}-sky)`} />
      <circle cx={sunX} cy="22" r="7" fill={scene.sun} />
      <path d="M0 42c12-3 22-3 34 0s24 3 62-2v24H0z" fill={`url(#${id}-sea)`} />
      <path d="M50 12c-3 9-7 17-12 24h12z" fill="#ffffff" />
      <path d="M53 16c5 6 8 13 10 20H53z" fill="#ffffff" opacity=".85" />
      <path d="M36 38h30c-2 4-5 6-10 6H44c-4 0-6-2-8-6z" fill="#0a2455" />
    </svg>
  );
}
