# 04. Model biznesowy

**Status:** etap B, wersja robocza. Data: 2026-11-05. To nie jest porada prawna ani podatkowa. **[PRAWNIK]** i **[KSIĘGOWY]** oznaczają punkty do potwierdzenia.

## Model w skrócie

Nautivo sprzedaje organizatorom oprogramowanie za abonament i pobiera 3% od każdej zrealizowanej płatności online. Organizator jest sprzedawcą oraz klientem operatora płatności, a Nautivo nie przyjmuje pieniędzy uczestników.

| Strumień | Rola | Uwaga |
|---|---|---|
| Abonament | Pokrywa koszty stałe | Działa także poza sezonem |
| Prowizja 3% | Rośnie wraz z obrotem klienta | Sezonowa i nieobecna przy płatnościach offline |
| Wdrożenie, dodatki | Uzupełnienie | Bez doradztwa, które mogłoby znieść zwolnienie z VAT **[KSIĘGOWY]** |

## Czy 3% to dobra stawka?

**Stawka jest do obrony, ale nie jest przewagą.**

- Jest znacznie niższa niż prowizje marketplace'ów (około 11,5–29%).
- Jest równa Portivo (149 zł + 3%), Rezdy i Checkfront.
- Jest wyższa niż Boater (299 zł + 1,5%) i yachtCMS (0%).
- Razem z opłatą operatora organizator oddaje łącznie około 4–5% płatności przy 800 zł (ceny katalogowe), zależnie od operatora i metody.

**Rekomendacja:** zostawić 3%, ale sprzedawać oszczędność czasu i mniej ręcznej pracy, a nie cenę. W wywiadach sprawdzić, czy łączny koszt jest akceptowalny.

## Zasady prowizji

Pozycje oznaczone „decyzja” wymagają Twojego zatwierdzenia.

| Zasada | Propozycja |
|---|---|
| Podstawa | Kwota każdej skutecznej płatności online: zaliczki, raty i salda osobno |
| Przykład z briefu | 800 + 1000 + 1200 zł daje 24 + 30 + 36 = **90 zł** |
| Opłata operatora | Ponosi organizator, potrącana z jego wpływu |
| Płatności offline | 0%, bo nie da się ich rzetelnie zweryfikować (**decyzja**; brief mówi o każdej wpłacie) |
| Zwrot | Zwrot proporcjonalnie cofa 3%; opłata operatora nie wraca (**decyzja**) |
| Chargeback | Jak zwrot, koszt sporu po stronie organizatora (**decyzja**, do sprawdzenia z operatorem) |
| Zaokrąglenie | Do 1 grosza, osobno dla każdej płatności, opisane w regulaminie |
| Minimum | Brak w MVP; Portivo stosuje minimum 15 zł za rezerwację |

Przerzucenie opłat na uczestnika można rozważyć po sprawdzeniu zasad prezentacji ceny całkowitej. **[PRAWNIK]**

## Model rozliczeń w kolejnych etapach

| Etap | Forma | Abonament | Prowizja | Warunek |
|---|---|---|---|---|
| W: walidacja | Bez rejestracji | Brak | Brak | Żadnych sprzedaży; można zacząć od razu |
| P: pilotaż zamknięty | Działalność nierejestrowana po potwierdzeniu | 0 zł dla partnerów, opcjonalnie niska opłata | Brak lub faktura ręczna; organizator ma własne konto u operatora | Konsultacja **[PRAWNIK] [KSIĘGOWY]** |
| K: prowizja przez operatora | JDG lub sp. z o.o. | Pełny cennik | Stripe Connect direct charges albo faktura miesięczna | Pozytywne potwierdzenie MCC i opinii |
| M: marketplace | JDG lub raczej sp. z o.o. | Pełny cennik | Jak wyżej; ewentualna wyższa stawka za pozyskany popyt | Test koszyka, DAC7, regulaminy |

## Działalność nierejestrowana w tym modelu

**Etap W nie wymaga rejestracji.** Sprzedaż w etapie P jest możliwa tylko po potwierdzeniu przez doradców.

