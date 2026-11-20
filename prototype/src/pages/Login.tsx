import { Eye, EyeOff, Loader2, Lock, Mail, User, Building2, type LucideIcon } from "lucide-react";
import { useState, type ReactNode } from "react";
import { api, type Me } from "@/api";
import { useSignIn } from "@/auth";
import { Logo } from "@/components/Brand";
import { FormError, useFormSubmit } from "@/components/shared";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

type Mode = "login" | "register";

const COPY: Record<Mode, { title: string; subtitle: string; submit: string; path: string }> = {
  login: { title: "Witaj ponownie", subtitle: "Zaloguj się, aby zarządzać rejsami.", submit: "Zaloguj się", path: "/auth/login" },
  register: { title: "Załóż konto", subtitle: "Utwórz konto swojej organizacji.", submit: "Załóż konto", path: "/auth/register" },
};

/**
 * Secret key administratora trzymany w tej przeglądarce. Zapisuje się go raz, otwierając panel
 * z adresem /#admin-key=<klucz> (fragment po # nie trafia do serwera ani logów). Logowanie dołącza go samo.
 */
const ADMIN_KEY_STORAGE = "nautivo.adminKey";

function captureAdminKeyFromUrl() {
  const match = /^#admin-key=(.+)$/.exec(window.location.hash);
  if (!match) return;
  try {
    let key = match[1];
    try {
      key = decodeURIComponent(key);
    } catch {
      // Klucz z samotnym „%” nie jest zakodowany; zapisujemy go dosłownie.
    }
    localStorage.setItem(ADMIN_KEY_STORAGE, key);
  } catch {
    // Bez localStorage klucza nie da się zapamiętać; logowanie administratora po prostu nie zadziała.
  }
  history.replaceState(null, "", window.location.pathname + window.location.search);
}

// Wywołane przy załadowaniu modułu, więc działa także wtedy, gdy ktoś jest już zalogowany.
captureAdminKeyFromUrl();

function storedAdminKey() {
  try {
    return localStorage.getItem(ADMIN_KEY_STORAGE);
  } catch {
    return null;
  }
}

function IconField({ label, icon: Icon, hint, trailing, ...input }: { label: string; icon: LucideIcon; hint?: string; trailing?: ReactNode } & React.InputHTMLAttributes<HTMLInputElement>) {
  return (
    <div>
      <label className="grid gap-1.5 text-sm font-medium">
        {label}
        <span className="relative block font-normal">
          <Icon className="pointer-events-none absolute top-1/2 left-3.5 size-[18px] -translate-y-1/2 text-muted-foreground" aria-hidden />
          <input
            {...input}
            className={cn(
              "h-12 w-full rounded-xl border border-input bg-white pl-11 text-[0.95rem] transition-colors outline-none placeholder:text-muted-foreground/70 hover:border-[#b5c3d6] focus:border-primary focus:ring-3 focus:ring-primary/15",
              trailing ? "pr-11" : "pr-3.5",
            )}
          />
          {trailing && <span className="absolute top-1/2 right-1.5 -translate-y-1/2">{trailing}</span>}
        </span>
      </label>
      {hint && <p className="mt-1.5 text-xs text-muted-foreground">{hint}</p>}
    </div>
  );
}

function RevealButton({ shown, onToggle, what }: { shown: boolean; onToggle: () => void; what: string }) {
  return (
    <button
      type="button"
      aria-label={`${shown ? "Ukryj" : "Pokaż"} ${what}`}
      aria-pressed={shown}
      onClick={onToggle}
      className="grid size-9 place-items-center rounded-lg text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
    >
      {shown ? <EyeOff className="size-[18px]" aria-hidden /> : <Eye className="size-[18px]" aria-hidden />}
    </button>
  );
}

