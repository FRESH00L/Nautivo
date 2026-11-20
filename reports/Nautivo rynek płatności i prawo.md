# Prowizja 3% działa tylko bez dotykania pieniędzy

Model abonament + 3% da się obronić tylko w jednym układzie: organizator jest sprzedawcą i klientem licencjonowanego operatora, pieniądze uczestnika nie przechodzą przez rachunek Nautivo, opłatę operatora ponosi organizator, a koszty stałe firmy pokrywa abonament, nie prowizja. Przepływ „uczestnik → rachunek Nautivo → organizator minus 3%” najbardziej przypomina usługę płatniczą, a świadczenie jej bez zezwolenia grozi grzywną do 5 mln PLN i karą do 2 lat pozbawienia wolności; wyłączenie agenta handlowego jest wąskie i pod PSR jeszcze się zawęża. Ekonomicznie 3% nie utrzyma pilotażu: nawet gdyby cały przychód trzech stowarzyszeń na poziomie mediany NGO (50 000 PLN rocznie) przeszedł przez platformę, dałby 4 500 PLN rocznie, czyli mniej niż same minimalne składki JDG. Działalność nierejestrowana wystarcza na walidację i co najwyżej zamknięty pilotaż SaaS po potwierdzeniu przez doradcę, ale nie na publiczny marketplace z prowizją: nie znaleziono operatora, który oferuje marketplace takiemu podmiotowi, a działalność regulowana jest z niej wyłączona. Czarter w Polsce jest już zajęty (Boater, Portivo, yachtCMS, Booking Manager, NauSYS, NaCzarter), więc wejście powinno iść przez kursy, rejsy na koje i kluby, przy czym ta luka wynika głównie z braku wyników wyszukiwania, a nie z dowodu popytu. Rekomendowany model płatności to Stripe Connect z direct charges i opłatą aplikacyjną 3%, pod warunkiem trzech potwierdzeń (MCC, onboarding stowarzyszeń, opinia prawnika o kontroli nad środkami), a jako fallback własne konta organizatorów z prowizją fakturowaną co miesiąc. Nie wiadomo, czy budżet 5 000 PLN jest jednorazowy, czy miesięczny, i ta odpowiedź zmienia wybór formy prawnej, dlatego raport pokazuje oba warianty.

**Data weryfikacji wszystkich faktów: 2026-11-05.** To synteza notatek badawczych, a nie porada prawna ani podatkowa. Oznaczenie **[PRAWNIK]** lub **[KSIĘGOWY]** wskazuje punkt wymagający potwierdzenia przez specjalistę przed jakimkolwiek działaniem.

Raport rozdziela trzy poziomy. **Potwierdzone** oznacza treść odczytaną w cytowanym źródle, przy czym deklaracje dostawców pozostają ich deklaracjami. **Wniosek** oznacza rozumowanie lub obliczenia na tych danych, a nie prawo ani oficjalną wykładnię. **Rekomendacja** oznacza proponowaną decyzję. „Fragment” oznacza treść widzianą tylko w wynikach wyszukiwania, bez otwarcia strony.

| Dokument docelowy | Sekcje tego raportu |
|---|---|
| 02-MARKET-RESEARCH | „Rynek…”, część o niewiadomych |
| 03-COMPETITOR-ANALYSIS | „Konkurencja…”, tabele cen w części ekonomicznej |
| 15-PAYMENT-INTEGRATION | „Model płatności…”, tabela kosztów w części ekonomicznej |
| 21-LEGAL-CHECKLIST | „Prawo…”, „Bez rejestracji…”, „Lista kontrolna…” |

## Prawo: największe ryzyka wynikają z projektu przepływu pieniędzy i koszyka, nie z samej prowizji

Sama prowizja 3% nie jest problemem. Ryzyko powstaje, gdy Nautivo przyjmuje cudze pieniądze, łączy usługi różnych dostawców lub wygląda dla klienta jak sprzedawca. Trzy z tych ryzyk mają wymiar karny i osobisty.

### Przyjmowanie pieniędzy uczestników to najpewniej usługa płatnicza

