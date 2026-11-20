# 15. Integracja płatności

**Status:** etap A, wersja robocza do akceptacji założyciela. Wariant główny jest warunkowy; nie budować przed spełnieniem warunków.

**Źródło:** wyciąg z raportu [`reports/Nautivo rynek płatności i prawo.md`](../reports/Nautivo%20rynek%20p%C5%82atno%C5%9Bci%20i%20prawo.md).

**Data weryfikacji wszystkich faktów: 2026-11-05.** To synteza notatek badawczych, a nie porada prawna ani podatkowa. Oznaczenie **[PRAWNIK]** lub **[KSIĘGOWY]** wskazuje punkt wymagający potwierdzenia przez specjalistę przed jakimkolwiek działaniem.

Raport rozdziela trzy poziomy. **Potwierdzone** oznacza treść odczytaną w cytowanym źródle, przy czym deklaracje dostawców pozostają ich deklaracjami. **Wniosek** oznacza rozumowanie lub obliczenia na tych danych, a nie prawo ani oficjalną wykładnię. **Rekomendacja** oznacza proponowaną decyzję. „Fragment” oznacza treść widzianą tylko w wynikach wyszukiwania, bez otwarcia strony.

## Model płatności: direct charges z opłatą 3%, fallback w postaci własnych kont organizatorów

**Rekomendacja i uzasadnienie w tabeli. Wszystkie oceny ryzyka to wnioski, a to, czy dany wariant zostawia Nautivo poza usługami płatniczymi, wymaga opinii prawnika.** **[PRAWNIK]**

| Wariant | Sprzedawca i klient operatora | Pobieranie 3% | Ryzyko zwrotów i chargebacków | Ocena |
|---|---|---|---|---|
| Rachunek Nautivo → przekaz do organizatora | Nautivo | Potrącenie | Nautivo | **Odrzucić**: art. 3–4, 150 |
| MIP | Nautivo | Potrącenie | Nautivo | **Odrzucić**: regulacja, AML, tylko Polska |
| Stripe destination lub separate charges | Platforma | `application_fee_amount` lub `transfer_data[amount]` | Saldo platformy; Stripe opisuje upadłości hoteli w czasie COVID, których chargebacki pokryły platformy | **Odrzucić na start** |
| **Stripe direct charges, straty po stronie Stripe** | Organizator | Automatyczna opłata aplikacyjna | Saldo organizatora | **Wariant główny, warunkowy** |
| **Własne konto organizatora + faktura miesięczna** | Organizator | Faktura z webhooków lub raportów | Organizator, a ryzyko kredytowe prowizji po stronie Nautivo | **Fallback i wariant bez podmiotu** |
| Marketplace PayU, Tpay, P24, Paynow, Autopay | Organizator jako submerchant | Mechanizm operatora | Na saldzie sprzedawcy; odpowiedzialność marketplace'u niepubliczna | **Później**, po wycenach i wolumenie |
| Adyen, Mangopay, Lemonway | Zależy | Zależy | Kontrakt | **Pominąć**: tryb sprzedażowy, a dla Lemonway brak dowodu BLIK i P24 |

**Potwierdzone: fundament wariantu głównego.** Przy direct charges „Refunds and chargebacks reduce the connected account's balance”, a przy destination i separate charges obciążają saldo platformy ([Stripe, typy obciążeń](https://docs.stripe.com/connect/charges)). Gdy Stripe odpowiada za ujemne salda kont, pokrywa straty i prowadzi KYC, platforma nie ma rezerwy, ale nie może wstrzymywać wypłat ani obciążać kont ([Stripe, ryzyko](https://docs.stripe.com/connect/risk-management)). W wariancie „Stripe handles pricing” nie ma opłat Connect dla platformy ([Stripe Connect, cennik](https://stripe.com/pl/connect/pricing)). Polskie konta mają typy `individual`, `company` i `non_profit`, a dla Polski dostępna jest tylko pełna umowa usługowa ([Stripe, wymagania PL](https://docs.stripe.com/_endpoint/get-requirement-selections-for-platform-country?platformCountry=PL)). Jest oficjalne SDK dla .NET ([stripe-dotnet](https://github.com/stripe/stripe-dotnet)), a wśród polskich operatorów takiego nie znaleziono, co jest brakiem dowodu z ograniczonych wyszukiwań, a nie deklaracją operatorów. NaCzarter i Portivo pokazują, że Stripe bywa używany w polskim czarterze, choć ich struktura obciążeń nie jest znana.

