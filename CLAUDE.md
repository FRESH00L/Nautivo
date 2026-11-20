# Nautivo

Komercyjny SaaS dla branży żeglarskiej i wodnej: system dla organizacji (moduły aktywowane per firma) oraz publiczny marketplace. Slogan: „Sail. Book. Explore.”

Projekt prowadzi jedna osoba przy ograniczonym budżecie. Celem jest produkt, za który organizatorzy zapłacą, a nie jak największy zakres funkcji.

## Etap projektu

Trwa budowa pierwszej działającej wersji panelu: `backend/` (ASP.NET Core, EF Core, PostgreSQL) i `prototype/` (React). Moduły na start: dashboard, rejsy, flota, kalendarz, uczestnicy. Wpłaty zapisywane są ręcznie, bez operatora płatności. Wersja służy do pokazów i testów na danych fikcyjnych; nie uruchamiamy jej na prawdziwych danych osobowych przed przeglądem bezpieczeństwa i RODO.

Na później (założyciel wraca do tego później): marketplace, wywiady i sprzedaż, cennik, prawo, prawdziwe płatności, dalsze dokumenty.

## Uruchamianie

- Baza: `docker compose up -d` w katalogu głównym; sekrety są w niecommitowanym `.env`.
- Backend: `dotnet run --project backend/src/Nautivo.Api`. Testy: `dotnet test backend` (wymagają Dockera).
- Frontend: `npm run dev` w `prototype/`.

## Priorytety (w tej kolejności)

1. Zdobycie pierwszych klientów
2. Rozwiązanie realnych problemów organizatorów
3. Bezpieczeństwo danych
4. Architektura multi-tenant
5. Prosta obsługa
6. Przychody z abonamentów
7. Naliczanie prowizji
8. Rozbudowa marketplace'u
9. Skalowanie

## Zasady dokumentacji

- Dokumenty piszemy po polsku, w `/docs`. Diagramy w Mermaid, w `/docs/diagrams`.
- Nie wymyślamy danych. Czego nie wiemy, oznaczamy jako nieznane.
- Oddzielamy fakty od założeń i rekomendacji.
- Przy danych rynkowych, cenach, przepisach i wersjach technologii podajemy źródło i datę weryfikacji. Preferujemy źródła urzędowe, oficjalną dokumentację i strony producentów.
- Prognozy nie zawierają gwarantowanych konwersji; założenia są jawne.
- Kwestie prawne i podatkowe oznaczamy do potwierdzenia przez prawnika lub księgowego.
- Pomysły założyciela oceniamy krytycznie. Przy rekomendacjach podajemy uzasadnienie, ryzyka i alternatywy.

## Kierunek techniczny (do potwierdzenia w dokumentacji)

Preferowany stack: ASP.NET Core (.NET 10 LTS), C#, EF Core, PostgreSQL, React, TypeScript, Tailwind, shadcn/ui. Modularny monolit, bez mikroserwisów. Rozwiązania optymalizujemy pod jednoosobowe utrzymanie.

## Twarde zasady na etap kodu

- Każdy dostęp do danych organizacji jest ograniczony do jej tenanta; izolację sprawdzają testy automatyczne.
- Aplikacja i testy izolacji działają na zwykłej roli bazy, a nie na superuserze czy właścicielu tabel, bo te mogą omijać RLS.
- Kwoty nie są przechowywane jako liczby zmiennoprzecinkowe.
- Webhooki płatności są idempotentne i weryfikowane podpisem.
- Dane szczególnych kategorii (np. zdrowotne) nie są zbierane bez analizy prawnej i uzasadnionej potrzeby.

## Bezpieczeństwo pracy

- Nie odczytujemy ani nie commitujemy sekretów (`.env`, klucze API).
- Pracujemy tylko na lokalnej bazie i danych testowych. Narzędzia AI nie łączą się z bazą produkcyjną.
- Integracje płatnicze testujemy wyłącznie w sandboxie.
- Commit i push wykonujemy tylko na wyraźną prośbę.
