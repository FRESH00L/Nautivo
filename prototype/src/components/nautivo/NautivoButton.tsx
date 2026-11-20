import { ArrowLeft, Loader2, MapPin, Pencil, Plus, Trash2, X, type LucideIcon } from "lucide-react";
import type { ComponentProps, ReactNode } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

/**
 * Jedyny przycisk z tekstem w aplikacji. Zamiast wariantów shadcn używamy nazw z naszego słownika:
 * primary (główna akcja), secondary (obramowany), ghost (cichy), danger (usuwanie).
 */
export type NautivoButtonVariant = "primary" | "secondary" | "ghost" | "danger";
export type NautivoButtonSize = "sm" | "md" | "lg";

const VARIANT = { primary: "default", secondary: "outline", ghost: "ghost", danger: "destructive" } as const;
const SIZE = { sm: "sm", md: "default", lg: "lg" } as const;

type ButtonProps = Omit<ComponentProps<typeof Button>, "variant" | "size" | "children">;

export function NautivoButton({
  variant = "primary",
  size = "md",
  icon: Icon,
  loading = false,
  disabled,
  children,
  ...props
}: ButtonProps & { variant?: NautivoButtonVariant; size?: NautivoButtonSize; icon?: LucideIcon; loading?: boolean; children: ReactNode }) {
  return (
    <Button variant={VARIANT[variant]} size={SIZE[size]} disabled={disabled || loading} aria-busy={loading || undefined} {...props}>
      {loading ? <Loader2 className="animate-spin" aria-hidden /> : Icon && <Icon aria-hidden />}
      {children}
    </Button>
  );
}

/**
 * Przycisk z samą ikoną. Gotowe akcje mają stałą ikonę, kolor i podpis, więc wszędzie wyglądają tak samo.
 * `label` mówi, czego dotyczy akcja (np. nazwa rejsu); trafia do czytnika ekranu i dymka.
 */
const ACTIONS = {
  edit: { icon: Pencil, verb: "Edytuj", className: "" },
  delete: { icon: Trash2, verb: "Usuń", className: "text-late hover:bg-late-soft hover:text-late" },
  close: { icon: X, verb: "Zamknij", className: "" },
  add: { icon: Plus, verb: "Dodaj", className: "" },
  locate: { icon: MapPin, verb: "Wskaż na mapie", className: "" },
} satisfies Record<string, { icon: LucideIcon; verb: string; className: string }>;

export type NautivoIconAction = keyof typeof ACTIONS;

export function NautivoIconButton({
  action,
  label,
  size = "md",
  active,
  className,
  ...props
}: Omit<ButtonProps, "aria-label"> & { action: NautivoIconAction; label?: string; size?: "sm" | "md"; active?: boolean }) {
  const { icon: Icon, verb, className: tone } = ACTIONS[action];
  const text = label ? `${verb}: ${label}` : verb;
  return (
    <Button
      type="button"
      variant={active ? "default" : "ghost"}
      size={size === "sm" ? "icon-xs" : "icon-lg"}
      aria-label={text}
      aria-pressed={active}
      title={text}
      className={cn(!active && tone, className)}
      {...props}
    >
      <Icon aria-hidden />
    </Button>
  );
}

/** Strzałka wstecz. Wraca do poprzedniego widoku w aplikacji; po wejściu z linku prowadzi do `fallback`. */
export function NautivoBackButton({ fallback, label = "Wróć" }: { fallback: string; label?: string }) {
  const navigate = useNavigate();
  const location = useLocation();
  // React Router nadaje pierwszemu wpisowi historii klucz "default", więc wtedy nie ma dokąd wracać w aplikacji.
  const canGoBack = location.key !== "default";
  return (
    <Button type="button" variant="ghost" size="icon-lg" aria-label={label} title={label} onClick={() => (canGoBack ? navigate(-1) : navigate(fallback))}>
      <ArrowLeft className="size-5" aria-hidden />
    </Button>
  );
}