**Warunki, które muszą być spełnione przed budową.** Po pierwsze, MCC. **P24 w Stripe zakazuje MCC 4457 Boat Rentals, 4722 Travel Agencies/Tour Operators, 4411 Cruise Lines i 5262 Marketplaces**, a także kategorii „Entertainment and event promotion” i kształcenia zawodowego, a ograniczenia dotyczą m.in. 7941 Sports Clubs i 8299 Educational Services ([Stripe P24](https://docs.stripe.com/payments/p24)). Ogólna lista oznacza „Travel reservation services and clubs” jako ograniczone, a zbiórki charytatywne w Polsce nie są obsługiwane ([Stripe, restricted businesses](https://stripe.com/legal/restricted-businesses)). Plan bazowy to BLIK + karty, bez P24, i wpłaty opisane jako usługi, nie darowizny. Po drugie, rzeczywista akceptacja polskich stowarzyszeń, zwłaszcza bez KRS. Po trzecie, opinia prawnika, że opłata aplikacyjna nie jest kontrolą nad środkami, oraz rozstrzygnięcie konfliktu z art. 75o DAC7. Jeśli Stripe odrzuci któryś warunek, przechodzi się na fallback.

**Fallback.** Własne konta działają u każdego polskiego operatora, ale nie znaleziono warunku, który dopuszcza lub zakazuje ustawienia prowadzonego przez platformę z prowizją fakturowaną później. Najlepszy publiczny dowód dla stowarzyszeń ma P24, którego lista dokumentów obejmuje wprost „organizations, clubs and associations without KRS” ([P24, dokumenty](https://www.przelewy24.pl/centrum-pomocy/sprzedaje-z-przelewy24/rejestracja-w-przelewy24/zarejestrowalemam-sie-w-p24-jakie-dokumenty-mam-wyslac)), oraz Tpay, którego definicja obejmuje podmioty bez działalności gospodarczej ([Tpay OWU](https://tpay.com/tpay-owu-2)). Czy P24 we własnym marketplace lub merchancie ma podobne wyłączenia MCC jak w Stripe, nie wiadomo. Wadą jest ponowny KYC i przegląd strony u każdego organizatora, ryzyko niezapłaconych faktur, przechowywanie kluczy API organizatorów i to, że klucze odpowiadają za działania akceptanta ([Tpay OWU](https://tpay.com/tpay-owu-2)). Przy 7 godzinach tygodniowo należy wybrać jednego operatora.

**Marketplace'y polskie, stan publiczny.** PayU: `shoppingCarts[]` z polem `fee`, płatności przed weryfikacją do 2 500 PLN, formularz do 20 sprzedawców, a partial capture nie działa w marketplace ([integracja](https://developers.payu.com/europe/docs/services/marketplace/integration/), [boarding](https://developers.payu.com/europe/docs/services/marketplace/boarding/), [marketplace](https://poland.payu.com/our-solutions/marketplace/), [capture](https://developers.payu.com/europe/docs/payment-flows/capture-and-cancel/)). Tpay: program partnerski i umowa, a lista metod nie wymienia kart ([Tpay marketplace](https://tpay.com/dla-biznesu/marketplace)). P24: prowizja „customised” ([P24 marketplace](https://www.przelewy24.pl/en/payment-solutions/marketplace)). Paynow: `feeAmount`, a rozmowy Enterprise od 100 000 PLN miesięcznie ([Paynow marketplace](https://docs.paynow.pl/docs/v3/marketplace), [cennik](https://www.paynow.pl/cennik)). Autopay: prowizje tylko procentowe, co pasuje do 3% ([Autopay marketplace](https://developers.autopay.pl/online/marketplace)). Żadne ceny marketplace nie są publiczne.

## Koszty operatorów a prowizja 3%

**Potwierdzone (ceny katalogowe, ceny marketplace są niepubliczne poza Stripe).** Żadne źródło nie mówi, czy ceny zawierają VAT. Obliczenia „zostaje z 3%” to wniosek dla jednej transakcji bez przewalutowania.

| Stawka (źródło) | Opłata przy 100 / 800 / 3000 PLN | Zostaje z 3%, gdy platforma pokrywa opłatę | Organizator dostaje, gdy sam ją pokrywa |
|---|---|---|---|
| [Stripe](https://stripe.com/pl/pricing) BLIK 1,6% + 1,00 | 2,60 / 13,80 / 49,00 | 0,40 / 10,20 / 41,00 | 94,40 / 762,20 / 2861,00 |
| Stripe karta EOG standard 1,5% + 1,00 | 2,50 / 13,00 / 46,00 | 0,50 / 11,00 / 44,00 | 94,50 / 763,00 / 2864,00 |
| Stripe karta premium lub P24 1,9% + 1,00 | 2,90 / 16,20 / 58,00 | 0,10 / 7,80 / 32,00 | 94,10 / 759,80 / 2852,00 |
| [Przelewy24](https://www.przelewy24.pl/en/offer/commissions-and-fees) 1,29% + 0,30 | 1,59 / 10,62 / 39,00 | 1,41 / 13,38 / 51,00 | 95,41 / 765,38 / 2871,00 |
| [Tpay](https://tpay.com/oferta) Starter BLIK 1,59% + 0,41 | 2,00 / 13,13 / 48,11 | 1,00 / 10,87 / 41,89 | 95,00 / 762,87 / 2861,89 |
| Tpay Business BLIK 0,99% + 0,02 (99 PLN/mies.) | 1,01 / 7,94 / 29,72 | 1,99 / 16,06 / 60,28 | 95,99 / 768,06 / 2880,28 |
| [PayU](https://poland.payu.com/oferta-handlowa/) BLIK 1,49% + 0,32 | 1,81 / 12,24 / 45,02 | 1,19 / 11,76 / 44,98 | 95,19 / 763,76 / 2864,98 |
| [Paynow](https://www.paynow.pl/cennik) od 0,95% | 0,95 / 7,60 / 28,50 | 2,05 / 16,40 / 61,50 | 96,05 / 768,40 / 2881,50 |
| [Autopay](https://autopay.pl/oferta/platnosci-online) 1,25% + 0,34 (fragment) | 1,59 / 10,34 / 37,84 | 1,41 / 13,66 / 52,16 | 95,41 / 765,66 / 2872,16 |

Tabeli nie wolno używać do rankingu operatorów przed uzyskaniem wycen marketplace. Punkt zerowy przy pokrywaniu opłaty przez platformę to 71,43 PLN dla Stripe BLIK, 90,91 PLN dla Stripe premium i P24 przez Stripe, 17,54 PLN dla P24 i 29,08 PLN dla Tpay Starter BLIK. Przy Stripe destination charges w trybie „you handle pricing” dochodzi 0,25% + 1,35 PLN za wypłatę i 9 PLN za aktywne konto miesięcznie ([Stripe Connect, cennik](https://stripe.com/pl/connect/pricing)), więc płatność BLIK na 100 PLN daje **stratę 1,20 PLN**, a na 800 PLN zostaje 6,85 PLN (najgorszy wariant: jedna wypłata na płatność). Podział 3 000 PLN na 800 + 2 200 kosztuje 50,00 PLN zamiast 49,00 PLN, a zwrot BLIK na 800 PLN nie oddaje opłaty 13,80 PLN.

**Rekomendacja.** Opłatę operatora ponosi organizator, a 3% jest przychodem Nautivo. Tak robi Portivo: 3% plus opłaty Stripe naliczane dodatkowo ([Portivo, cennik](https://operator.portivo.eu/pl/cennik)).

### Reguły do 15-PAYMENT-INTEGRATION

| Obszar | Fakt potwierdzony | Decyzja projektowa (rekomendacja) |
|---|---|---|
| Prowizja | `application_fee_amount` ([Stripe](https://docs.stripe.com/connect/destination-charges.md?platform=web&ui=stripe-hosted); dokument dla destination, sprawdzić dla direct) | 3% osobno na każdym obciążeniu, zaokrąglenie opisane w regulaminie |
| Zaliczka i saldo | Żaden operator nie oferuje obiektu raty na poziomie rezerwacji; każda płatność to osobna transakcja | Własny model rezerwacji z harmonogramem; ostrzeżenie, że każda rata dodaje stałą opłatę |
| Zwrot | Domyślnie opłata aplikacyjna zostaje u platformy, chyba że `refund_application_fee=true`; opłata Stripe nie wraca ([cennik](https://stripe.com/pl/pricing)) | Proponowana wykładnia „faktycznej płatności”: zwrot płatności oznacza proporcjonalny zwrot 3%, a opłatę operatora ponosi organizator. Decyzja założyciela, zapis w umowie |
| Okna zwrotów | Stripe BLIK 730 dni, P24 180 dni ([BLIK](https://docs.stripe.com/payments/blik), [P24](https://docs.stripe.com/payments/p24)); Paynow 180; Autopay 6/12 mies. (strona może być stara) ([Autopay](https://developers.autopay.pl/online/faq/rozliczenia)) | Blokada zwrotu w UI po terminie, alternatywa przelewem |
| Spory | Karta: 90 PLN za spór ([Stripe](https://stripe.com/pl/pricing)); BLIK: 12 dni na dowody, bez opłaty, bez odwołania ([BLIK](https://docs.stripe.com/payments/blik)); P24 bez chargebacków ([P24](https://docs.stripe.com/payments/p24)) | Obowiązek dowodów po stronie organizatora, powiadomienie w panelu |
| Kaucja | BLIK i P24 bez manual capture; karta 7 dni, rozszerzona do 30 dni na IC+ ([holds](https://docs.stripe.com/payments/place-a-hold-on-a-payment-method), [extended](https://docs.stripe.com/payments/extended-authorization.md?platform=web&ui=stripe-hosted)) | Kaucji nie obsługiwać w MVP; organizator rozlicza ją sam, jak przy odbiorze w [NaCzarter](https://naczarter.pl/regulamin). Rynkowo 1 500–3 000 PLN ([Poljacht](https://poljacht.pl/cennik/)). Status środków: **[PRAWNIK]** |
| Wstrzymane wypłaty | Stripe nie oferuje escrow, ręczne wypłaty do 90 dni, odradza arbitralne trzymanie środków ([manual payouts](https://docs.stripe.com/connect/manual-payouts), [balances](https://docs.stripe.com/connect/account-balances)) | Brak funkcji wstrzymania w MVP, chyba że wymaga jej DAC7 po opinii |
| Webhooki | `Stripe-Signature` HMAC-SHA256, ponowienia do 3 dni, duplikaty możliwe, bez gwarancji kolejności ([webhooki](https://docs.stripe.com/webhooks)) | Tabela zdarzeń z unikalnym ID, obsługa idempotentna, stany niezależne od kolejności |
| Idempotencja | `Idempotency-Key` do 255 znaków, klucze mogą wygasać po 24 h ([API](https://docs.stripe.com/api/idempotent_requests)) | Klucz wyprowadzony z ID płatności i próby |
| Rozliczenia | Obiekty Application Fee, raporty, zdarzenia `payout.*` ([payouts](https://docs.stripe.com/connect/manage-connected-account-payouts)) | Dzienny import do księgi prowizji i danych DAC7 |
| BLIK | Minimum 2,00 PLN, off-session do 2 000 PLN ([BLIK](https://docs.stripe.com/payments/blik)) | Walidacja kwot; abonament Nautivo osobnym kanałem |
| Dane sprzedawcy | P24 wymaga na stronie adresu, NIP, numeru rejestrowego i polityki zwrotów ([Stripe P24](https://docs.stripe.com/payments/p24)); Autopay m.in. regulaminu i przykładowych usług ([Autopay](https://developers.autopay.pl/online/faq/o-usludze/ktore-dzialalnosnosci-rejestrowane-moga-korzystac-z-platnosci-online)) | Profil organizatora z tymi polami i danymi do DAC7; czy profil w Nautivo zalicza się jako strona, ustalić z operatorem |

**Pytania do operatorów (wysłać przed kodem).** Czy platforma może integrować sprzedawców z ich poświadczeniami? Czy profil w Nautivo spełnia wymóg strony? Jakie MCC dla rejsów, kursów i wynajmu? Jaki cennik marketplace, minimalny wolumen i podział odpowiedzialności? Czy przyjmują stowarzyszenia bez KRS i działalności gospodarczej? Czy działalność nierejestrowana może być platformą? Czy dla kart u Tpay obowiązują warunki indywidualne?

## Konflikty źródeł dotyczące tego dokumentu

| Temat | Konflikt | Stan |
|---|---|---|
| Karty Tpay | [Oferta](https://tpay.com/oferta): stawki; [PDF 2026](https://tpay.com/user/assets/files_for_download/pakiety-serwisowe-2026.pdf): warunki indywidualne; [inny PDF](https://tpay.com/user/assets/files_for_download/tpay-oferta.pdf): BLIK 1,78% + 0,39 (fragment) | Potwierdzić z Tpay |
| PayU | Oferta w [T&C](https://poland.payu.com/wp-content/uploads/sites/14/2026/05/Terms-and-conditions.pdf) obowiązywała do 31.05.2026, a [strona](https://poland.payu.com/oferta-handlowa/) nadal pokazuje 1,1% i 1,49% | Stawka promocyjna mogła wygasnąć |
| Autopay | 1,25% + 0,34 (fragment) i 1,19% + 0,34 na innych stronach | Potwierdzić |

## Granice materiału

**EUR-Lex nie był dostępny dla narzędzi badawczych**, więc DSA odczytano z nieoficjalnego lustra, PSD2, P2B, RODO i dyrektywę 2015/2302 z tekstów „adopted” na legislation.gov.uk, a dyrektywę 2026/1024 tylko ze źródeł wtórnych. Polskie ustawy płatnicze, turystyczne, konsumenckie, e-usługowe i o wymianie informacji odczytano z PDF-ów ISAP według dat kompilacji; późniejsze zmiany nie byłyby widoczne. Tekst VAT pochodzi z Dz.U. 2025 poz. 775 i nie zawiera zmian z 2026, więc bieżące kwoty pochodzą z MF. Niedostępne były FAQ MSiT, ustawa o nieuczciwych praktykach w ISAP, pełny PDF wytycznych EDPB 07/2020, art. 5 Prawa przedsiębiorców, tabele ZUS, autopay.pl, developers.przelewy24.pl, pełny cennik PayU, umowa marketplace PayU, załącznik kartowy Tpay i druga połowa umowy Stripe. Tekst dokumentu Rady jest kompromisem, a nie ogłoszonym aktem. Odpowiedzi EBA dotyczą przepisów z dnia publikacji i nie są systematycznie przeglądane.

Nie zbadano też: czasu i kosztu budowy, stosu i kosztów infrastruktury Nautivo, rzeczywistych cen ubezpieczeń i księgowości, liczby klientów konkurentów, akceptacji Stripe dla polskich stowarzyszeń, ani tego, czym dokładnie są trzy stowarzyszenia dostępne założycielowi. Przy tych punktach raport nie podaje liczb.