**Potwierdzone.** Usługi płatnicze obejmują wykonywanie transakcji, acquiring i przekaz pieniężny, a ten ostatni polega na przyjęciu środków od płatnika i przekazaniu ich odbiorcy bez rachunku płatnika. Świadczyć je mogą tylko dostawcy usług płatniczych. **Art. 150 przewiduje grzywnę do 5 000 000 PLN, do 2 lat pozbawienia wolności albo obie kary**, także dla osoby działającej w imieniu lub interesie innego podmiotu; karalne jest też używanie określenia „usługi płatnicze” w nazwie lub reklamie bez uprawnień ([Ustawa o usługach płatniczych, t.j., stan z 2026-10-08](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20111991175/U/D20111175Lj.pdf)). Mała instytucja płatnicza wymaga wpisu, organizacji AML, planu finansowego i procedury ryzyka, działa wyłącznie w Polsce i ma limit średnio 1 500 000 EUR miesięcznie (ta sama ustawa, art. 117f–117h). Art. 6 pkt 2 wyłącza agenta działającego w imieniu lub na rzecz „wyłącznie płatnika albo wyłącznie odbiorcy”. EBA uznaje, że platforma B2C „could be excluded”, jeśli nie działa także dla płatnika, ale organ ocenia to po modelu biznesowym, a PSD2 nie daje kryteriów ([EBA Q&A 2020_5355](https://eba.europa.eu/single-rule-book-qa/qna/view/publicId/2020_5355)). Platforma przyjmująca płatność za odbiorcę „is not to be excluded by default” ([EBA Q&A 2020_5354](https://www.eba.europa.eu/single-rule-book-qa/qna/view/publicId/2020_5354)).

Tekst kompromisowy PSR z 17 kwietnia 2026 (oznaczony LIMITE) wymaga umocowania „only the payer or only the payee, but not both of them” oraz realnego zakresu negocjacji; motyw 11 stosuje to niezależnie od posiadania środków klienta i wyłącza platformy działające dla obu stron bez autonomii negocjacyjnej. PSR ma być stosowane 21 miesięcy po wejściu w życie ([Dokument Rady 8221/26](https://data.consilium.europa.eu/doc/document/ST-8221-2026-INIT/en/pdf)). Publikacji PSR/PSD3 w Dzienniku Urzędowym nie potwierdzono.

**Wniosek.** Przepływ przez rachunek Nautivo naraża i podmiot, i działające za niego osoby. Argument jednostronnego agenta jest słaby, gdy Nautivo ma też regulamin i obowiązki wobec uczestników, a standardowe warunki bez roli negocjacyjnej trudno pogodzić z PSR. MIP nie pasuje do jednoosobowego startu z 7 godzinami tygodniowo. Fakt, że Click&Boat trzyma środki w escrow ([Click&Boat, regulamin](https://www.clickandboat.com/en/terms)), a NaCzarter pobiera zaliczkę przez PayU lub Stripe ([NaCzarter, regulamin](https://naczarter.pl/regulamin)), nie dowodzi legalności takiego przepływu dla Nautivo, bo nie znamy ich struktury umownej ani statusu regulacyjnego.

**Rekomendacja.** Nie budować biznesu na wyłączeniu agenta i nie przyjmować środków uczestników na żaden rachunek Nautivo. Prawnikowi przekazać pytania: czy Nautivo może kierować, opóźniać lub cofać wypłaty, czy 3% jest opłatą rozdzielaną przez operatora, czyją nazwę widzi uczestnik na wyciągu, kto jest klientem operatora i czy Nautivo podpisuje cokolwiek w imieniu operatora (art. 151). Unikać w materiałach określenia „usługi płatnicze”. **[PRAWNIK]**

**Nie ustalono.** Nie znaleziono stanowiska KNF wobec marketplace'ów ani art. 6 pkt 2. Status AML Nautivo poza ścieżką MIP nie został zbadany.

### „Agent” znaczy co innego w trzech reżimach

**Wniosek, który łączy kilka notatek.** To samo słowo ciągnie w różne strony. W VAT handel we własnym imieniu na rzecz osoby trzeciej oznacza, że podatnik sam otrzymał i wyświadczył usługę, więc podstawą może stać się wartość brutto, a nie 3% ([Ustawa o VAT, art. 8 ust. 2a](https://eli.gov.pl/api/acts/DU/2025/775/text/O/D20250775.pdf)). W usługach płatniczych agent musi działać dla jednej strony. W turystyce agent sprzedający imprezy na podstawie umowy agencyjnej może działać tylko dla organizatorów wpisanych do rejestru lub mających siedzibę w UE albo państwie z art. 8 ust. 2 ([Ustawa o imprezach turystycznych, art. 4 pkt 9, art. 7 ust. 4](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)). Umowa „pod VAT” może więc stworzyć problem turystyczny. **Rekomendacja:** zlecić jeden wspólny opis ról, a nie trzy oddzielne opinie. **[PRAWNIK] [KSIĘGOWY]**

### Rejs z noclegiem to prawdopodobnie impreza turystyczna

**Potwierdzone.** Impreza łączy co najmniej dwa rodzaje usług turystycznych i spełnia art. 5 ust. 1, na przykład przez cenę łączną albo zakup w jednym punkcie sprzedaży, którym może być strona internetowa. Wyłączone są imprezy okazjonalne, niezarobkowe i dla ograniczonej grupy oraz trwające poniżej 24 godzin bez noclegu (art. 3). Organizator podlega wpisowi do rejestru marszałka, zabezpieczeniu, składkom TFG i odpowiada za wszystkie usługi (art. 7, 22, 48, 50); warunki mniej korzystne dla podróżnego są nieważne (art. 54). **Działanie bez wpisu lub zabezpieczenia, także jako członek zarządu spółki, fundacji lub stowarzyszenia, zagrożone jest do 3 lat pozbawienia wolności** (art. 59–60) ([Ustawa o imprezach turystycznych, t.j., stan z 2026-07-31](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)). Motyw 17 dyrektywy odróżnia nocleg w transporcie od rejsu, a motyw 18 wymienia wynajem sprzętu sportowego i próg 25% wartości ([Dyrektywa 2015/2302, tekst przyjęty](https://www.legislation.gov.uk/eudr/2015/2302/adopted?view=plain)). Prawnicy WDG Hauslag relacjonują, że ministerstwo, nie wymieniając jachtów, odpowiedziało, iż obowiązek ciąży na każdym, kto faktycznie spełnia kryteria, a PZŻ nie zajął stanowiska; tekst jest parafrazą bez sygnatury pisma ([nowezagle.pl](https://nowezagle.pl/organizacja-rejsow-morskich-a-obowiazki-prawne/)).

**Wniosek (klasyfikacja autorska, każdy wiersz do sprawdzenia). [PRAWNIK]**

| Produkt | Prawdopodobny wynik | Dlaczego i co jest niepewne |
|---|---|---|
| Rejs wielodniowy ze skipperem i kojami, jedna cena | **Prawdopodobnie impreza** | Art. 4 pkt 1 lit. a + b, art. 5 ust. 1 pkt 2 lit. b, motyw 17. Kontrargument: nocleg nierozerwalny z przewozem, a załoga może nie być „pasażerami”. |
| Czarter bez załogi, jedna usługa | **Raczej nie** | Potrzeba dwóch rodzajów usług. Lit. c mówi o „pojazdach silnikowych”; status jachtu żaglowego nieustalony. |
| Czarter + płatny skipper, transfer, prowiant | **Zależy** | Próg 25% i test elementu istotnego (art. 5 ust. 2). |
| Kurs bez noclegu i transportu | **Nie** | Jeden rodzaj usługi (lit. d). |
| Kurs z noclegiem, ponad 24 h | **Prawdopodobnie impreza** | Lit. b + d. Wyjątek kursów językowych z motywu 17 nie wygląda na pasujący. |
| Wydarzenie jednodniowe bez noclegu | **Poza ustawą** | Art. 3 pkt 3. |

Rola platformy zależy od projektu produktu. Pojedyncza impreza stworzona przez jednego organizatora nie czyni z Nautivo twórcy, ale prowizja od sprzedaży może oznaczać rolę agenta, co zabroniłoby sprzedaży dla organizatorów bez wpisu. Koszyk łączący usługi różnych dostawców grozi statusem organizatora (art. 5 ust. 1 pkt 2 lit. a). Osobny wybór i płatność podczas jednej wizyty oraz celowany cross-sell grożą statusem ułatwiającego powiązane usługi (art. 6). Przekazanie innemu przedsiębiorcy imienia, e-maila i danych płatniczych w ciągu 24 godzin czyni przekazującego organizatorem (art. 4 pkt 8, art. 5 ust. 1 pkt 2 lit. e). Organizator z turystycznym rachunkiem powierniczym musi przyjmować wpłaty wyłącznie na ten rachunek (art. 7 ust. 2 pkt 3), więc potrącenie 3% przed wpływem wygląda na konflikt.

**Rekomendacja.** Bez koszyka wielu dostawców, bez cross-sellu między dostawcami, bez marki „rejsy Nautivo”, jeden organizator nazwany w każdej umowie, a w onboardingu weryfikacja wpisu do rejestru albo deklarowana podstawa z art. 3. Organizatorów z rachunkiem powierniczym na razie wykluczyć z płatności w Nautivo.

**Co dalej.** Dyrektywa (UE) 2026/1024 weszła w życie 28 maja 2026, transpozycja do 29 września 2028, stosowanie od 29 marca 2029, a powiązane usługi turystyczne znikają jako osobna kategoria ([Komisja Europejska, Transition Pathways](https://transition-pathways.europa.eu/tourism/legislation/revised-package-travel-directive-now-force); [Garrigues](https://blogturismo.garrigues.com/en/european-union/the-european-union-discontinues-linked-travel-arrangements-and-reorganizes-the-legal-framework-governing-package-travel)). Tekstu dyrektywy nie przeczytano bezpośrednio, a o polskiej transpozycji nic nie znaleziono.

### Stowarzyszenia nie mają zwolnienia ze względu na status

**Potwierdzone.** Ustawa nie wymienia stowarzyszeń ani członków; jedyna droga to art. 3 pkt 1 (okazjonalnie, niezarobkowo, wyłącznie dla ograniczonej grupy), a przedsiębiorcą turystycznym może być osoba „prowadząca działalność odpłatną” (art. 4 pkt 7) ([Ustawa o imprezach turystycznych](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)). Motyw 19 podaje przykład wyjazdów „not more than a few times a year” organizowanych przez kluby dla członków ([Dyrektywa 2015/2302](https://www.legislation.gov.uk/eudr/2015/2302/adopted?view=plain)). Podobnie, jako stanowisko organizacji branżowej, a nie organu, pisze Polska Izba Turystyki ([Stanowisko PIT](https://cmrt.com.pl/wp-content/uploads/2023/11/Jak-identyfikowac-impreze-turystyczna-Stanowisko-Rady-Polskiej-Izby-Turystyki.pdf)). FAQ ministerstwa ma wpisy o stowarzyszeniach, ale strona zwróciła błąd 503 i widziano tylko tytuły pytań ([FAQ MSiT](https://www.msit.gov.pl/pl/turystyka/prawo/7951,Najczesciej-zadawane-pytania-dot-Ustawy-o-imprezach-turystycznych-i-powiazanych-.html)).

**Wniosek.** Bezpieczniej wygląda kilka rejsów rocznie po kosztach dla istniejącego członkostwa. Niebezpiecznie: program przez większość sezonu, nadwyżka albo członkostwo nabywane przy rezerwacji. **Publiczne ogłoszenie w marketplace może podważać „ograniczoną grupę”**, a wpływ prowizji 3% lub ceny powyżej kosztów na „niezarobkowość” pozostaje nierozstrzygnięty. To ma bezpośredni skutek dla pilotażu: trzej partnerzy z bezpośredniego dostępu powinni startować w trybie zamkniętym, widocznym tylko dla członków.

**Rekomendacja.** Od każdego z trzech stowarzyszeń zebrać liczbę rejsów rocznie, krąg uczestników, sposób kalkulacji ceny i ewentualny wpis do rejestru, zanim cokolwiek zostanie uruchomione, a odpowiedź z FAQ ministerstwa przeczytać ręcznie. **[PRAWNIK]**

### Obowiązki platformy są znośne przy małej skali, z wyjątkiem DAC7

**Potwierdzone.** DSA: mechanizm zgłoszeń z art. 16 dotyczy dostawców hostingu niezależnie od wielkości, a mikro i małe przedsiębiorstwa są zwolnione z sekcji platformowej (poza art. 24 ust. 3), sekcji B2C i raportów przejrzystości. Art. 6 ust. 3 odbiera ochronę w prawie konsumenckim, gdy przeciętny konsument sądzi, że usługa pochodzi od platformy ([DSA art. 6](https://www.eu-digital-services-act.com/Digital_Services_Act_Article_6.html), [art. 16](https://www.eu-digital-services-act.com/Digital_Services_Act_Article_16.html), [art. 19](https://www.eu-digital-services-act.com/Digital_Services_Act_Article_19.html), [art. 29](https://www.eu-digital-services-act.com/Digital_Services_Act_Article_29.html); nieoficjalne lustro, sprawdzić w EUR-Lex). **Polska ustawa wdrażająca DSA (Dz.U. 2026 poz. 1296) wchodzi w życie 6 listopada 2026, dzień po dacie weryfikacji**: koordynatorem jest Prezes UKE, a rozdział 3 ustawy o świadczeniu usług drogą elektroniczną zostaje uchylony ([Ustawa o ś.u.d.e.](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20021441204/U/D20021204Lj.pdf); [Lege Artis](https://czasopismo.legeartis.org/2026/10/koordynator-spraw-uslug-cyfrowych-urzad-komunikacji-elektronicznej-rozporzadzenie-dsa/)). Art. 8 tej ustawy wymaga regulaminu udostępnionego przed zawarciem umowy.

**DAC7 dla usług nie ma progu minimalnego**: próg dotyczy tylko towarów, a usługi są raportowane od pierwszej transakcji ([MF](https://podatki-arch.mf.gov.pl/wyjasnienia/co-warto-wiedziec-o-dyrektywie-dac7/)). Działalność objęta to m.in. usługi osobiste i udostępnianie środka transportu. Raport składa się do końca stycznia, dokumentację trzyma 5 lat, kara sięga 1 000 000 PLN, a po dwóch bezskutecznych ponagleniach operator musi wstrzymać wypłatę (art. 75o ust. 5) albo zablokować działalność, gdy wstrzymanie jest niemożliwe (ust. 6). Definicja „podmiotu” wymienia osoby prawne, jednostki i umowy spółki, nie wymienia osób fizycznych ([Ustawa o wymianie informacji podatkowych](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170000648/U/D20170648Lj.pdf)).

P2B wymaga jasnych warunków, 15 dni na zmiany i 30 dni na zakończenie współpracy, ujawnienia parametrów rankingu, przy zwolnieniu małych firm z wewnętrznego systemu skarg i mediatora ([Rozporządzenie 2019/1150](https://www.legislation.gov.uk/eur/2019/1150/adopted?view=plain)). Omnibus (art. 12a ustawy o prawach konsumenta) wymaga ujawnienia parametrów rankingu, statusu sprzedawcy na podstawie jego deklaracji i podziału obowiązków. Art. 38 ust. 1 pkt 12 wyłącza prawo odstąpienia dla usług wypoczynkowych i sportowych z oznaczonym terminem, lecz umowy o imprezę idą osobną drogą (art. 3 ust. 1 pkt 8), z prawem odstąpienia w każdym czasie za rozsądną opłatą (art. 47 ustawy turystycznej) ([Ustawa o prawach konsumenta, stan z 2026-10-29](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20140000827/U/D20140827Lj.pdf)). Platforma ODR została zamknięta, a odniesienia trzeba było usunąć do 20 lipca 2025 ([Prawo.pl](https://www.prawo.pl/biznes/uokik-e-sklepy-musza-zmienic-zapisy-o-platformie-odr,531199.html)). Twierdzenie o autentyczności opinii bez rozsądnej weryfikacji jest zakazaną praktyką; ustawy nie przeczytano bezpośrednio, opis pochodzi z [Lege Artis](https://czasopismo.legeartis.org/2022/12/zamieszczanie-falszywych-opinii-konsumentow-obowiazek-weryfikacji-recenzji-klientow/).

**Wniosek.** Marketplace prawdopodobnie jest platformą w DSA i usługą P2B, a sam SaaS może być tylko hostingiem. **Dostrzeżony konflikt wymagań:** DAC7 zakłada zdolność wstrzymania wypłaty, a przy strukturze Stripe, w której to Stripe odpowiada za straty, platforma nie może wstrzymywać wypłat ani obciążać kont ([Stripe, zarządzanie ryzykiem](https://docs.stripe.com/connect/risk-management)). Zostaje więc blokada z art. 75o ust. 6. Z drugiej strony każda funkcja wstrzymania zwiększa kontrolę nad środkami, która jest problemem na gruncie ustawy płatniczej. **[PRAWNIK] [KSIĘGOWY]**

**Wniosek RODO (role funkcjonalne, nie deklaratywne).** Nautivo jest prawdopodobnie podmiotem przetwarzającym dla list uczestników i formularzy w SaaS, administratorem dla kont marketplace, opinii, DAC7 i wykrywania nadużyć, a współadministratorem przy wspólnej promocji lub wspólnych profilach; własna analityka na danych najemców czyni je administratorem w tym zakresie ([RODO art. 28](https://www.legislation.gov.uk/eur/2016/679/article/28/adopted)). Dane zdrowotne w formularzach załogi powinny być minimalne, dostępne tylko organizatorowi i szybko usuwane, a marketplace nie powinien ich zbierać. Profile małoletnich nie powinny być publiczne. Nie znaleziono wytycznych UODO dla takiego modelu, retencji logów ani danych zdrowotnych na wyjazdach młodzieżowych. **[PRAWNIK]**

## Bez rejestracji można walidować, ale nie prowadzić publicznego modelu prowizyjnego

**Potwierdzone.** Od 1 stycznia 2026 limit działalności nierejestrowanej jest kwartalny: **225% płacy minimalnej, czyli 10 813,50 PLN przychodu należnego na kwartał** ([podatki.gov.pl](https://podatki.gov.pl/poradniki-i-informatory/dzialalnosc-nierejestrowana)). Może z niej korzystać osoba fizyczna, która nie prowadziła działalności przez ostatnie 60 miesięcy. **Wyłączona jest działalność wymagająca zezwolenia, koncesji lub wpisu do rejestru działalności regulowanej.** Po przekroczeniu limitu jest 7 dni na wpis do CEIDG ([biznes.gov.pl](https://www.biznes.gov.pl/pl/portal/00115)). Płaca minimalna 2027 to 4 950 PLN ([Portal Oświatowy](https://www.portaloswiatowy.pl/zmiany-w-prawie-oswiatowym/ile-wyniesie-minimalne-wynagrodzenie-za-prace-w-2027-r.-rozporzadzenie-podpisane-26518.html)), więc przy tej samej stawce limit wyniósłby 11 137,50 PLN (obliczenie, nie komunikat urzędowy).

Podatnik zwolniony wystawia fakturę na żądanie nabywcy. **Od 1 stycznia 2027 każda faktura B2B przechodzi przez KSeF, co wymaga NIP uzyskanego przez VAT-R i NIP-7** ([Podręcznik KSeF MF](https://ksef.podatki.gov.pl/media/qzlha3vx/podrecznik-ksef-20-cz-ii-wystawianie-i-otrzymywanie-faktur-w-ksef-20260209.pdf)). Limit zwolnienia z VAT to 240 000 PLN, a usługi prawne, doradztwo i windykacja wraz z faktoringiem wyłączają zwolnienie (art. 113 ust. 13). **Kupno usług zagranicznych objętych art. 28b wymaga zgłoszenia VAT-UE przed pierwszym zakupem** (art. 97 ust. 3 pkt 1), także przy działalności nierejestrowanej ([Ustawa o VAT](https://eli.gov.pl/api/acts/DU/2025/775/text/O/D20250775.pdf)), a podatnik zwolniony nalicza VAT bez prawa do odliczenia ([Poradnik Przedsiębiorcy](https://poradnikprzedsiebiorcy.pl/-import-uslug-a-podatnik-zwolniony-z-vat)).

Operatorzy zamykają ścieżkę od strony umów. Stripe dopuszcza „businesses (including sole proprietors)” i organizacje non-profit, bez wzmianki o działalności nierejestrowanej ([Stripe SSA PL](https://stripe.com/legal/ssa/pl)). Tpay wymaga przedsiębiorcy prowadzącego działalność albo podmiotu niebędącego osobą fizyczną ([Tpay OWU](https://tpay.com/tpay-owu-2)). Rejestracja P24 kieruje do firm (fragment, [P24](https://registration.przelewy24.pl/new-account/0444ae7a-9865-4c69-9882-8893fd10b30c)). Autopay dopuszcza działalność nierejestrowaną jako zwykłego sprzedawcę, bez kart ([Autopay](https://developers.autopay.pl/online/dzialalnosc-nierejestrowana)). **Nie znaleziono operatora oferującego marketplace działalności nierejestrowanej.** Sformułowanie PayU „natural person conducting business activity” jest niesprawdzoną interpretacją i nie należy na nim polegać ([PayU T&C](https://poland.payu.com/wp-content/uploads/sites/14/2026/05/Terms-and-conditions.pdf)).

**Wniosek: co limit oznacza dla 3%.** Jeśli przychodem jest tylko prowizja, limit odpowiada 360 450 PLN obrotu kwartalnie, czyli około 131 kojom po 2 750 PLN. Jeśli sprzedaż we własnym imieniu powoduje liczenie wartości brutto, limit przekraczają już **4 takie koje na kwartał**. Czy art. 8 ust. 2a VAT przekłada się na test z art. 5 Prawa przedsiębiorców, nie zostało ustalone. **[KSIĘGOWY]** Osoba bez rejestracji odpowiada jak JDG całym majątkiem (wniosek, źródła nie odnoszą się do tego wprost), a kary z ustawy płatniczej i turystycznej są osobiste. Zastosowanie DAC7 do osoby fizycznej jest niejasne, co nie jest argumentem za startem w ten sposób, tylko pytaniem do prawnika.

**Odpowiedź.** Bez rejestracji da się wykonać wywiady, testy cen, prototyp i zamknięty pilotaż SaaS z abonamentem oraz prowizją fakturowaną ręcznie, jeśli organizator ma własne konto u operatora, a Nautivo nie przyjmuje środków, nie sprzedaje doradztwa i przed zakupem chmury oraz API załatwi VAT-R. Każde z tych założeń wymaga potwierdzenia. **[PRAWNIK] [KSIĘGOWY]** Publiczny marketplace, Stripe Connect i każda rola regulowana wymagają co najmniej JDG, a przy przedpłatach i wyższym ryzyku raczej sp. z o.o. Stowarzyszenie nie zastępuje firmy, bo dochód nie może trafiać do członków ([Prawo o stowarzyszeniach, art. 34](https://lexlege.pl/prawo-o-stowarzyszeniach/art-34/)). AIP opisują tylko stare źródła z 2014 i 2018 roku, które wskazują na możliwość zakończenia współpracy przy działalności wymagającej zezwoleń ([Bankier](https://www.bankier.pl/wiadomosc/Firma-w-AIP-biznes-na-probe-Wady-i-zalety-7224184.html)).

### Budżet 5 000 PLN zmienia wybór formy i pozostaje nierozstrzygnięty

Brief nie mówi, czy 5 000 PLN to kwota jednorazowa czy miesięczna, ani czy poza „konsultacją prawną” wyłączone są także księgowość i doradca podatkowy. Raport nie przyjmuje żadnego założenia. Stawki składek pochodzą z portali księgowych, bo strona ZUS zwróciła 404 ([iFirma](https://www.ifirma.pl/blog/skladki-zus-2026-ile-wynosza-aktualne-skladki-zus-dla-przedsiebiorcow/); [Norman](https://norman.finance/pl/narzedzia/ile-kosztuje-spolka-z-o-o)); w 2027 będą wyższe. **[KSIĘGOWY]**

| Forma, faza (2026, PLN miesięcznie, bez podatku, księgowości i narzędzi) | Składki | Jeśli 5 000 PLN miesięcznie | Jeśli 5 000 PLN jednorazowo |
|---|---|---|---|
| Nierejestrowana | 0 ZUS, PIT-36 od dochodu | Mieści się | Mieści się, ale limit i VAT od zakupów pozostają |
| JDG, ulga na start (6 mies.), skala / ryczałt ≤60 tys. | 432,54 / 498,35 | Zostaje ponad 4 500 | Razem z preferencyjnym ZUS starcza na około 8–9 mies. samych składek |
| JDG, ZUS preferencyjny (24 mies.), skala / ryczałt ≤60 tys. | 853,41 / 919,22 | Zostaje ponad 4 000 | Jak wyżej |
| JDG, pełny ZUS, skala | 2 220,83 | Zostaje około 2 780 | Około 2 mies. |
| Sp. z o.o., jeden wspólnik (model Norman, z 600 PLN księgowości) | 3 218,87 + 274 założenie przez S24 | Zostaje około 1 781 | Około 1,5 mies. |

Notatki podają „około 10 miesięcy” składek w fazie ulgi przy ryczałcie, ale ulga trwa tylko 6 miesięcy. Po uwzględnieniu przejścia na ZUS preferencyjny wychodzi około 8,2 miesiąca przy ryczałcie i 8,8 przy skali, bez jakichkolwiek innych wydatków. Warunki ulgi z art. 18 Prawa przedsiębiorców nie zostały potwierdzone w ustawie, a opis ulgi pochodzi z podsumowania wyszukiwania ([Money.pl](https://direct.money.pl/artykuly/porady/zus-przedsiebiorcy-ile-wynosza-skladki-i-jak-je-policzyc)).

**Rekomendacja warunkowa.** Przy budżecie jednorazowym: walidacja bez rejestracji, potem JDG na uldze dopiero przy pierwszym płatnym kliencie z Connect, a sp. z o.o. dopiero, gdy przychód ją finansuje. Przy budżecie miesięcznym: rejestracja sp. z o.o. przed publicznym marketplace jest osiągalna i daje bufor odpowiedzialności, ale nie immunitet ([Prawo.pl o art. 299 KSH](https://www.prawo.pl/biznes/kto-odpowiada-z-art-299-kodeksu-spolek-handlowych,517070.html)); art. 59 ustawy turystycznej wprost obejmuje członków zarządu. **[PRAWNIK] [KSIĘGOWY]**

### Status założyciela też jest nieznany i zmienia koszty

**Potwierdzone.** Etat z wynagrodzeniem co najmniej minimalnym znosi obowiązkowe składki społeczne z działalności, ale składka zdrowotna co do zasady zostaje ([podatki.biz](https://www.podatki.biz/artykuly/zbieg-tytulow-do-ubezpieczen-zus-pracownika-na-etacie_33_52616.htm)). Osoba bezrobotna ma limit 50% płacy minimalnej, 2 403 PLN w 2026 ([Zielona Linia](https://zielonalinia.gov.pl/bezrobotni-a-dzialalnosc-nierejestrowa/)). **Pytania do założyciela:** czy jest zatrudniony i za ile, czy prowadził działalność w ostatnich 60 miesiącach, czy jest bezrobotny, studentem, emerytem lub na świadczeniu, czy sprzedaje do obecnego lub byłego pracodawcy, czy umowa o pracę ma zakaz konkurencji lub przeniesienie praw do kodu, i skąd ma ubezpieczenie zdrowotne.

## Ekonomia: prowizja wynagradza skalę, a koszty stałe musi pokryć abonament

### Opłata operatora decyduje, czy 3% w ogóle coś zostawia

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

### Pilotaż na trzech stowarzyszeniach nie zarobi nawet na składki

**Potwierdzone.** Mediana przychodu NGO w 2023 to 50 000 PLN, mediana liczby członków stowarzyszenia to 25, a rezerwy finansowe ma 35% organizacji; dane nie dotyczą żeglarstwa ([Klon/Jawor](https://api.ngo.pl/media/get/258002)). Średnia organizacja PZŻ ma około 43 osoby (22 158 / 518, obliczenie na danych z [raportu PZŻ 2024](https://pya.org.pl/files/sprawozdania/sprawozdanie-z-dzialalnosci-pzz-za-rok-2024.pdf)). Rozkładu nie opublikowano.

**Wniosek (scenariusz, nie prognoza).** Gdyby cały przychód stowarzyszenia na poziomie mediany szedł przez Nautivo, co jest górnym i nierealistycznym założeniem, prowizja wyniosłaby 1 500 PLN rocznie, a od trzech 4 500 PLN rocznie, czyli 375 PLN miesięcznie. Abonament 149 PLN to 3,6% takiego przychodu, a 299 PLN to 7,2%. Kluby z mariną lub flotą mogą wyglądać zupełnie inaczej. Skoro nie ma danych o budżetach, stowarzyszenia należy traktować jako partnerów projektowych i kanał dotarcia, a nie jako źródło przychodu. W pilotażu rozsądny jest abonament zerowy lub symboliczny w zamian za dane, wywiady i referencje, tym bardziej że Anolla i Bókun mają plany bezpłatne ([Anolla](https://anolla.com/pl/oprogramowanie-do-wynajmu-lodzi); [Bókun](https://www.bokun.io/pricing)).

### Próg opłacalności wyznaczają składki, a nie ambicja

Poniższa tabela to arytmetyka autorska. Cena abonamentu Nautivo nie jest znana, więc 149 i 299 PLN netto to kotwice cenowe Portivo i Boatera, a nie propozycje. Koje liczone są po 2 750 PLN (mediana z 4 ofert, z których 2 zweryfikowano), czyli 82,50 PLN prowizji. Tabela pomija koszty narzędzi, VAT, podatek dochodowy i wynagrodzenie założyciela, a poza modelem sp. z o.o. także księgowość.

| Koszt miesięczny do pokrycia | PLN | Sam obrót przy 3% (PLN/mies.) | Lub koje 7-dniowe/mies. | Lub klienci po 149 | Lub klienci po 299 |
|---|---|---|---|---|---|
| JDG, ulga, skala | 432,54 | 14 418 | 6 | 3 | 2 |
| JDG, preferencyjny, skala | 853,41 | 28 447 | 11 | 6 | 3 |
| JDG, preferencyjny, ryczałt ≤60 tys. | 919,22 | 30 641 | 12 | 7 | 4 |
| JDG, pełny ZUS, skala | 2 220,83 | 74 028 | 27 | 15 | 8 |
| Sp. z o.o. (model Norman) | 3 218,87 | 107 296 | 40 | 22 | 11 |

Prowizja jest wysoce sezonowa, bo lipiec i sierpień to 51,4% ruchu turystycznego maj–wrzesień w Warmińsko-Mazurskiem (dane o całym województwie, nie o żeglarzach) ([Analiza ruchu turystycznego](https://warmia.mazury.pl/images/Departamenty/Departament_Turystyki/forum_turystyki/forum2024/Analiza_ruchu_turystycznego.pdf)). Składki są stałe. Abonament wyrównuje ten rozjazd, ale poza sezonem klient odczuwa go jako czysty koszt.

### 3% to dużo mniej niż marketplace, ale nie jest tanie wobec SaaS

**Wniosek.** Dla organizatora 3% + abonament jest dużo tańsze niż prowizje marketplace'ów (tabela niżej, 11,5–29%, a do 35% w ujęciu pośrednim), lecz tylko równe Portivo i wyższe niż Boater (1,5%) oraz yachtCMS (0%). Przy 5 jachtach i 200 000 PLN płatności online rocznie Boater kosztuje około 6 588 PLN, Portivo około 7 788 PLN, a yachtCMS około 548 PLN, bez opłat operatora. Nautivo z abonamentem 149 PLN i 3% kosztowałoby tyle samo co Portivo. Jest też odwrotna luka: **jednolite 3% nie zarabia na popycie, który przyniesie marketplace**, a Getmyboat rozdziela stawki na 14,5% dla rezerwacji z platformy i 3% dla bezpośrednich poza USA ([Getmyboat](https://www.getmyboat.com/for-owners/)).

**Rekomendacja do testu (odstępstwo od briefu oznaczone wprost).** Zachować abonament + 3% dla własnych rezerwacji organizatora, testując trzy rzeczy: 0% na płatnościach offline, które są normą u Bókun, Checkfront, TuriTop i Portivo, choć Broad Reach pobiera 1,75% także od gotówki ([Broad Reach](https://www.broadreachbookings.com/pricing)); przełącznik przenoszenia opłaty na klienta, po sprawdzeniu prezentacji ceny całkowitej z art. 12 ust. 1 pkt 5 **[PRAWNIK]**; oraz wyższą stawkę tylko za popyt pozyskany przez marketplace, co wykracza poza brief i wymaga decyzji założyciela. Brief mówi o „każdej faktycznej płatności”, więc trzeba też zdefiniować, czy obejmuje ona płatności offline i czy zwrócona płatność jest „faktyczna”.

## Rynek: 548–936 policzalnych organizacji, ale brak danych o gotowości do płacenia

**Potwierdzone: organizacje.** Na 31.12.2024 PZŻ miał 553 członków: 35 okręgowych związków i 518 innych organizacji, razem 22 158 osób. W 2024 liczba organizacji spadła z 547 do 518: 11 przyjęto, 31 wykluczono za składki lub warunki formalne, 7 zrezygnowało, 2 rozwiązano. Licencji klubów sportowych było 128 ([Raport PZŻ 2024](https://pya.org.pl/files/sprawozdania/sprawozdanie-z-dzialalnosci-pzz-za-rok-2024.pdf)). Wykaz ministerstwa z 05.10.2026 ma 518 decyzji; automatyczne dopasowanie nazw daje około 210 odrębnych podmiotów, z czego 56 to stowarzyszenia, kluby, WOPR, ZHP i podobne, 33 to sp. z o.o., a 118 to głównie jednoosobowe lub nieustalone ([Wykaz podmiotów](https://www.gov.pl/attachment/d9006683-f406-4719-9573-c25adb44cbb5)). Liczba 518 występuje w obu źródłach, ale oznacza co innego: organizacje w PZŻ oraz decyzje w wykazie.

**Potwierdzone: wolumen szkoleń i ceny.** W 2024 wydano 13 418 patentów żeglarza jachtowego i 2 378 jachtowego sternika morskiego, ponad 80% po egzaminach w podmiotach uprawnionych ([Raport PZŻ 2024](https://pya.org.pl/files/sprawozdania/sprawozdanie-z-dzialalnosci-pzz-za-rok-2024.pdf)). PZMWiNW wydał 70 473 uprawnienia, w tym 59 757 sterników motorowodnych po egzaminach, i wskazuje, że szkolenia maleją lub ich brak ([Raport PZMWiNW 2024](https://www.motorowodniacy.org/dokumenty/statut_i_akty_prawne/sprawozdanie_z_prac_zarzadu_pzmwinw_za_okres_od_01012024_do_31122024_r.pdf)). Rozporządzenie nie wymaga kursu przed egzaminem ([Dz.U. 2013 poz. 460](https://eli.gov.pl/api/acts/DU/2013/460/text.html)); brak licencji na szkolenie to wniosek z milczenia przepisów. **[PRAWNIK]**

| Oferta (ceny katalogowe, próby małe) | Wartość PLN | 3% PLN | Podstawa |
|---|---|---|---|
| Czarter mazurski, tydzień szczytu | 4 900 | 147,00 | Mediana 19 modeli × 7 dni, [Poljacht](https://poljacht.pl/cennik/) |
| Czarter mazurski, tydzień wiosną | 2 800 | 84,00 | Jw. |
| Koja, rejs 7-dniowy | 2 750 | 82,50 | n = 4, zweryfikowane [Sailing Factory](https://sailingfactory.pl/rejsy-morskie/) i [Yachtica](https://yachticacharter.pl/terminy-trasy-i-ceny-rejsow-w-chorwacji/) |
| Kurs żeglarza jachtowego, dorośli | 2 140 | 64,20 | n = 3, m.in. [Busola](https://busolamazury.pl/), [Lazy Jack](https://lazyjack.pro/oferta-klasyczne.html) |
| Kurs sternika motorowodnego | 659 | 19,77 | Promocja, Warszawa, [JMaster](https://www.jmaster.pl/szkolenia) |

**Potwierdzone: rytm płatności.** Poljacht bierze 40% w 7 dni i 60% do 4 tygodni przed czarterem ([Poljacht](https://poljacht.pl/cennik/)), Maxus 50% w 3 dni i resztę na 30 dni przed ([Maxus](https://www.sztynort.pl/czarter-jachtow-zaglowych/)), a Żeglarstwo Skipper 50% w tydzień i resztę miesiąc przed rejsem ([Żeglarstwo Skipper](https://www.zeglarstwo-skipper.pl/rejsy-i-szkolenia/)). First Minute NAUTIGO na 2026 skończył się 31.01.2026 ([NAUTIGO](https://czarter-jachtow-mazury.pl/)). Sailing Factory ma już rejsy do grudnia 2027, a jego kalendarz obejmuje zimowe Kanary i Azory ([Sailing Factory](https://sailingfactory.pl/rejsy-morskie/)).

**Wniosek: skala.** Policzalne organizacje to **548 do 936**, gdzie 548 to same organizacje PZŻ w Polsce (553 minus 5 klubów polonijnych), a 936 to suma bez nakładania się: 548 + 218 + 170, przy czym 170 pochodzi z niepotwierdzonego fragmentu. Bez 28 stowarzyszeń klasowych górna granica spada do 908. Zakres nie jest rynkiem całkowitym: pomija organizatorów rejsów, kluby spoza PZŻ i szkoły bez uprawnień egzaminacyjnych, a nakładania się zbiorów nie zmierzono, więc nie wolno z niego liczyć środka. Nie policzono SAM, SOM ani przychodu, bo konwersja i cena są nieznane.

**Wniosek: wartość.** Górna arytmetyka wydatków na kursy, tylko gdyby każdy kandydat kupił kurs w cenie z próby, to 28,7 mln PLN dla żeglarstwa i 39,4 mln PLN dla motorowodnych. To nie jest wielkość rynku, bo udział kupujących kurs jest nieznany. Stara teza Click&Boat o 100 mln EUR polskiego rynku czarterów pochodzi z około 2020 roku i od strony zainteresowanej ([money.pl](https://www.money.pl/gielda/click-boat-wolumen-polskiego-rynku-czarterow-lodzi-wynosi-ok-100-mln-euro-6549983985907329a.html)); nie należy jej używać. Podobnie 10–14 tys. jachtów na Mazurach to nieokreślona estymacja z [NAUTIGO](https://czarter-jachtow-mazury.pl/), a nie flota czarterowa.

**Wniosek: segmenty.** Motorowodni to wolumen około 4,5 raza większy od żeglarskiego, ale niska wartość koszyka (19,77 PLN) i słaby popyt na kursy, więc nie są oczywistym pierwszym celem. Kurs żeglarski i koja dają 64–83 PLN prowizji na osobę. Na Mazurach PZŻ ma tylko 22 organizacje przy około 30 osobach na organizację, więc tamtejszy popyt wygląda na komercyjny. Kalendarz sugeruje sprzedaż do organizatorów śródlądowych przed okresem styczeń–wiosna, a zaliczki rozłożone przez zimę i wiosnę z saldami skupionymi od końca maja do lipca; to wyprowadzono z warunków, nie zmierzono.

**Niewiadome.** Nie ma danych o wydatkach klubów i szkół na oprogramowanie, używanych narzędziach, wrażliwości na cenę, czasie wyprzedzenia rezerwacji, obłożeniu i obrocie, liczbie firm czarterowych według CEIDG/KRS ani liczbie sprzedawców rejsów (zaobserwowano próbkę 8 firm). PZŻ ma też własne [e-biuro](https://pya.org.pl/files/e-biuro/FAQ-ebiuro-pya.pdf) (fragmenty) do licencji i członkostwa, więc te funkcje mogą nie być czymś, za co członkowie zapłacą osobno. Zbiory można oczyścić przez dopasowanie NIP/KRS, zaczynając od publicznego wykazu ministerstwa.

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

## Lista kontrolna do 21-LEGAL-CHECKLIST

Status odzwierciedla notatki na dzień weryfikacji. Bramki odnoszą się do proponowanych etapów: **W** walidacja bez płatności, **P** zamknięty pilotaż SaaS, **K** pobieranie 3% przez operatora, **M** publiczny marketplace.

| # | Zagadnienie | Pytanie lub działanie | Kto | Blokuje |
|---|---|---|---|---|
| 1 | Forma działalności | Czy i kiedy nierejestrowana jest dopuszczalna; JDG czy sp. z o.o.; czy odpowiedzialność karna idzie za członkiem zarządu | Prawnik, Księgowy | P |
| 2 | Status założyciela | Zatrudnienie, 60 miesięcy, zakaz konkurencji, prawa do kodu | Założyciel, Księgowy, Prawnik | P |
| 3 | Budżet | Rozstrzygnąć: jednorazowy czy miesięczny, co obejmuje | Założyciel | W |
| 4 | VAT zakupów | VAT-R, VAT-UE przed pierwszym zakupem zagranicznym; czy opłaty Stripe to wyzwalają | Księgowy | P |
| 5 | KSeF | NIP, VAT-R, NIP-7 przed fakturami B2B od 1 stycznia 2027; rachunek czy faktura | Księgowy | P |
| 6 | Zakres oferty | Bez doradztwa i windykacji/faktoringu, które wyłączają zwolnienie; brak interpretacji KIS dla SaaS i prowizji | Księgowy | P |
| 7 | Podstawa VAT | Prowizja czy brutto (art. 8 ust. 2a), wpływ na limity 10 813,50 i 240 000 PLN | Księgowy | K |
| 8 | PKWiU i ryczałt | 8,5% czy 12% ([KIS 2023](https://www.gazetaprawna.pl/podatki/artykuly/10756808,czy-stawka-podatku-dla-uslug-utrzymania-aplikacji-internetowych-w-modelu-saas-wynosi-85.html)); interpretacje chronią tylko opisany stan | Księgowy | K |
| 9 | Kasa fiskalna | Limit startowy 20 000 PLN B2C; czy karta spełnia poz. 42 ([Dz.U. 2024 poz. 1902](https://eli.gov.pl/api/acts/DU/2024/1902/text/O/D20241902.pdf)); sprawdzić nowelizację 2026 | Księgowy | M |
| 10 | Usługa płatnicza | Czy direct charges lub konta organizatorów zostawiają Nautivo poza art. 3; ewentualne zapytanie do KNF; PSR art. 2(2)(b) | Prawnik | K |
| 11 | AML | Status Nautivo poza MIP, niezbadany | Prawnik | K |
| 12 | Rola agenta | Czy 3% wymaga umowy agencyjnej; art. 4 pkt 9, art. 7 ust. 4; co przy organizatorze bez wpisu | Prawnik | K |
| 13 | Klasyfikacja produktów | Rejs, czarter, kurs; ewentualne pismo do MSiT lub marszałka | Prawnik | K |
| 14 | Test checkoutu | Art. 5 ust. 1 pkt 2 lit. a i e, art. 6, gotowość na dyrektywę 2026/1024 | Prawnik | M |
| 15 | Rachunek powierniczy | Zgodność z płatnością przez operatora i potrąceniem 3% (art. 7 ust. 2 pkt 3) | Prawnik | K |
| 16 | Stowarzyszenia | Granice art. 3 pkt 1, publiczne ogłoszenie, prowizja a „niezarobkowo”, odpowiedzialność zarządu; rozliczenie podatkowe stowarzyszenia | Prawnik, Księgowy | P |
| 17 | DAC7 | Mapowanie usług, czy zamknięty SaaS to platforma, status osoby fizycznej, pierwszy raport, funkcja wstrzymania, ścieżka operatora wyłączonego; schemat XML niezweryfikowany | Księgowy, Prawnik | K |
| 18 | DSA | Status mikro/małego w grupie (progi niezweryfikowane), podział hosting/platforma, mechanizm zgłoszeń; art. 11–14, 17–18 sprawdzić bezpośrednio | Prawnik | P |
| 19 | Regulaminy | Art. 8 ś.u.d.e., P2B, art. 12a i kolejne ustawy o prawach konsumenta, klauzule abuzywne (niezbadane), brak linku ODR | Prawnik | P |
| 20 | Prawo odstąpienia | Dopasowanie art. 38 ust. 1 pkt 12 do czarteru i szkoleń; vouchery i kursy bez daty; warunki anulacji | Prawnik | K |
| 21 | Opinie | Weryfikacja przez zakończoną rezerwację; przeczytać ustawę o nieuczciwych praktykach, która była niedostępna | Prawnik | M |
| 22 | RODO | Mapa ról, umowa z art. 28, DPIA, zdrowie, małoletni, retencja logów; art. 4, 5, 6, 8, 13–14, 30, 32, 35, 37 nie zostały sprawdzone w tej rundzie | Prawnik | P |
| 23 | Małoletni | Ustawa Kamilka i przepisy o wypoczynku dzieci i młodzieży, niezbadane | Prawnik | P |
| 24 | Odpowiedzialność i ubezpieczenia | Delikt i umowa przy wypadku, limity, OC/tech E&O, cyber, D&O; wymóg ubezpieczenia organizatorów. Rynek ubezpieczeń niezbadany | Prawnik | M |
| 25 | Żegluga | Kwalifikacje skippera, bezpieczeństwo jachtów, prawo żeglugi; poza zakresem badań | Prawnik | M |
| 26 | VAT turystyczny | Procedura marży u organizatorów i traktowanie prowizji, poza zakresem | Księgowy | K |

**Pierwszy zakres konsultacji.** Jeśli budżet na prawnika jest ograniczony, największą wartość mają pozycje 1, 10, 12, 13 i 16, bo rozstrzygają o możliwości startu i o ryzyku karnym, a z księgowych pozycje 4, 5 i 7.

## Niewiadome, ograniczenia i konflikty źródeł

### Granice materiału

**EUR-Lex nie był dostępny dla narzędzi badawczych**, więc DSA odczytano z nieoficjalnego lustra, PSD2, P2B, RODO i dyrektywę 2015/2302 z tekstów „adopted” na legislation.gov.uk, a dyrektywę 2026/1024 tylko ze źródeł wtórnych. Polskie ustawy płatnicze, turystyczne, konsumenckie, e-usługowe i o wymianie informacji odczytano z PDF-ów ISAP według dat kompilacji; późniejsze zmiany nie byłyby widoczne. Tekst VAT pochodzi z Dz.U. 2025 poz. 775 i nie zawiera zmian z 2026, więc bieżące kwoty pochodzą z MF. Niedostępne były FAQ MSiT, ustawa o nieuczciwych praktykach w ISAP, pełny PDF wytycznych EDPB 07/2020, art. 5 Prawa przedsiębiorców, tabele ZUS, autopay.pl, developers.przelewy24.pl, pełny cennik PayU, umowa marketplace PayU, załącznik kartowy Tpay i druga połowa umowy Stripe. Tekst dokumentu Rady jest kompromisem, a nie ogłoszonym aktem. Odpowiedzi EBA dotyczą przepisów z dnia publikacji i nie są systematycznie przeglądane.

Nie zbadano też: czasu i kosztu budowy, stosu i kosztów infrastruktury Nautivo, rzeczywistych cen ubezpieczeń i księgowości, liczby klientów konkurentów, akceptacji Stripe dla polskich stowarzyszeń, ani tego, czym dokładnie są trzy stowarzyszenia dostępne założycielowi. Przy tych punktach raport nie podaje liczb.

### Konflikty źródeł

| Temat | Konflikt | Stan |
|---|---|---|
| Rachunek w działalności nierejestrowanej | [biznes.gov.pl](https://www.biznes.gov.pl/pl/portal/00115): rachunek na żądanie; [Podręcznik MF](https://ksef.podatki.gov.pl/media/qzlha3vx/podrecznik-ksef-20-cz-ii-wystawianie-i-otrzymywanie-faktur-w-ksef-20260209.pdf): obowiązek faktury ma pierwszeństwo | Nierozstrzygnięty. **[KSIĘGOWY]** |
| Limit działalności nierejestrowanej | Wersja EN biznes.gov.pl i [Autopay](https://developers.autopay.pl/online/dzialalnosc-nierejestrowana) pokazują stary limit 75%; podatki.gov.pl kwartalny | Obowiązuje kwartalny |
| Zastosowanie PSR | 21 miesięcy w dokumencie Rady; niektóre trackery 18 lub 24 | Wygląda na nieaktualne trackery; czekać na Dz.U. |
| Stan PSR | [Open Banking Tracker](https://www.openbankingtracker.com/guides/psd3-psr-readiness) widzi kroki w toku; tytuł [Matheson](https://www.matheson.com/insights/psd3-and-psr-published-whats-changing-for-eu-payment-services/) „published”, nieotwarty | Sprawdzić Dz.U. |
| Reforma ADR | Transpozycja 20.03.2028 ([EAPIL](https://eapil.org/2026/01/23/directive-eu-2025-2647-amending-the-adr-framework-for-consumer-disputes/)) lub stosowanie 20.09.2028 ([Digital Policy Alert](https://digitalpolicyalert.org/change/7397)) | Nierozstrzygnięty |
| Druga ustawa DSA | Sejm czy Senat ([Lege Artis](https://czasopismo.legeartis.org/2026/10/koordynator-spraw-uslug-cyfrowych-urzad-komunikacji-elektronicznej-rozporzadzenie-dsa/), [DSK](https://dsk-kancelaria.pl/blog/prezydent-rp-podpisal-jedna-z-ustaw-wdrazajacych-dsa-w-polsce)) | Nierozstrzygnięty |
| Opinie konsumenckie | [Spider's Web](https://spidersweb.pl/2026/04/uokik-nieuczciwe-opinie-kary.html): ciężar dowodu i 12 miesięcy odstąpienia | Niepotwierdzone, nie polegać |
| Karty Tpay | [Oferta](https://tpay.com/oferta): stawki; [PDF 2026](https://tpay.com/user/assets/files_for_download/pakiety-serwisowe-2026.pdf): warunki indywidualne; [inny PDF](https://tpay.com/user/assets/files_for_download/tpay-oferta.pdf): BLIK 1,78% + 0,39 (fragment) | Potwierdzić z Tpay |
| PayU | Oferta w [T&C](https://poland.payu.com/wp-content/uploads/sites/14/2026/05/Terms-and-conditions.pdf) obowiązywała do 31.05.2026, a [strona](https://poland.payu.com/oferta-handlowa/) nadal pokazuje 1,1% i 1,49% | Stawka promocyjna mogła wygasnąć |
| Autopay | 1,25% + 0,34 (fragment) i 1,19% + 0,34 na innych stronach | Potwierdzić |
| SamBoat | 20% ([pomoc](https://www.samboat.com/help/how-much-will-i-receive-per-rental-93)) i 18% ([Trustpilot, fragment](https://www.trustpilot.com/review/samboat.fr?page=3)) | Przewaga 20%, nierozstrzygnięty |
| Click&Boat | 26% (fragment) i starsze 15%; [recenzja afiliacyjna](https://yachtsforkings.pages.dev/brokers/click-and-boat-review/) 7–12%, zdyskontowana | Sprawdzić na koncie właściciela |
| Regiondo | [„No commission”](https://go.regiondo.com/pricing) i opłata za rezerwację; [TrekkSoft](https://www.trekksoft.com/en/blog/regiondo-pricing-guide-what-to-know-before-you-buy) podaje 0,49 EUR za bilet | Źródło konkurencyjne |
| Viking i eola | [Viking EN](https://www.vikingbookings.com/plans) i [NL](https://vikingbookings.com/nl/plans); [eola](https://eola.co/business/pricing) i [druga strona](https://business.eola.co/page/pricing) | Strony ze sobą sprzeczne |
| REJA24 2024 | 28 813 w PZŻ i 20 000 w [Portalu Morskim](https://www.portalmorski.pl/porty-logistyka/59486-szykuja-sie-zmiany-w-systemie-rejestracji-jachtow-reja-24) (fragment) | Nierozstrzygnięty, być może definicje |
| Mały ZUS Plus | Kto korzysta z resetu ([Poradnik Przedsiębiorcy](https://poradnikprzedsiebiorcy.pl/-maly-zus-plus-dla-kogo-i-na-jakich-warunkach-w-2026-r), [podatki.biz](https://www.podatki.biz/artykuly/maly-zus-plus-zmiany-od-2026_33_60253.htm)) | **[KSIĘGOWY]** |
| Kary KSeF | Art. 106ni od 2027, projekt przenosi kary za błędy na 2028 ([Infor](https://ksiegowosc.infor.pl/wiadomosci/7657331,mf-nie-bedziemy-karac-za-bledy-w-ksef-do-konca-2027-r-ale-niestosowanie-ksef-spowoduje-reakcje-organow-kas.html)) | Status druku niesprawdzony |

## Kolejność decyzji: najpierw odpowiedzi, potem kod płatności

**Rekomendacja, a progi są propozycją bez źródła.** **Etap W (teraz):** rozstrzygnąć budżet i status założyciela, zebrać dane od trzech stowarzyszeń, przeprowadzić 10–20 wywiadów i wysłać pytania do Stripe i P24. Wystarczy to bez rejestracji. **Etap P:** zamknięty SaaS dla członków stowarzyszeń na sezon 2027, po konsultacji z punktami 1, 4, 5, 16 i bez przyjmowania środków. **Etap K:** rejestracja i 3% przez direct charges lub fallback, gdy pilot pokaże realne płatności oraz co najmniej kilku komercyjnych organizatorów deklaruje zakup po podanej cenie. **Etap M:** publiczny marketplace dopiero po teście checkoutu i opinii o roli agenta. Rekomendację należy zmienić, jeśli wywiady pokażą, że organizatorzy zostają przy formularzach także przy abonamencie zerowym, jeśli prawnik uzna opłatę aplikacyjną za kontrolę nad środkami albo jeśli Stripe odrzuci MCC dla wszystkich trzech typów organizatorów.

## Podsumowanie

Pytanie „czy 3% działa” okazało się źle postawione: stawka jest najmniej ryzykownym elementem modelu. O legalności decyduje to, czyje są pieniądze w drodze, o ryzyku karnym to, jak zaprojektowany jest koszyk, a o przetrwaniu to, czy abonament pokrywa koszty, gdy prowizja jest sezonowa i niska. Bezpośredni dostęp do trzech stowarzyszeń jest cenny, ale w innej roli niż pierwszy przychód: to segment o najsłabszych przesłankach budżetowych, a tryb zamknięty, który chroni ich zwolnienie turystyczne, jest sprzeczny z logiką publicznego marketplace'u. Dlatego są one poligonem produktu, a pieniędzy trzeba szukać u komercyjnych organizatorów rejsów i szkół, co pozostaje hipotezą bez jednego punktu danych o gotowości do płacenia.

Najtańsza dostępna informacja nie jest prawna ani techniczna. Jedna odpowiedź założyciela o charakterze budżetu, jedno pytanie do Stripe o MCC i 10–20 rozmów z organizatorami mogą zmienić formę prawną, model płatności i docelowy segment, zanim zostanie napisana pierwsza linia kodu płatności. Jednocześnie napięcie między DAC7, które wymaga możliwości wstrzymania wypłaty, a ustawą płatniczą, przy której kontrola nad wypłatami jest ryzykiem, nie jest przeoczeniem do załatwienia w regulaminie: to konkretne pytanie, od którego powinna zacząć się konsultacja prawna.
