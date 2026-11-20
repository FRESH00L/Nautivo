# 03. Analiza konkurencji

**Status:** etap A, wersja robocza do akceptacji założyciela. Brak wyniku wyszukiwania nie dowodzi braku produktu.

**Źródło:** wyciąg z raportu [`reports/Nautivo rynek płatności i prawo.md`](../reports/Nautivo%20rynek%20p%C5%82atno%C5%9Bci%20i%20prawo.md).

**Data weryfikacji wszystkich faktów: 2026-11-05.** To synteza notatek badawczych, a nie porada prawna ani podatkowa. Oznaczenie **[PRAWNIK]** lub **[KSIĘGOWY]** wskazuje punkt wymagający potwierdzenia przez specjalistę przed jakimkolwiek działaniem.

Raport rozdziela trzy poziomy. **Potwierdzone** oznacza treść odczytaną w cytowanym źródle, przy czym deklaracje dostawców pozostają ich deklaracjami. **Wniosek** oznacza rozumowanie lub obliczenia na tych danych, a nie prawo ani oficjalną wykładnię. **Rekomendacja** oznacza proponowaną decyzję. „Fragment” oznacza treść widzianą tylko w wynikach wyszukiwania, bez otwarcia strony.

## Konkurencja: czarter jest zajęty, a rejsy na koje, kursy i kluby wyglądają na otwarte

### Oprogramowanie dla operatorów

**Potwierdzone.** Ceny netto, bez opłat operatora płatności, zgodnie z tym, co pokazywały strony w dniu weryfikacji.

