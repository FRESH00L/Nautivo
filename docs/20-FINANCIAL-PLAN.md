# 20. Plan finansowy

**Status:** etap B, model założeń. Data: 2026-11-05. **To scenariusze arytmetyczne, a nie prognozy.** Nie ma żadnych danych o sprzedaży Nautivo ani o gotowości klientów do płacenia. Liczby nie obejmują podatku dochodowego, VAT ani wynagrodzenia założyciela.

## Najważniejsze wnioski

1. **Główne ryzyko nie leży w kosztach utrzymania.** Te są niskie, a modele osiągają dodatni wynik już przy kilku klientach. Ryzykiem jest to, czy w ogóle znajdą się płacący klienci.
2. **Etat bardzo pomaga.** Bez niego koszt pełnego ZUS przesuwa próg ze skali 1–2 klientów na 13 w scenariuszu realistycznym i na 94 w pesymistycznym.
3. **Odejście z pracy jest odległe.** Około 8 000 zł miesięcznie zysku przed podatkiem wymaga mniej więcej 58 klientów w scenariuszu realistycznym i 309 w pesymistycznym.
4. **Prowizja jest w modelu duża, ale niepotwierdzona.** Bez niej cel 8 000 zł rośnie w scenariuszu realistycznym do 171 klientów.
5. **Czas założyciela jest ograniczeniem wcześniej niż pieniądze.** Około 38 godzin wsparcia przy 50 klientach przekracza całe około 30 godzin miesięcznie, które masz na projekt.

## Założenia scenariuszy

| Założenie | Pesymistyczny | Realistyczny | Optymistyczny |
|---|---:|---:|---:|
| Średni abonament netto/mies. | 79 zł | 129 zł | 179 zł |
| Płatności online na klienta/rok | 15 000 zł | 50 000 zł | 150 000 zł |
| Wsparcie h/klient/mies. | 1,0 | 0,75 | 0,5 |

„Realistyczny” oznacza założenie robocze, a nie najbardziej prawdopodobny wynik. Przedział obrotu oparto na małej próbie cen i medianie przychodu NGO (50 000 zł rocznie, dane niebranżowe). Dla porównania: 50 000 zł to około 18 koi po 2 750 zł.

