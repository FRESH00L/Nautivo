# CRM w Nautivo: co to jest, co już mamy i na co uważać

Data przygotowania: 2026-11-09. Kwestie prawne wymagają potwierdzenia przez prawnika.

## Czy CRM to po prostu panel klientów?

Nie. Panel klientów to lista osób i ich danych. CRM (Customer Relationship Management) odpowiada na trzy pytania o każdego klienta:

1. **Kim jest i jak do niego dotrzeć?** Kontakt, źródło pozyskania, tagi, zgody.
2. **Co się z nim działo?** Rejsy, wpłaty, rozmowy, e-maile i notatki w jednej osi czasu.
3. **Co mamy zrobić dalej?** Zadania z terminami, czyli kogo dziś przypomnieć, do kogo oddzwonić, komu wysłać ofertę.

Trzeci punkt odróżnia CRM od książki adresowej. U organizatora rejsów największe pieniądze leżą w powracających klientach i poleceniach. CRM ma pilnować, żeby nikt nie wypadł z obiegu. Ten punkt to rekomendacja, nie zmierzona prawidłowość: nie mamy danych rynkowych o odsetku powrotów w tej branży.

## Co jest w prototypie (moduł „Klienci”)

| Obszar | Co działa |
|---|---|
| Etap relacji | Liczony automatycznie z historii rejsów: potencjalny (bez rejsu), klient (1 rejs), stały klient (2+), do odzyskania (ostatni rejs ponad rok temu, nic w planach). Nikt nie musi go przestawiać ręcznie. |
| Segmentacja | Tagi (do 10 na osobę), źródło pozyskania, filtry: etap, tag, zgoda marketingowa, niedopłata. |
| Historia kontaktu | Notatki, telefony, e-maile i spotkania z autorem i datą. Zapisy na rejsy i wpłaty dopisują się do osi czasu automatycznie. |
| Zadania | Przypomnienia z terminem przy kliencie. Na liście klientów jest panel „Do zrobienia” z zadaniami po terminie wyróżnionymi na czerwono. |
| Zgody | Zgoda marketingowa z datą odnotowania. Każda zmiana zgody zapisuje się w historii jako wpis systemowy, którego nie da się usunąć. |
| Prawa osoby (RODO) | Eksport wszystkich danych osoby do pliku JSON (dostęp i przenoszenie). Anonimizacja: znikają dane osobowe, zostają kwoty i daty wpłat. Usunięcie całkowite, gdy osoba nie ma historii. |
| Eksport listy | CSV z aktualnie przefiltrowanej listy, gotowy do otwarcia w Excelu. |
| Izolacja | Każda organizacja widzi tylko swoich klientów, notatki i zadania. Sprawdzają to testy automatyczne. |

## Na co zwracać uwagę przy budowie CRM

### 1. Model danych

- **Jedna osoba to jeden rekord.** Ta sama osoba płynie wiele razy. Duplikaty („Jan Kowalski” trzy razy) psują statystyki i prowadzą do wysyłania tej samej oferty dwa razy. Do zrobienia: ostrzeżenie o duplikacie po e-mailu lub telefonie przy dodawaniu oraz scalanie rekordów.
- **Etapy liczone, nie wpisywane.** Etap wpisywany ręcznie po miesiącu przestaje być aktualny. Tam, gdzie się da, wyliczaj go z faktów, czyli z rejsów i wpłat.
- **Uczestnik a płatnik.** Rodzic płaci za dziecko, firma za pracownika. Dziś każdy zapis ma jednego uczestnika. Relacje między osobami („opiekun”, „firma”) to naturalny następny krok, ale dopiero gdy klienci o to poproszą.
- **B2B osobno.** Kluby, szkoły i firmy zamawiające rejsy integracyjne to inny rodzaj klienta: organizacja z kilkoma osobami kontaktowymi. Nie mieszać tego z osobą prywatną na siłę.

### 2. Dane osobowe i RODO (do potwierdzenia przez prawnika)