| Produkt | Segment | Cena | Polska | Możliwa szczelina (wniosek) |
|---|---|---|---|---|
| [Boater](https://boater.pl) | Czarter, wynajem krótkoterminowy | 299 PLN/mies. + 1,5% online albo 4 500 PLN jednorazowo, 0% | PL, P24, PayU, BLIK, KSeF. Podmiot prawny i liczba klientów nieustalone | Kursy, koje, kluby; weryfikowalna firma |
| [Portivo](https://operator.portivo.eu/pl/cennik) | Czarter | 149 PLN/mies. + 3% online, min. 15 PLN za rezerwację; aplikacja +249 PLN | PL, BLIK przez Stripe, spółka z USA z odwrotnym obciążeniem | Polska faktura, płatności poza Stripe |
| [yachtCMS](https://yachtcms.pl/cennik) | Czarter, widget | 0,20–0,40 PLN za punkt, 1 punkt na jacht/dzień, 0% prowizji | PL, P24, deklaruje [ponad 200 firm](https://yachtcms.pl/o-nas) | Szerszy zakres; ceną trudno wygrać |
| [Booking Manager](https://www.booking-manager.com) | Floty, dystrybucja agencyjna | Niepubliczna | Partner [Ahoj Czarter](https://www.booking-manager.com/en/golden-partner-details/p/partnerid-2674/Ahoj%20Czarter.html) | Integrować, nie konkurować |
| [NauSYS](https://www.nausys.com) | Floty, dystrybucja | Niepubliczna, „no fees on bookings” | Partner [NaRejsy.pl](https://www.nausys.com/?p=21719) | Integrować, nie konkurować |
| [Sedna Genesis](https://genesis.sednasystem.com/landing) | Floty, sprzedaż kabin | 30 EUR za łódź/mies.; 5% tylko od klientów pozyskanych przez Sednę | Brak wzmianki | Język, koje zamiast kabin |
| [Broad Reach](https://www.broadreachbookings.com/pricing) | Szkoły, rejsy | 1,75% od wszystkich rezerwacji, min. 49 GBP/mies. | Brak wzmianki | Język, BLIK/P24, KSeF, polskie patenty |
| [Viking](https://www.vikingbookings.com/plans) | Szkoły wodne | 3% lub 2,25% albo 4 900 EUR/rok; setup 380 EUR | Brak wersji PL | Jw. |
| [eola](https://eola.co/business/pricing) | Szkoły | 2% + karta 1,7% + 30p; marketplace 18% | Logotypy z UK | Jw. |
| [Anolla](https://anolla.com/pl/oprogramowanie-do-wynajmu-lodzi) | Horyzontalny, łodzie | Plan 0 EUR; opłaty niejasne | Strona PL, Stripe; BLIK i P24 bez wyraźnej wzmianki | Lokalne płatności |
| [Bookero](https://bookero.pl/cennik) | Rezerwacje ogólne | 19,90–89,90 PLN/mies. + VAT | Polski | Logika rejsów i kabin (nietestowane) |
| [Clubspot](https://www.theclubspot.com/pricing), [SailingClubManager](https://www.sailingclubmanager.com) | Kluby | Stawki nieodczytane lub niepubliczne | Brak wzmianki | Polskie prawo stowarzyszeń, PLN |

Horyzontalne systemy rezerwacji są punktem odniesienia dla 3%: Rezdy 49–249 USD + 3% ([Rezdy](https://www.rezdy.com/pricing/)), Bókun +1–1,5% ([Bókun](https://www.bokun.io/pricing)), Checkfront 99 EUR + 3% ([Checkfront](https://www.checkfront.com/pricing/)), TuriTop 29–299 EUR + 0,75–2% ([TuriTop](https://www.turitop.com/en/pricing/)). Oficjalny cennik FareHarbor się nie załadował, a „do 6%” pochodzi od konkurentów ([Checkfront](https://www.checkfront.com/blog/checkfront-vs-fareharbor-an-honest-2026-comparison/)).

### Marketplace'y

| Platforma | Właściciel płaci | Klient płaci | Oferta polska (liczby nieporównywalne, nie sumować) |
|---|---|---|---|
| [NaCzarter](https://naczarter.pl/) | Prowizja [nieujawniona](https://naczarter.pl/dla-armatorow), ogłoszenie bezpłatne | Nie podano | 270+ jednostek, 30+ armatorów, tylko Mazury (deklaracja) |
| [Click&Boat](https://www.clickandboat.com/pl/czarter/kraj-polska) | Do 26% (fragment, [pomoc](https://help.clickandboat.com/hc/en-gb/articles/4416266551570-Net-earning-after-the-commission-has-been-deducted-and-commission-invoice)) | Nieujawniona opłata | Około 244 w FAQ, „+100” w nagłówku tej samej strony |
| [SamBoat](https://www.samboat.com/help/how-much-will-i-receive-per-rental-93) | 20% (inny fragment: 18%) | Nieujawniona | [93 łodzie](https://www.samboat.com/boat-rental/poland) |
| [Getmyboat](https://www.getmyboat.com/for-owners/) | 11,5–14,5%; Charge 1,5–3% | [13–16%](https://getmyboat.com/how-it-works/) | [50 łodzi](https://www.getmyboat.com/boat-rental/Poland/), częściowo Praga |
| [Sailo](https://myc.sailo.com/company/faq/topic/Policies-And-Procedures/Fees-And-Commission/Service-Fees/) | 14% lub 29% | 7% | [15 w Gdańsku](https://www.sailo.com/boat-rentals/Poland/Gdansk) (fragment) |
| [Boatsetter](https://www.boatsetter.com/fees) | 11,5–35% w ujęciu pośrednim | Opłata bez podanego % | Nie znaleziono |
| [zegluj.pl](https://zegluj.pl/) | Nie podano | Nie podano | „1700” w nagłówku, „ponad trzysta” w treści |
| [Yachtic](https://yachtic.com/about) | Nieujawniona | Bez opłaty rezerwacyjnej | Agregator światowy, 30 000+ jachtów, Katowice |

Getmyboat i Boatsetter ogłosiły fuzję 18 grudnia 2025 ([Boatsetter](https://www.boatsetter.com/boating-resources/boatsetter-and-getmyboat-announce-merger-forming-a-powerhouse-marketplace-for-boat-rentals-on-the-water-adventure)). Boataround nie został zweryfikowany (403).

**Nazwy z briefu, których nie potwierdzono.** „SailUp” jako system dla żeglarstwa nie istnieje w sprawdzonych wynikach: pod tą nazwą są aplikacja treningowa, polska firma IT i ghańskie API ([sailup.pl](https://sailup.pl), [sailup.io](https://www.sailup.io/)). „SEARM” nie znaleziono ([searm.com](https://www.searm.com) przekierowuje na stronę sprzedaży domeny). „tubajacht.pl” i „jachtclick.pl” nie rozwiązywały się w DNS, a [turbotic.com](https://turbotic.com/) to firma od automatyzacji AI; to, że chodziło o TuriTop, jest domysłem. Autor briefu powinien podać adresy.

### Prawdziwym konkurentem wielu małych organizatorów są formularze

**Potwierdzone, jeden przypadek.** Klub Żeglarski UW zbiera zapisy przez Formularze Google, a koszt i terminy podaje w wydarzeniu na Facebooku ([Regulamin KZUW 2026](https://klubzeglarski.uw.edu.pl/wp-content/uploads/2026/02/Regulamin-zapisow-na-rejs-Gaflowy-Baltyk-z-KZUW-rejs-zaglowcem-STS-%E2%80%9EGeneral-Zaruski.pdf)). Nie ma badania narzędzi polskich organizatorów, a arkuszy kalkulacyjnych nie udało się w ogóle potwierdzić źródłem.

**Wniosek.** Pozycja „pierwszy polski system czarterowy” jest niedostępna. Nie potwierdzono żadnego produktu sprzedającego pojedyncze koje w kabinie. Sedna sprzedaje kabiny ([Sedna](https://genesis.sednasystem.com/landing)), Charterfy zapowiada to jako „coming soon” ([Charterfy](https://charterfy.com)), a Broad Reach ma pojemność na osobę. Dla rejsów na koje, kursów i klubów siła dowodu to odpowiednio słaba do umiarkowanej, słaba do umiarkowanej i słaba. Metoda wyszukiwania była ograniczona: Boater i yachtCMS znaleziono dopiero przez testowanie domen z briefu, a narzędzie jest amerykańskie, więc mogły umknąć inne małe polskie produkty. Luka może też oznaczać niską gotowość do płacenia, a Boater i Portivo mają już płatności, dokumenty i kalendarz, więc mogą ją szybko zamknąć. Płatności BLIK i podział zaliczka/saldo są standardem rynku, a nie wyróżnikiem.

**Rekomendacja.** Nie twierdzić, że konkurencja jest niska, przed wykonaniem 10–20 wywiadów z organizatorami rejsów, szkołami i klubami, ręcznym wyszukiwaniem z Polski, sprawdzeniem stopek „powered by” i demo Boatera oraz Portivo. Pierwszy zakres to jeden rdzeń: rejs lub kurs z limitem miejsc, listą rezerwową, zaliczką i saldem. Początek w jednym niszowym segmencie ma polski precedens w NaCzarter, choć jego czas i koszt są nieznane. SaaS-first pozwala organizatorowi korzystać z Nautivo dla własnych rezerwacji, zanim marketplace będzie miał popyt, co rozwiązuje zimny start przy 7 godzinach tygodniowo i około 364 godzinach rocznie.

### 3% to dużo mniej niż marketplace, ale nie jest tanie wobec SaaS

**Wniosek.** Dla organizatora 3% + abonament jest dużo tańsze niż prowizje marketplace'ów (tabela marketplace'ów wyżej, 11,5–29%, a do 35% w ujęciu pośrednim), lecz tylko równe Portivo i wyższe niż Boater (1,5%) oraz yachtCMS (0%). Przy 5 jachtach i 200 000 PLN płatności online rocznie Boater kosztuje około 6 588 PLN, Portivo około 7 788 PLN, a yachtCMS około 548 PLN, bez opłat operatora. Nautivo z abonamentem 149 PLN i 3% kosztowałoby tyle samo co Portivo. Jest też odwrotna luka: **jednolite 3% nie zarabia na popycie, który przyniesie marketplace**, a Getmyboat rozdziela stawki na 14,5% dla rezerwacji z platformy i 3% dla bezpośrednich poza USA ([Getmyboat](https://www.getmyboat.com/for-owners/)).

**Rekomendacja do testu (odstępstwo od briefu oznaczone wprost).** Zachować abonament + 3% dla własnych rezerwacji organizatora, testując trzy rzeczy: 0% na płatnościach offline, które są normą u Bókun, Checkfront, TuriTop i Portivo, choć Broad Reach pobiera 1,75% także od gotówki ([Broad Reach](https://www.broadreachbookings.com/pricing)); przełącznik przenoszenia opłaty na klienta, po sprawdzeniu prezentacji ceny całkowitej z art. 12 ust. 1 pkt 5 **[PRAWNIK]**; oraz wyższą stawkę tylko za popyt pozyskany przez marketplace, co wykracza poza brief i wymaga decyzji założyciela. Brief mówi o „każdej faktycznej płatności”, więc trzeba też zdefiniować, czy obejmuje ona płatności offline i czy zwrócona płatność jest „faktyczna”.

## Konflikty źródeł dotyczące tego dokumentu

| Temat | Konflikt | Stan |
|---|---|---|
| SamBoat | 20% ([pomoc](https://www.samboat.com/help/how-much-will-i-receive-per-rental-93)) i 18% ([Trustpilot, fragment](https://www.trustpilot.com/review/samboat.fr?page=3)) | Przewaga 20%, nierozstrzygnięty |
| Click&Boat | 26% (fragment) i starsze 15%; [recenzja afiliacyjna](https://yachtsforkings.pages.dev/brokers/click-and-boat-review/) 7–12%, zdyskontowana | Sprawdzić na koncie właściciela |
| Regiondo | [„No commission”](https://go.regiondo.com/pricing) i opłata za rezerwację; [TrekkSoft](https://www.trekksoft.com/en/blog/regiondo-pricing-guide-what-to-know-before-you-buy) podaje 0,49 EUR za bilet | Źródło konkurencyjne |
| Viking i eola | [Viking EN](https://www.vikingbookings.com/plans) i [NL](https://vikingbookings.com/nl/plans); [eola](https://eola.co/business/pricing) i [druga strona](https://business.eola.co/page/pricing) | Strony ze sobą sprzeczne |

## Granice materiału

**EUR-Lex nie był dostępny dla narzędzi badawczych**, więc DSA odczytano z nieoficjalnego lustra, PSD2, P2B, RODO i dyrektywę 2015/2302 z tekstów „adopted” na legislation.gov.uk, a dyrektywę 2026/1024 tylko ze źródeł wtórnych. Polskie ustawy płatnicze, turystyczne, konsumenckie, e-usługowe i o wymianie informacji odczytano z PDF-ów ISAP według dat kompilacji; późniejsze zmiany nie byłyby widoczne. Tekst VAT pochodzi z Dz.U. 2025 poz. 775 i nie zawiera zmian z 2026, więc bieżące kwoty pochodzą z MF. Niedostępne były FAQ MSiT, ustawa o nieuczciwych praktykach w ISAP, pełny PDF wytycznych EDPB 07/2020, art. 5 Prawa przedsiębiorców, tabele ZUS, autopay.pl, developers.przelewy24.pl, pełny cennik PayU, umowa marketplace PayU, załącznik kartowy Tpay i druga połowa umowy Stripe. Tekst dokumentu Rady jest kompromisem, a nie ogłoszonym aktem. Odpowiedzi EBA dotyczą przepisów z dnia publikacji i nie są systematycznie przeglądane.

Nie zbadano też: czasu i kosztu budowy, stosu i kosztów infrastruktury Nautivo, rzeczywistych cen ubezpieczeń i księgowości, liczby klientów konkurentów, akceptacji Stripe dla polskich stowarzyszeń, ani tego, czym dokładnie są trzy stowarzyszenia dostępne założycielowi. Przy tych punktach raport nie podaje liczb.