| Parametr wspólny | Wartość | Status |
|---|---|---|
| Prowizja | 3%, pomniejszona o 5% zwrotów | Zwroty to założenie |
| Opłata operatora od płatności uczestnika | Ponosi organizator | Rekomendacja |
| Opłata za pobranie abonamentu | 1,9% + 1 zł | Oparta na [Stripe](https://stripe.com/pl/pricing) (karta premium/P24); zastąpić stawką faktycznie używaną |
| Kursy | 1 USD = 3,70 zł, 1 EUR = 4,30 zł | Założenie, kursów nie weryfikowano |
| Wsparcie darmowe | 10 h/mies., powyżej 60 zł/h | Założenie |

### Koszty stałe według liczby klientów

| Pozycja (zł/mies.) | 5 | 10 | 25 | 50 | 100 | 200 | Podstawa |
|---|---:|---:|---:|---:|---:|---:|---|
| Forma: składki | 0 | 498 | 831 | 831 | 831 | 2 619 | Portale księgowe; patrz niżej |
| Księgowość | 0 | 250 | 400 | 400 | 400 | 600 | Założenie, tylko 600 zł z modelu Norman |
| Pobieranie abonamentu | 17 | 35 | 86 | 173 | 345 | 690 | Jak wyżej |
| Hosting, baza, storage | 60 | 60 | 100 | 180 | 350 | 650 | Założenie; architektura w etapie D |
| E-mail | 0 | 0 | 0 | 74 | 74 | 130 | [Resend](https://resend.com/pricing): 0 / 20 / 35 USD |
| SMS (20 na klienta) | 14 | 27 | 68 | 135 | 270 | 540 | [OVH](https://www.ovhcloud.com/pl/sms/) 0,135 zł; SMSAPI niezweryfikowane |
| Narzędzie AI | 74 | 74 | 74 | 370 | 370 | 370 | [Claude Pro lub Max](https://claude.com/pricing) |
| Wsparcie | 0 | 0 | 525 | 1 650 | 3 900 | 8 400 | Założenie |
| Marketing | 50 | 100 | 300 | 800 | 1 500 | 3 000 | Założenie |
| Prawo, ubezpieczenie | 0 | 0 | 100 | 100 | 300 | 500 | Założenie, rynek niezbadany |

Tabela dotyczy scenariusza realistycznego. Hosting zakłada mały serwer, na przykład [Hetzner CX23 około 4–6 EUR miesięcznie](https://betterstack.com/community/guides/web-servers/hetzner-cloud-review/) plus kopie zapasowe i storage; źródła są sprzeczne, więc sprawdź cenę przed zakupem.

**Forma w modelu:**
- działalność nierejestrowana, jeśli przychód mieści się w limicie, do 5 klientów;
- JDG przy etacie: 498,35 zł miesięcznie do 60 000 zł przychodu, 830,58 zł do 300 000 zł. To składki zdrowotne na ryczałcie z 2026, wyliczone z tabel składek, przy założeniu pensji co najmniej minimalnej;
- powyżej 300 000 zł model sp. z o.o.: 2 618,87 zł plus 600 zł księgowości.

Stawki wzrosną w 2027, a podatek od przychodu nie jest ujęty. **[KSIĘGOWY]**

## Wyniki

Kwoty w złotych, miesięcznie, poza ARR.

### Pesymistyczny: 79 zł, 15 000 zł rocznie na klienta

| Klienci | Forma | MRR abonament | ARR abonament | Prowizja/mies. | Przychód/mies. | Koszty/mies. | Zysk operacyjny/mies. | Wsparcie h/mies. |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| 5 | nierej. | 395 | 4 740 | 178 | 573 | 210 | 363 | 5 |
| 10 | JDG+etat | 790 | 9 480 | 356 | 1 146 | 1 034 | 112 | 10 |
| 25 | JDG+etat | 1 975 | 23 700 | 891 | 2 866 | 2 352 | 513 | 25 |
| 50 | JDG+etat | 3 950 | 47 400 | 1 781 | 5 731 | 5 415 | 317 | 50 |
| 100 | JDG+etat | 7 900 | 94 800 | 3 562 | 11 462 | 9 745 | 1 718 | 100 |
| 200 | JDG+etat | 15 800 | 189 600 | 7 125 | 22 925 | 18 320 | 4 605 | 200 |

Po skokach kosztów wynik chwilowo spada poniżej zera; trwale dodatni jest od około 107 klientów.

### Realistyczny: 129 zł, 50 000 zł rocznie na klienta

| Klienci | Forma | MRR abonament | ARR abonament | Prowizja/mies. | Przychód/mies. | Koszty/mies. | Zysk operacyjny/mies. | Wsparcie h/mies. |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| 5 | nierej. | 645 | 7 740 | 594 | 1 239 | 215 | 1 024 | 4 |
| 10 | JDG+etat | 1 290 | 15 480 | 1 187 | 2 478 | 1 044 | 1 434 | 8 |
| 25 | JDG+etat | 3 225 | 38 700 | 2 969 | 6 194 | 2 483 | 3 710 | 19 |
| 50 | JDG+etat | 6 450 | 77 400 | 5 938 | 12 388 | 4 712 | 7 675 | 38 |
| 100 | JDG+etat | 12 900 | 154 800 | 11 875 | 24 775 | 8 340 | 16 435 | 75 |
| 200 | sp. z o.o. | 25 800 | 309 600 | 23 750 | 49 550 | 17 499 | 32 051 | 150 |

### Optymistyczny: 179 zł, 150 000 zł rocznie na klienta

| Klienci | Forma | MRR abonament | ARR abonament | Prowizja/mies. | Przychód/mies. | Koszty/mies. | Zysk operacyjny/mies. | Wsparcie h/mies. |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| 5 | nierej. | 895 | 10 740 | 1 781 | 2 676 | 220 | 2 457 | 2 |
| 10 | JDG+etat | 1 790 | 21 480 | 3 562 | 5 352 | 1 536 | 3 817 | 5 |
| 25 | JDG+etat | 4 475 | 53 700 | 8 906 | 13 381 | 2 132 | 11 249 | 12 |
| 50 | sp. z o.o. | 8 950 | 107 400 | 17 812 | 26 762 | 5 998 | 20 765 | 25 |
| 100 | sp. z o.o. | 17 900 | 214 800 | 35 625 | 53 525 | 8 923 | 44 602 | 50 |
| 200 | sp. z o.o. | 35 800 | 429 600 | 71 250 | 107 050 | 14 689 | 92 361 | 100 |

## Warianty wykorzystania rezerwacji

Prowizja miesięczna przy 3%, po 5% zwrotów, średnio w roku.

| Obrót online klienta/rok | 5 | 10 | 25 | 50 | 100 | 200 |
|---:|---:|---:|---:|---:|---:|---:|
| 10 000 zł | 119 | 238 | 594 | 1 187 | 2 375 | 4 750 |
| 30 000 zł | 356 | 712 | 1 781 | 3 562 | 7 125 | 14 250 |
| 50 000 zł | 594 | 1 187 | 2 969 | 5 938 | 11 875 | 23 750 |
| 100 000 zł | 1 187 | 2 375 | 5 938 | 11 875 | 23 750 | 47 500 |
| 250 000 zł | 2 969 | 5 938 | 14 844 | 29 687 | 59 375 | 118 750 |
| 500 000 zł | 5 938 | 11 875 | 29 687 | 59 375 | 118 750 | 237 500 |

To średnie miesięczne, a prowizja jest silnie sezonowa. Przy 5 klientach prowizja nie przejdzie przez Connect bez rejestracji.

## Próg rentowności

Liczba klientów, od której zysk operacyjny w modelu pierwszy raz jest nieujemny. To wynik bez wynagrodzenia założyciela i bez kosztu czasu budowy.

| Scenariusz | Pierwszy zysk | Bez prowizji | Bez etatu (pełny ZUS) | 8 000 zł/mies. przed podatkiem | 8 000 zł bez prowizji |
|---|---:|---:|---:|---:|---:|
| Pesymistyczny | 2 | 3 | 94 | 309 | ponad 999 |
| Realistyczny | 1 | 2 | 13 | 58 | 171 |
| Optymistyczny | 1 | 2 | 5 | 19 | 80 |

Rynek obejmuje 548–936 policzalnych organizacji, a wiele z nich to małe kluby. Dlatego 300 płacących klientów oznaczałoby nierealnie wysoki udział, przynajmniej bez rozszerzenia oferty. Pesymistyczny wariant nie zastępuje etatu.

8 000 zł to przykładowy poziom do porównania, a nie cel. Podaj swój, jeśli ma być inny.

## Budżet startowy 5 000 zł, 12 miesięcy

| Pozycja | Kwota | Uwaga |
|---|---:|---|
| Domena i podstawy | 150 zł | Założenie; dostępność `nautivo.pl` i znak towarowy do sprawdzenia |
| Hosting i kopie zapasowe | 720 zł | Maksymalnie 60 zł/mies.; startuje dopiero przy pilotażu |
| Narzędzie AI (Claude Pro) | 890 zł | 20 USD/mies. **Plan Max kosztowałby około 4 440 zł rocznie, prawie cały budżet** |
| Księgowy, jednorazowa konsultacja | 500 zł | Założenie, wycena do sprawdzenia |
| Materiały | 300 zł | |
| Dojazdy i wydarzenia | 600 zł | |
| Test reklam | 500 zł | Tylko po działającym lejku |
| **Rezerwa** | **1 340 zł** | Na wyższe rachunki, opłaty lub JDG |
| **Razem** | **5 000 zł** | Bez konsultacji prawnej, którą finansujesz osobno |

**Rezerwa na JDG:** przy etacie składka zdrowotna to około 433–498 zł miesięcznie. Rezerwa pokrywa około 3 miesięcy. Dlatego rejestracja powinna nastąpić wtedy, gdy płacą klienci, nie wcześniej.

**Twój czas:** 7 h tygodniowo to około 364 h rocznie na kod, sprzedaż, wsparcie, prawo i decyzje. AI skraca programowanie, ale nie zastąpi rozmów z klientami, przeglądów ani odpowiedzialności. Koszt budowy MVP zostanie oszacowany na etapie D.

## Oś czasu

Poniższe liczby to tylko oceny robocze. Nie ma danych historycznych na ich poparcie.

| Moment | Pesymistycznie | Roboczo | Optymistycznie |
|---|---|---|---|
| Po 12 miesiącach | Pilotaż, 0 płacących | 1–5 płacących | Około 10 płacących |
| Po 24 miesiącach | 5 płacących lub zamknięcie | 10–25 płacących | Około 50 płacących |

Sprzedaż jest ograniczona sezonem, a przy 7 h tygodniowo zaczyna się dopiero po pierwszych rozmowach i pilotażu.

## Kiedy przestać

Propozycje progów do zatwierdzenia:

- Po 20 rozmowach mniej niż 3 zobowiązania cenowe: zmienić segment lub ofertę.
- Po pierwszym pełnym sezonie mniej niż 3 płacących: pauza przed dalszym budowaniem.
- Rezerwa spada do 500 zł bez przychodu: wstrzymać wydatki poza utrzymaniem pilotażu.

## Czego model nie uwzględnia

Podatku dochodowego, VAT, wynagrodzenia założyciela, odejść klientów, kosztu pozyskania poza budżetem marketingu, kosztów incydentów, audytu bezpieczeństwa i zmian stawek. Po pierwszym sezonie należy zastąpić założenia danymi.
