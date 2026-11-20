import { useQuery, useQueryClient } from "@tanstack/react-query";
import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { api, ApiError, setApiOrganization, type Me, type OrganizationInfo } from "@/api";

const STORAGE_KEY = "nautivo.organization";
const stored = () => {
  try {
    return localStorage.getItem(STORAGE_KEY);
  } catch {
    return null;
  }
};

interface Session {
  me: Me;
  organization: OrganizationInfo | null;
  selectOrganization: (id: string) => void;
  hasModule: (module: string) => boolean;
  signIn: (me: Me) => void;
  signOut: () => Promise<void>;
}

const SessionContext = createContext<Session | null>(null);

export function useMeQuery() {
  return useQuery({
    queryKey: ["me"],
    queryFn: async () => {
      try {
        return await api<Me>("/auth/me");
      } catch (error) {
        if (error instanceof ApiError && error.status === 401) return null;
        throw error;
      }
    },
    retry: false,
  });
}

export function useSignIn() {
  const client = useQueryClient();
  return (me: Me) => {
    client.removeQueries({ predicate: (q) => q.queryKey[0] !== "me" });
    client.setQueryData(["me"], me);
  };
}

export function SessionProvider({ me, children }: { me: Me; children: ReactNode }) {
  const client = useQueryClient();
  const [selectedId, setSelectedId] = useState<string | null>(stored);
  const organization = me.organizations.find((o) => o.id === selectedId) ?? me.organizations[0] ?? null;
  // Ustawione w trakcie renderu, żeby zapytania potomnych widoków szły już do właściwej organizacji.
  setApiOrganization(organization?.id ?? null);

  useEffect(() => {
    try {
      if (organization) localStorage.setItem(STORAGE_KEY, organization.id);
    } catch {
      // Brak dostępu do localStorage oznacza tylko, że wybór nie przetrwa odświeżenia.
    }
  }, [organization]);

  const value = useMemo<Session>(
    () => ({
      me,
      organization,
      selectOrganization: (id) => {
        setApiOrganization(id);
        client.removeQueries({ predicate: (q) => q.queryKey[0] !== "me" });
        setSelectedId(id);
      },
      hasModule: (module) => me.isAdmin || !!organization?.modules.includes(module),
      signIn: (next) => client.setQueryData(["me"], next),
      signOut: async () => {
        await api("/auth/logout", { method: "POST" });
        setApiOrganization(null);
        // clear() odpiąłby aktywne zapytanie o profil i ekran nie wróciłby do logowania.
        client.removeQueries({ predicate: (q) => q.queryKey[0] !== "me" });
        client.setQueryData(["me"], null);
      },
    }),
    [client, me, organization],
  );

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

export function useSession() {
  const session = useContext(SessionContext);
  if (!session) throw new Error("useSession poza SessionProvider");
  return session;
}