export function Login() {
  const signIn = useSignIn();
  const [mode, setMode] = useState<Mode>("login");
  const [form, setForm] = useState({ fullName: "", email: "", password: "", organizationName: "" });
  const [showPassword, setShowPassword] = useState(false);
  const [capsLock, setCapsLock] = useState(false);
  const set = (key: keyof typeof form) => (e: { target: { value: string } }) => setForm((f) => ({ ...f, [key]: e.target.value }));

  const isNewAccount = mode === "register";
  const copy = COPY[mode];

  const { submit, error, busy } = useFormSubmit(async () => {
    const body: Record<string, string> = { email: form.email, password: form.password };
    if (isNewAccount) {
      body.fullName = form.fullName;
      body.organizationName = form.organizationName;
    } else {
      const secretKey = storedAdminKey();
      if (secretKey) body.secretKey = secretKey;
    }
    signIn(await api<Me>(copy.path, { method: "POST", body }));
  });

  const switchTo = (next: Mode) => {
    setMode(next);
    setForm((f) => ({ ...f, password: "" }));
  };

  return (
    <div className="relative flex min-h-svh flex-col items-center justify-center overflow-hidden bg-gradient-to-b from-[#f4f8fd] via-[#e6f0fb] to-[#cfe3f8] px-4 py-10">
      <svg viewBox="0 0 1440 320" preserveAspectRatio="none" className="pointer-events-none absolute inset-x-0 bottom-0 h-[34svh] w-full" aria-hidden>
        <path d="M0 150C220 90 420 90 640 140s430 70 800-30v210H0z" fill="#7fb6ee" opacity=".45" />
        <path d="M0 200C260 130 470 170 720 205s440 20 720-70v185H0z" fill="#1673e0" opacity=".55" />
        <path d="M0 255c240-55 480-30 730 5s460 10 710-60v120H0z" fill="#0a2455" />
      </svg>

      <main className="relative w-full max-w-[440px]">
        <div className="mb-7 flex justify-center">
          <Logo size="lg" slogan />
        </div>

        <div className="rounded-2xl bg-white p-7 shadow-[0_24px_60px_-20px_rgb(10_36_85/0.35)] sm:p-8">
          <h1 className="text-center text-2xl">{copy.title}</h1>
          <p className="mt-1 text-center text-sm text-muted-foreground">{copy.subtitle}</p>

          <form className="mt-6 grid gap-4" onSubmit={submit}>
            {isNewAccount && <IconField label="Imię i nazwisko" icon={User} autoComplete="name" value={form.fullName} onChange={set("fullName")} required />}
            {isNewAccount && <IconField label="Nazwa organizacji" icon={Building2} autoComplete="organization" value={form.organizationName} onChange={set("organizationName")} required />}
            <IconField label="E-mail" icon={Mail} type={isNewAccount ? "email" : "text"} inputMode="email" autoComplete="username" placeholder="nazwa@firma.pl" value={form.email} onChange={set("email")} required autoFocus />
            <IconField
              label="Hasło"
              icon={Lock}
              type={showPassword ? "text" : "password"}
              autoComplete={isNewAccount ? "new-password" : "current-password"}
              minLength={isNewAccount ? 12 : undefined}
              hint={capsLock ? "Caps Lock jest włączony." : isNewAccount ? "Co najmniej 12 znaków." : undefined}
              value={form.password}
              onChange={set("password")}
              onKeyUp={(e) => setCapsLock(e.getModifierState("CapsLock"))}
              required
              trailing={<RevealButton shown={showPassword} onToggle={() => setShowPassword((v) => !v)} what="hasło" />}
            />
            <FormError message={error} />
            <Button type="submit" className="mt-1 h-12 w-full rounded-xl text-[0.95rem] font-semibold" disabled={busy}>
              {busy && <Loader2 className="animate-spin" aria-hidden />}
              {copy.submit}
            </Button>
          </form>

          <p className="mt-5 text-center text-sm text-muted-foreground">
            {mode === "login" && (
              <>
                Nie masz konta?{" "}
                <button type="button" className="font-semibold text-primary hover:underline" onClick={() => switchTo("register")}>
                  Załóż konto
                </button>
              </>
            )}
            {mode === "register" && (
              <>
                Masz już konto?{" "}
                <button type="button" className="font-semibold text-primary hover:underline" onClick={() => switchTo("login")}>
                  Zaloguj się
                </button>
              </>
            )}
          </p>
        </div>
      </main>
    </div>
  );
}
