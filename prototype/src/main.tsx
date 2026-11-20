import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { StrictMode, type ReactNode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { SessionProvider, useMeQuery, useSession } from "@/auth";
import { Layout } from "@/components/Layout";
import { LoadState } from "@/components/shared";
import { Toaster } from "@/components/ui/sonner";
import { CalendarPage } from "@/pages/Calendar";
import { ChartersPage } from "@/pages/Charters";
import { CruiseDetailPage, CruisesPage } from "@/pages/Cruises";
import { Dashboard } from "@/pages/Dashboard";
import { FleetPage } from "@/pages/Fleet";
import { Login } from "@/pages/Login";
import { ContactPage, ParticipantsPage } from "@/pages/Participants";
import { CourseDetailPage, CoursesPage } from "@/pages/Courses";
import { SettingsPage } from "@/pages/Settings";
import "./index.css";

const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false, refetchOnWindowFocus: false } } });

function Gate({ module, children }: { module: string; children: ReactNode }) {
  const { hasModule } = useSession();
  return hasModule(module) ? children : <Navigate to="/" replace />;
}

function App() {
  const { data: me, error, isPending } = useMeQuery();
  if (isPending || error) {
    return (
      <div className="p-8">
        <LoadState error={error} />
      </div>
    );
  }
  if (!me) return <Login />;
  return (
    <SessionProvider key={me.id} me={me}>
      <Routes>
        <Route element={<Layout />}>
          <Route index element={<Dashboard />} />
          <Route path="rejsy" element={<Gate module="rejsy"><CruisesPage /></Gate>} />
          <Route path="rejsy/:id" element={<Gate module="rejsy"><CruiseDetailPage /></Gate>} />
          <Route path="flota" element={<Gate module="flota"><FleetPage /></Gate>} />
          <Route path="czartery" element={<Gate module="czartery"><ChartersPage /></Gate>} />
          <Route path="kalendarz" element={<Gate module="kalendarz"><CalendarPage /></Gate>} />
          <Route path="szkolenia" element={<Gate module="szkolenia"><CoursesPage /></Gate>} />
          <Route path="szkolenia/:id" element={<Gate module="szkolenia"><CourseDetailPage /></Gate>} />
          <Route path="uczestnicy" element={<Gate module="uczestnicy"><ParticipantsPage /></Gate>} />
          <Route path="uczestnicy/:id" element={<Gate module="uczestnicy"><ContactPage /></Gate>} />
          <Route path="ustawienia" element={<SettingsPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
    </SessionProvider>
  );
}

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <App />
      </BrowserRouter>
      <Toaster />
    </QueryClientProvider>
  </StrictMode>,
);