- **Minimalizacja.** Zbieraj tylko to, czego używasz. Dodatkowe pole to dodatkowe ryzyko i obowiązek.
- **Szczególne kategorie danych.** Zdrowie, leki, alergie, dieta wynikająca z choroby czy niepełnosprawność to dane szczególnej kategorii (art. 9 RODO). Zgodnie z zasadami projektu nie zbieramy ich bez analizy prawnej. Formularz klienta ostrzega przed wpisywaniem ich do notatek.
- **Kontakt alarmowy to dane innej osoby.** Ona nie zawierała z nami umowy. Wpisuj go tylko na czas rejsu i rozważ usuwanie po rejsie.
- **Podstawa przetwarzania.** Obsługa rejsu (zapis, wpłaty, kontakt w sprawie rejsu) to zwykle wykonanie umowy. Marketing (oferty nowych rejsów) to osobna sprawa.
- **Zgoda na marketing elektroniczny.** Wysyłanie informacji handlowej e-mailem lub SMS-em wymaga uprzedniej zgody adresata. Od 10.11.2024 podstawą jest art. 398 ustawy Prawo komunikacji elektronicznej (Dz. U. 2024 poz. 1221). Zgoda powinna być osobna, dobrowolna i możliwa do wykazania, dlatego zapisujemy datę i historię zmian. Nie ustaliliśmy, czy art. 10 ustawy o świadczeniu usług drogą elektroniczną nadal obowiązuje. Źródła: [ifirma.pl](https://www.ifirma.pl/blog/prawo-komunikacji-elektronicznej-juz-opublikowane-nowe-zasady-zgod-marketingowych-w-2024-roku/), [legeartis.org](https://czasopismo.legeartis.org/2024/08/przesylanie-informacji-handlowych-prawo-komunikacji-elektronicznej-uprzednia-zgoda-marketing-bezposredni/), [eli.gov.pl, Dz. U. 2024 poz. 1803](https://eli.gov.pl/eli/DU/2024/1803/ogl/pol/pdf). Wyszukiwanie z 2026-11-09; tekstów ujednoliconych nie sprawdzaliśmy.
- **Prawa osoby.** Dostęp, sprostowanie, usunięcie, przenoszenie i sprzeciw wobec marketingu. Eksport i anonimizacja są już w aplikacji.
- **Retencja.** Jak długo trzymać dane po ostatnim rejsie? Dokumenty księgowe trzeba trzymać latami, notatek marketingowych nie. Termin do ustalenia z prawnikiem i księgowym. Potem można dodać automatyczne przypomnienie o anonimizacji.
- **Nautivo jako procesor.** Organizator jest administratorem danych swoich klientów, a Nautivo je przetwarza w jego imieniu. Przed pierwszym klientem potrzebna jest umowa powierzenia przetwarzania (art. 28 RODO).
- **Eksporty to wyciek w zasięgu ręki.** Plik CSV z listą klientów na pulpicie czy w mailu to częsta droga wycieku. W aplikacji jest ostrzeżenie. Później warto dodać zapis, kto i kiedy eksportował.

### 3. Bezpieczeństwo i dostęp

- Izolacja organizacji jest już sprawdzana testami.
- **Role:** czy instruktor ma widzieć wpłaty i notatki handlowe? Na razie wszyscy w organizacji widzą wszystko. Role to pierwszy krok, gdy pojawią się firmy z kilkoma pracownikami.
- **Dziennik zmian:** kto zmienił dane klienta i kiedy. Przy kilku osobach w zespole pozwala wyjaśnić pomyłki i zgłoszenia RODO.

### 4. Codzienna użyteczność

- **Wpisanie notatki musi zająć 10 sekund.** Jeśli zajmuje dłużej, nikt nie będzie ich wpisywał, a CRM bez historii jest pusty.
- **Zadania mają termin.** „Kiedyś oddzwonić” nie działa. Panel „Do zrobienia” to pierwsza rzecz, którą organizator powinien widzieć rano.
- **Telefon.** Organizator często jest na kei, a nie przy biurku. Karta klienta musi działać na telefonie; linki `tel:` i `mailto:` już są.
- **Import.** Każdy organizator ma dziś Excela albo Google Sheets z klientami. Bez importu z CSV nikt nie przeniesie się do nowego narzędzia. To najważniejsza brakująca funkcja, jeśli celem jest pozyskanie pierwszych klientów.

### 5. Czego nie budować teraz

- Tablicy Kanban z lejkiem sprzedaży. Pasuje do sprzedaży B2B z długim cyklem, a organizator rejsów ma głównie zapisy indywidualne.
- Własnej wysyłki newsletterów. To osobny produkt z dostarczalnością, wypisami i reputacją domeny. Lepiej eksport segmentu do istniejącego narzędzia, a integrację dopiero na wyraźną prośbę.
- Rozbudowanej punktacji klientów (lead scoring). Przy setkach klientów wystarczą etap i tagi.

## Proponowana kolejność dalszych prac

1. Import klientów z CSV (z wykrywaniem duplikatów).
2. Ostrzeżenie o duplikacie i scalanie rekordów.
3. Szablony wiadomości (przypomnienie o dopłacie, oferta dla „do odzyskania”) do skopiowania lub otwarcia w kliencie poczty.
4. Uzgodnienie z prawnikiem: retencja, treść klauzul i zgód, umowa powierzenia. Potem automatyczne przypomnienia o anonimizacji.
5. Role w organizacji i dziennik zmian.

## Do weryfikacji

- Czy art. 398 PKE i RODO wyczerpują wymagania dla marketingu e-mail i SMS u organizatorów rejsów? Do potwierdzenia przez prawnika.
- Okres przechowywania danych po rejsie i po ostatnim kontakcie. Do potwierdzenia przez prawnika i księgowego.
- Czy anonimizacja w obecnej postaci (dane osobowe usunięte, kwoty i daty wpłat zachowane) spełnia żądanie usunięcia danych przy obowiązkach księgowych? Do potwierdzenia przez prawnika.
