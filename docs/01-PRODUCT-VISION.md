# 01. Wizja produktu

**Status:** etap B, wersja robocza. Data: 2026-11-05. Podstawa: [02](02-MARKET-RESEARCH.md), [03](03-COMPETITOR-ANALYSIS.md), [21](21-LEGAL-CHECKLIST.md). To, co nie ma źródła, jest hipotezą do sprawdzenia w wywiadach.

## Wizja

Nautivo pozwala organizatorowi rejsu, kursu lub wydarzenia żeglarskiego sprzedać miejsca i odebrać wpłaty bez formularzy, arkuszy i ręcznego pilnowania rat. Z czasem oferty organizatorów tworzą publiczną wyszukiwarkę.

Slogan „Sail. Book. Explore.” pozostaje nazwą docelowej obietnicy. Pierwsza wersja realizuje tylko „Book”.

## Dla kogo najpierw

| Kolejność | Segment | Rola | Dlaczego |
|---|---|---|---|
| 1 | Stowarzyszenia i kluby, do których masz dostęp | Partnerzy pilotażu, nie źródło przychodu | Szybki dostęp i testy; budżety nieznane, a mediana przychodu NGO to 50 000 zł rocznie |
| 2 | Szkoły i organizatorzy kursów | Pierwsi płacący (hipoteza) | Około 210 podmiotów z uprawnieniami egzaminacyjnymi jest na publicznej liście; nie znaleziono polskiego produktu branżowego |
| 3 | Organizatorzy rejsów na koje | Pierwsi płacący (hipoteza) | Nie potwierdzono produktu sprzedającego pojedyncze koje, wyższa wartość rezerwacji; ryzyko prawne imprezy turystycznej |
| Później | Czarter i wynajem | Po zdobyciu pozycji | Rynek zajęty: Boater, Portivo, yachtCMS, NaCzarter |
| Później | Użytkownik końcowy marketplace'u | Po zebraniu ofert | Zimny start i dodatkowe obowiązki prawne |

Luka w segmentach 2 i 3 wynika głównie z braku wyników wyszukiwania. Może też oznaczać, że organizatorzy nie chcą za to płacić.

## Problem do potwierdzenia

Hipoteza brzmi: organizator traci czas i pieniądze na ręczne zapisy, listę rezerwową, rozliczanie zaliczek i salda, ponaglenia oraz listy uczestników.

Potwierdzony jest tylko jeden przypadek: Klub Żeglarski UW zbiera zapisy przez Formularze Google. Skala problemu i gotowość do płacenia pozostają nieznane, dlatego wywiady z [19](19-CUSTOMER-ACQUISITION.md) są warunkiem pisania produktu.

## Obietnica pierwszej wersji

Jeden rdzeń: **termin z limitem miejsc → zapis → lista rezerwowa → zaliczka i saldo → przypomnienia → lista uczestników.** Ten sam rdzeń obsługuje kurs i rejs.

Zakres funkcji doprecyzuje etap C.

## Czego świadomie nie robimy na start

| Odkładamy | Powód |
|---|---|
| Publiczny marketplace | Wymaga rejestracji, testu koszyka i opinii o roli agenta; brak popytu na start |
| Moduł czarteru i protokoły | Rynek zajęty, trudno wygrać ceną |
| Kaucje | Brak pewnego rozwiązania technicznego i prawnego; organizator rozlicza je sam |
| Przyjmowanie wpłat na rachunek Nautivo | Ryzyko usługi płatniczej; **nigdy w tym modelu** |
| Koszyk wielu organizatorów i cross-sell | Ryzyko statusu organizatora lub usług powiązanych |
| Dane zdrowotne | Wymagają analizy prawnej i uzasadnionej potrzeby |
| Aplikacja mobilna | Wystarczy responsywna strona |

## Pozycjonowanie (do testu)

Dla organizatorów kursów i rejsów, którzy dziś zbierają zapisy formularzem i pilnują wpłat ręcznie, Nautivo jest polskim systemem zapisów z ratami i listą rezerwową. W odróżnieniu od systemów czarterowych liczy miejsca i koje, a nie całe jachty.

Nie używamy haseł „pierwszy”, „jedyny” ani „brak konkurencji”, dopóki nie ma na to dowodów.

## Zasady produktu

1. Organizator konfiguruje termin w kilka minut, bez wdrożenia.
2. Moduły włącza się osobno, ale wszystkie korzystają z jednego rdzenia.
3. Pieniądze uczestnika trafiają do organizatora przez licencjonowanego operatora.
4. Każda oferta jasno wskazuje, kto jest organizatorem i sprzedawcą.
5. Zbieramy tylko dane, które są potrzebne.

## Miary sukcesu

Progi są propozycją decyzyjną, a nie prognozą.

| Bramka | Sygnał | Co dalej |
|---|---|---|
| Walidacja | Co najmniej 5 z 10–20 rozmówców opisuje ten sam problem, a 3 komercyjnych organizatorów akceptuje konkretną cenę | Budowa pilotażu |
| Pilotaż | Jeden prawdziwy termin przechodzi od zapisu do listy uczestników bez pomocy założyciela | Sprzedaż |
| Płatność | 5 płacących klientów po zakończeniu okresu próbnego | Rejestracja i prowizja przez operatora |
| Marketplace | Co najmniej 20 aktywnych organizatorów i opinia prawna | Publiczna wyszukiwarka |

## Warunek przerwania

Jeśli organizatorzy wolą zostać przy formularzach nawet przy niskiej cenie, należy zmienić segment albo zakończyć projekt, zanim powstanie moduł płatności.