- **Limit:** 10 813,50 zł przychodu należnego na kwartał w 2026, czyli średnio około 3 604 zł miesięcznie.
- **Abonament roczny szybko zużywa limit:** to tylko 7 przedpłat po 1 490 zł w jednym kwartale, jeśli liczy się cały należny przychód. **[KSIĘGOWY]**
- **Warunek 60 miesięcy:** nie można korzystać z tej formy, jeśli działalność była prowadzona w ostatnich 60 miesiącach. Do potwierdzenia.
- **Faktury:** od 1 stycznia 2027 faktury B2B przechodzą przez KSeF, co wymaga NIP.
- **Zakupy zagraniczne:** usługi chmurowe i API mogą wymagać VAT-UE przed pierwszym zakupem, a Twój zakup prywatny nie musi rozwiązywać problemu po stronie działalności. **[KSIĘGOWY]**
- **Podatek:** przychód trafia do PIT-36. [Ulga dla młodych obejmuje etat i zlecenie, a nie działalność gospodarczą](https://freenance.io/podatki/pit-0-dla-mlodych-2026-ulga-do-26-roku-zycia/); przychodu nierejestrowanego prawdopodobnie też nie obejmuje. **[KSIĘGOWY]**

## Kiedy założyć działalność

Rejestracja jest odłożona, ale ma jasne wyzwalacze. Załóż JDG, gdy zajdzie **pierwszy** z tych warunków:

1. Pierwszy klient chce płatności przez platformę lub konieczna jest prowizja przez operatora.
2. Przychód kwartalny przekracza około 70% limitu (około 7 500 zł; próg bezpieczeństwa to propozycja).
3. Zapada decyzja o publicznym marketplace.
4. Doradca uzna, że działalność nierejestrowana nie pasuje.

**Twój etat obniża ten koszt.** Jeśli pensja wynosi co najmniej minimalną krajową, składki społeczne z JDG odpadają, a zostaje głównie zdrowotna: w 2026 **432,54 zł** miesięcznie na skali lub **498,35 zł** na ryczałcie do 60 000 zł przychodu. Na skali to minimum, które rośnie z dochodem. Przy niepełnym etacie lub niższej pensji koszt jest inny. Warunki sprawdź z księgowym. **[KSIĘGOWY]**

## Ryzyko etatu i zakazu konkurencji

Zakaz konkurencji w WMS nie musi dotyczyć Nautivo, ale przed sprzedażą sprawdź w umowie:

- dokładny zakres zakazu (czy mówi tylko o WMS, czy o szerszej działalności pracodawcy);
- klauzulę o prawach do kodu, także tworzonego poza pracą;
- zakaz używania sprzętu, kont i wiedzy pracodawcy przy Nautivo.

Pracuj na własnym sprzęcie i poza godzinami pracy. **[PRAWNIK]**

## Odpowiedzialność: Nautivo a organizator

| Obszar | Organizator | Nautivo |
|---|---|---|
| Realizacja rejsu, kursu, bezpieczeństwo | Tak | Nie, jeśli nie występuje jako organizator |
| Status imprezy turystycznej, wpis, zabezpieczenie | Tak | Weryfikacja deklaracji przy onboardingu; ryzyko przy roli agenta **[PRAWNIK]** |
| Umowa z uczestnikiem, zwroty | Tak | Narzędzie i przejrzyste informacje |
| Dane uczestników w SaaS | Administrator | Podmiot przetwarzający, umowa z art. 28 RODO |
| Konta marketplace, opinie | Zależy | Administrator **[PRAWNIK]** |
| Regulamin platformy, DSA, DAC7 | Dostarcza dane | Odpowiada |
| Bezpieczeństwo systemu | Nie | Tak |

Nautivo unika marki „rejsy Nautivo”, koszyka wielu organizatorów i określenia „usługi płatnicze”.

## Główne założenia do obalenia

1. Organizatorzy mają problem na tyle duży, by płacić abonament.
2. Akceptują 3% razem z opłatą operatora.
3. Większość wpłat może przejść online.
4. Stripe akceptuje tę branżę i polskie stowarzyszenia.
5. Prawnik potwierdzi, że opłata aplikacyjna nie jest kontrolą nad środkami.

Założenia 1–3 sprawdzają wywiady, 4 pytanie do Stripe, a 5 konsultacja.
