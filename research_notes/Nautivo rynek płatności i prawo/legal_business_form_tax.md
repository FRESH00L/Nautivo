# Polish business form and tax rules for a SaaS founder (subscription + commission), 2026 law

Verification date: 2026-11-05. This is research, not tax or legal advice. Items marked **[ACCOUNTANT]** need a tax adviser (doradca podatkowy) or a lawyer before anyone relies on them.

Source quality: statute quotes come from the consolidated VAT Act (Dz.U. 2025 poz. 775, published June 2025) and the cash-register regulation (Dz.U. 2024 poz. 1902), both read directly. The VAT text predates the 2026 amendments, so it still shows 200 000 PLN and an older art. 145m. The current figures come from the Ministry of Finance (MF) KSeF handbook (law as of 1 Feb 2026) and from podatki.gov.pl. isap.sejm.gov.pl was not read. ZUS amounts come from accounting portals, because the ZUS URL tried returned 404.

## 1. Działalność nierejestrowana in 2026: limit, eligibility, exclusions, exceeding the limit

### Takeaway
The monthly 75% rule ended on 31 Dec 2025. The 2026 limit is 225% of the minimum wage per quarter, which is **10 813,50 PLN of accrued revenue per quarter**. Nothing excludes SaaS as such, but activity needing a permit, licence or regulated-activity entry is excluded, and a commission model that handles other people's money could raise that issue.

### Cited Findings
- Since 1 Jan 2026 the limit is quarterly: 225% of the minimum wage, 10 813,50 PLN gross in 2026 (minimum wage 4 806 PLN). In 2025 it was monthly: 75%, 3 499,50 PLN. — [podatki.gov.pl, updated 26.05.2026](https://podatki.gov.pl/poradniki-i-informatory/dzialalnosc-nierejestrowana); [biznes.gov.pl](https://www.biznes.gov.pl/pl/portal/00115)
- The legal basis is art. 5 ust. 1 of the Prawo przedsiębiorców Act (Dz.U. 2025 poz. 1480). — [MF KSeF handbook part II](https://ksef.podatki.gov.pl/media/qzlha3vx/podrecznik-ksef-20-cz-ii-wystawianie-i-otrzymywanie-faktur-w-ksef-20260209.pdf)
- Eligibility: a natural person who has not run business activity in the last 60 months. A period of suspension counts as not running a business. — [biznes.gov.pl](https://www.biznes.gov.pl/pl/portal/00115)
- Exclusion: activity that requires a permit (zezwolenie), a licence (koncesja) or an entry in the register of regulated activity cannot be run without registration. — [biznes.gov.pl](https://www.biznes.gov.pl/pl/portal/00115)
- Exceeding the limit: the activity becomes business activity, and the person has 7 days from the moment of exceeding to register in CEIDG. — [biznes.gov.pl](https://www.biznes.gov.pl/pl/portal/00115)
- Revenue is counted as accrued (należny) on issued documents, even if unpaid. — [biznes.gov.pl search extract](https://www.biznes.gov.pl/pl/portal/00115)
- The 2027 minimum wage is set at 4 950 PLN (regulation of 14 Sep 2026, Dz.U. poz. 1213, in force 1 Jan 2027). — [Portal Oświatowy](https://www.portaloswiatowy.pl/zmiany-w-prawie-oswiatowym/ile-wyniesie-minimalne-wynagrodzenie-za-prace-w-2027-r.-rozporzadzenie-podpisane-26518.html)
- The English biznes.gov.pl page and some guides still show the old monthly rule, so they are out of date. — [biznes.gov.pl EN](https://www.biznes.gov.pl/en/portal/004051)

### Inferences
- 10 813,50 PLN per quarter averages 3 604,50 PLN per month and at most 43 254 PLN per year (arithmetic). The quarterly test permits an uneven month, unlike the 2025 rule.
- If 225% still applies, the 2027 limit would be 11 137,50 PLN per quarter (225% × 4 950). This is my calculation, not an official statement.
- **Commission trap:** art. 8 ust. 2a of the VAT Act treats a taxpayer who acts in its own name for a third party as having itself received and supplied the service. If the platform sells in its own name and collects the full price, the gross value, not only the commission, could count as its sales. Whether this also drives the art. 5 revenue test is a separate question. The agency model and contract design decide this. **[ACCOUNTANT]** ([VAT Act, art. 8 ust. 2a](https://eli.gov.pl/api/acts/DU/2025/775/text/O/D20250775.pdf))
- **Licence trap:** plain software subscriptions do not obviously need a licence. Collecting and passing on customer funds could be a regulated payment service, which would rule out unregistered activity. I did not analyse this; it needs a lawyer and the payments research. **[ACCOUNTANT/LAWYER]**
- Once the limit is exceeded, registration is compulsory and the follow-on costs in section 4 apply. The founder should have that path ready before launch.

### Gaps
- I could not open art. 5 itself (lexlege was blocked), so the numbers of its sub-paragraphs other than ust. 1 are unverified.
- No source addressed how a mid-quarter start affects the limit, or how refunds and chargebacks count beyond the page's general exclusion of returns and discounts. **[ACCOUNTANT]**

## 2. Documents, records, PIT and KSeF

### Takeaway
A VAT-exempt unregistered seller issues an invoice only on the buyer's request, within the 3-month window. But KSeF applies from **1 Jan 2027** to every B2B invoice, which requires a NIP obtained through VAT-R and NIP-7. That largely removes the "no registration anywhere" appeal for a B2B SaaS. Income goes on PIT-36 as other-source income, with documented costs deductible and no monthly advances.

### Cited Findings
**Invoices and bills**
- A taxpayer exempt under art. 113 ust. 1 and 9 issues an invoice only at the buyer's request (art. 106b ust. 3 pkt 2), and the KSeF rollout does not change this. The request must be made within 3 months from the end of the month of supply. — [MF handbook](https://ksef.podatki.gov.pl/media/qzlha3vx/podrecznik-ksef-20-cz-ii-wystawianie-i-otrzymywanie-faktur-w-ksef-20260209.pdf)
- **Conflict on bills:** biznes.gov.pl says you issue a rachunek at the buyer's request ([biznes.gov.pl](https://www.biznes.gov.pl/pl/portal/00115)). The MF handbook has a section titled "no possibility of issuing a rachunek by a taxpayer using the exemption" and says the invoice duty under VAT takes priority over art. 87 Ordynacja podatkowa ([MF handbook, section 1.3](https://ksef.podatki.gov.pl/media/qzlha3vx/podrecznik-ksef-20-cz-ii-wystawianie-i-otrzymywanie-faktur-w-ksef-20260209.pdf)). **[ACCOUNTANT]**
- Exempt taxpayers may issue simplified invoices. Those subject to KSeF must carry the seller's NIP, and a business buyer should give its NIP. — [MF handbook](https://ksef.podatki.gov.pl/media/qzlha3vx/podrecznik-ksef-20-cz-ii-wystawianie-i-otrzymywanie-faktur-w-ksef-20260209.pdf)

**Sales records and cash register**
- Art. 109 ust. 1 of the VAT Act: exempt taxpayers keep sales records for each day, no later than before the next day's sale. Under ust. 2, where records are missing or unreliable and sales cannot be established from documents, the authority estimates them; if the subject of taxation cannot be determined, 22% applies. — [VAT Act](https://eli.gov.pl/api/acts/DU/2025/775/text/O/D20250775.pdf)
- The records may be on paper or electronic. — [biznes.gov.pl](https://www.biznes.gov.pl/pl/portal/00115)
- Cash register exemptions run to 31 Dec 2027. Starters are exempt if expected B2C sales, proportional to the period, do not exceed 20 000 PLN (§3 ust. 1 pkt 2). — [Dz.U. 2024 poz. 1902](https://eli.gov.pl/api/acts/DU/2024/1902/text/O/D20241902.pdf)
- Electronic services are not on the §4 list of activities barred from the exemptions, as far as I read it; that list includes legal services and tax advice. The electronic-services entry in the annex (poz. 27) applies only if the taxpayer also provides telecoms services. — [Dz.U. 2024 poz. 1902](https://eli.gov.pl/api/acts/DU/2024/1902/text/O/D20241902.pdf)
- Annex poz. 42 exempts B2C services paid in full through post, bank or SKOK into the taxpayer's account, where the records and payment evidence clearly show what each payment was for. — [Dz.U. 2024 poz. 1902](https://eli.gov.pl/api/acts/DU/2024/1902/text/O/D20241902.pdf)
- A person using a cash register has a NIP as their tax identifier. — [MF handbook](https://ksef.podatki.gov.pl/media/qzlha3vx/podrecznik-ksef-20-cz-ii-wystawianie-i-otrzymywanie-faktur-w-ksef-20260209.pdf)

**PIT**
- Revenue is not business income. It is reported on PIT-36 between 15 Feb and 30 Apr of the following year, with no monthly advances. Costs can be deducted if documented, so purchase documents must be kept. — [podatki.gov.pl](https://podatki.gov.pl/poradniki-i-informatory/dzialalnosc-nierejestrowana)
- Tax is on the progressive scale. — [biznes.gov.pl](https://www.biznes.gov.pl/pl/portal/00115)

**KSeF**
- Stages: from 1 Feb 2026 for taxpayers with 2024 gross sales above 200 million PLN; from 1 Apr 2026 for other businesses. Until the end of 2026, the smallest may invoice outside KSeF if invoiced sales do not exceed 10 000 PLN gross per month (art. 145m ust. 1). — [MF handbook](https://ksef.podatki.gov.pl/media/qzlha3vx/podrecznik-ksef-20-cz-ii-wystawianie-i-otrzymywanie-faktur-w-ksef-20260209.pdf)
- Once the 10 000 PLN limit is exceeded, KSeF applies from the invoice that crossed it and to all later invoices. Falling below in a later month does not restore the relief. Only invoices that must go through KSeF count, so consumer invoices do not. — [KSeF page "Poniżej 10 000 zł"](https://ksef.podatki.gov.pl/ponizej-10-000-zl/)
- MF says unregistered sellers' duty to obtain a NIP arises "in principle" only on 1 Jan 2027, because of that relief. — [MF handbook](https://ksef.podatki.gov.pl/media/qzlha3vx/podrecznik-ksef-20-cz-ii-wystawianie-i-otrzymywanie-faktur-w-ksef-20260209.pdf)
- A NIP is required to use KSeF. An exempt person without CEIDG entry who will invoice through KSeF must first register for VAT with the status "VAT zwolniony" via VAT-R and NIP-7. The tax office assigns the NIP within at most 3 days. — [MF handbook](https://ksef.podatki.gov.pl/media/qzlha3vx/podrecznik-ksef-20-cz-ii-wystawianie-i-otrzymywanie-faktur-w-ksef-20260209.pdf)
- A person issuing only consumer invoices needs neither KSeF nor a NIP. — [MF handbook](https://ksef.podatki.gov.pl/media/qzlha3vx/podrecznik-ksef-20-cz-ii-wystawianie-i-otrzymywanie-faktur-w-ksef-20260209.pdf); B2C invoices may optionally go through KSeF. — [podatki.gov.pl](https://podatki.gov.pl/poradniki-i-informatory/dzialalnosc-nierejestrowana)
- Penalties under art. 106ni are due to apply from 1 Jan 2027, with secondary sources citing up to 100% of VAT or 18,7% of the invoice total. A government bill sent to the Sejm on 20 Oct 2026 (reported as druk 3245) would move penalties for errors to 1 Jan 2028. MF says non-use of KSeF will still draw a KAS response. — [Infor](https://ksiegowosc.infor.pl/wiadomosci/7657331,mf-nie-bedziemy-karac-za-bledy-w-ksef-do-konca-2027-r-ale-niestosowanie-ksef-spowoduje-reakcje-organow-kas.html); [Infor on rates](https://ksiegowosc.infor.pl/ksef/7603120,kary-w-ksef-wedlug-wskaznika-100-i-187-nie-bedzie-zlagodzenia-bo-przepisy-sa-juz-lagodne.html)

### Inferences
- Because 10 813,50 PLN is the quarterly revenue ceiling, an unregistered seller could exceed 10 000 PLN of invoices in a month only by bunching nearly a whole quarter's revenue. In 2026, then, unregistered sellers will almost always stay outside mandatory KSeF. From 1 Jan 2027 the relief ends and any requested B2B invoice must go through KSeF.
- B2B customers will generally want invoices to book costs, so a B2B SaaS should plan for VAT-R, NIP-7 and KSeF from day one.
- Ryczałt appears unavailable for unregistered activity, since official guidance names PIT-36 and the scale.
- **Card payments and the cash register:** whether a card or Stripe payment meets poz. 42 ("through a bank... into the taxpayer's account", identifiable to each sale) is unresolved. The 20 000 PLN starter exemption covers an early-stage B2C product. Past it, a cash register or a confirmed poz. 42 route is needed. **[ACCOUNTANT]**

### Gaps
- PIT article numbers (commonly cited as art. 10 ust. 1 pkt 9 PIT) were not read in the statute, so they are unverified.
- I found no official format for the sales record beyond the daily timing rule.
- The current status of druk 3245 was not checked in the Sejm record.
- A 2026 amendment to the cash-register regulation exists (reported in search results). I read only the 2024 text, so it needs checking.

## 3. VAT: exemption limit, art. 113 ust. 13, foreign purchases, VAT-UE

### Takeaway
The exemption limit is **240 000 PLN** from 1 Jan 2026. SaaS, electronic services and commission are not listed in art. 113 ust. 13, but **consulting (doradztwo), legal services and debt collection/factoring are**, and they remove the exemption for the taxpayer. The biggest trap: buying EU or foreign services under art. 28b as an exempt taxpayer requires VAT-UE registration **before** the first purchase, plus self-assessed VAT that cannot be deducted. This applies to unregistered activity too.

### Cited Findings
**Limit**
- 240 000 PLN applies from 1 Jan 2026. — [podatki.gov.pl](https://podatki.gov.pl/poradniki-i-informatory/dzialalnosc-nierejestrowana); [MF handbook](https://ksef.podatki.gov.pl/media/qzlha3vx/podrecznik-ksef-20-cz-ii-wystawianie-i-otrzymywanie-faktur-w-ksef-20260209.pdf)
- Art. 113 ust. 5: the exemption lapses from the transaction that crosses the limit. Ust. 9: for those starting mid-year, the limit applies in proportion to the period of activity. Ust. 10: the exemption lapses when actual sales exceed that proportional limit. Ust. 11: the exemption can be reclaimed no sooner than one year after the end of the year in which it was lost or given up. Ust. 4: voluntary resignation needs written notice. — [VAT Act](https://eli.gov.pl/api/acts/DU/2025/775/text/O/D20250775.pdf)

**Art. 113 ust. 13 (exclusions)**
- Pkt 2 excludes taxpayers providing: (a) legal services; (b) services in the field of consulting (doradztwo), except specified agricultural advice; (c) jewellery services; (d) debt collection, including factoring. — [VAT Act](https://eli.gov.pl/api/acts/DU/2025/775/text/O/D20250775.pdf)
- Pkt 1 concerns goods only, including excise goods, new means of transport and specified goods sold at a distance. No service-type exclusion refers to electronic services, software or intermediation. — [VAT Act](https://eli.gov.pl/api/acts/DU/2025/775/text/O/D20250775.pdf)
- A KIS interpretation reportedly held that legal and advisory services exclude the exemption for the whole activity, not only for those services. — [Gazeta Prawna](https://www.gazetaprawna.pl/podatki/artykuly/10756029,czy-uslugi-mediacji-oraz-uslugi-prawne-i-doradcze-korzystaja-ze-zwolnienia-z-opodatkowania-vat.html)

**Foreign purchases**
- Art. 97 ust. 3 pkt 1: exempt taxpayers must notify the tax office in the registration form **before the first such purchase** (via ust. 1, applied accordingly) if they buy services under art. 28b that would be import of services for them. Art. 97 ust. 4: the office registers them as VAT-UE taxpayers. — [VAT Act](https://eli.gov.pl/api/acts/DU/2025/775/text/O/D20250775.pdf)
- Art. 17 ust. 1 pkt 4 makes the buyer the VAT payer where the supplier has no seat or fixed establishment in Poland. — [VAT Act](https://eli.gov.pl/api/acts/DU/2025/775/text/O/D20250775.pdf)
- Under the MF handbook, unregistered activity is a VAT taxpayer under art. 15. — [MF handbook](https://ksef.podatki.gov.pl/media/qzlha3vx/podrecznik-ksef-20-cz-ii-wystawianie-i-otrzymywanie-faktur-w-ksef-20260209.pdf)
- An exempt buyer self-assesses but cannot deduct. It files VAT-9M and pays by the 25th of the following month. The tax paid can be treated as a cost. Secondary source; art. numbers for VAT-9M were not read in the statute. — [Poradnik Przedsiębiorcy](https://poradnikprzedsiebiorcy.pl/-import-uslug-a-podatnik-zwolniony-z-vat)
- **Conflict resolved by statute:** secondary sources disagree on whether exempt buyers file VAT-UE summaries. [Poradnik Przedsiębiorcy](https://poradnikprzedsiebiorcy.pl/-import-uslug-a-podatnik-zwolniony-z-vat) says no filing; another search result said they do, but I did not open it or identify which page it was. Art. 100 ust. 1 pkt 4 covers services **supplied** in other member states, not purchased. So purchases alone do not appear to generate a VAT-UE filing. — [VAT Act](https://eli.gov.pl/api/acts/DU/2025/775/text/O/D20250775.pdf)

**Selling abroad**
- Art. 97 ust. 3 pkt 2 and art. 100 ust. 1 pkt 4: B2B services to other EU taxpayers under art. 28b need VAT-UE registration and summary filings. — [VAT Act](https://eli.gov.pl/api/acts/DU/2025/775/text/O/D20250775.pdf)
- Art. 28k: B2C electronic services are generally taxed where the consumer is, subject to the small-supplier conditions in ust. 2. — [VAT Act](https://eli.gov.pl/api/acts/DU/2025/775/text/O/D20250775.pdf)

### Inferences
- **Stack costs:** with reverse charge at 23%, each 1 000 PLN net of foreign services would cost 230 PLN of non-deductible VAT for an exempt seller (illustrative, assumes a 23% taxable service). Cloud, AI API and similar bills from EU and non-EU vendors are the likely triggers, so exemption is not free on a SaaS stack.
- **Unregistered activity does not avoid this:** VAT-R is a tax registration, separate from CEIDG. An unregistered person can hold "VAT zwolniony" status and a VAT-UE number without leaving art. 5. Buying under a consumer account to avoid this looks legally unsound where the purchase is for the activity. **[ACCOUNTANT]**
- **Scope discipline:** selling paid implementation advice, business consulting or "strategy" alongside SaaS could end the exemption. Keep the offer to software access, and have the adviser review descriptions. SaaS as "electronic service" and commission as "intermediation" are not listed, but this reading comes from the absence of those terms, and no KIS ruling on SaaS or commission under art. 113 ust. 13 was found. **[ACCOUNTANT]**
- **Debt collection/factoring (lit. d):** paying merchants early or collecting their debts could trigger the exclusion. **[ACCOUNTANT]**
- **Commission VAT:** when exempt, no VAT is charged. When active, commission is taxed under general rules. The agent versus own-name question (art. 8 ust. 2a) decides the taxable base and how fast 240 000 PLN is reached. **[ACCOUNTANT]**
- Payment-processor fees may differ from cloud services, since some financial services are VAT-exempt. Whether Stripe fees trigger art. 97 ust. 3 pkt 1 needs review. **[ACCOUNTANT]**

### Gaps
- The art. 28k ust. 2 EUR threshold was not read in full, so it is not quoted here.
- I found no KIS ruling on SaaS or platform commissions under art. 113 ust. 13.
- Vendor-specific VAT treatment (Stripe, AWS, Anthropic and others) depends on the contracting entity and invoice; not checked.
- Whether Stripe accepts an unregistered Polish individual as a merchant was not checked; the payments research should cover this.

## 4. Alternatives and costs: JDG, sp. z o.o., AIP; liability

### Takeaway
A JDG on ulga na start costs about **433–498 PLN per month** in mandatory contributions for 6 months, then about **853–955 PLN** on preferential ZUS for 24 months, excluding accounting and tax. A sole-shareholder sp. z o.o. costs about **3 219 PLN per month** in one published model. That fits a monthly 5 000 PLN budget but not a one-off 5 000 PLN budget. The JDG leaves unlimited personal liability; the sp. z o.o. limits it, except for board-member liability rules.

### Cited Findings
**ZUS 2026 (secondary sources; ZUS itself not reached)**
- Full ZUS: base 5 652 PLN; 1 788,29 PLN without sickness cover, 1 926,76 PLN with it. — [iFirma](https://www.ifirma.pl/blog/skladki-zus-2026-ile-wynosza-aktualne-skladki-zus-dla-przedsiebiorcow/)
- Preferential ZUS: base 1 441,80 PLN (30% of 4 806 PLN); 420,87 PLN without sickness cover, 456,19 PLN with it. — [iFirma](https://www.ifirma.pl/blog/skladki-zus-2026-ile-wynosza-aktualne-skladki-zus-dla-przedsiebiorcow/)
- Health contribution, Feb–Dec 2026: scale and linear minimum 432,54 PLN (January 314,96 PLN). Ryczałt: 498,35 PLN up to 60 000 PLN of annual revenue; 830,58 PLN for 60 000–300 000 PLN; 1 495,04 PLN above that. — [iFirma](https://www.ifirma.pl/blog/skladki-zus-2026-ile-wynosza-aktualne-skladki-zus-dla-przedsiebiorcow/) (the same article's summary misprints 860,58 PLN; [Norman](https://norman.finance/pl/narzedzia/ile-kosztuje-spolka-z-o-o) confirms 830,58 PLN)
- Ulga na start: 6 months, social contributions only, health contribution still due. Preferential ZUS then runs 24 months. — [Money.pl guide](https://direct.money.pl/artykuly/porady/zus-przedsiebiorcy-ile-wynosza-skladki-i-jak-je-policzyc) (from a search summary across several portals; page not opened, attribution to this exact page unconfirmed)
- Mały ZUS Plus: base between 30% of minimum wage and 60% of forecast average pay; at least 60 days of activity in the prior year; annual revenue up to 120 000 PLN. — [iFirma](https://www.ifirma.pl/blog/skladki-zus-2026-ile-wynosza-aktualne-skladki-zus-dla-przedsiebiorcow/)
- Mały ZUS Plus runs 36 months within a 60-month period, reset under 2026 rules. Sources differ on exactly who benefits from the reset. — [Poradnik Przedsiębiorcy](https://poradnikprzedsiebiorcy.pl/-maly-zus-plus-dla-kogo-i-na-jakich-warunkach-w-2026-r); [podatki.biz](https://www.podatki.biz/artykuly/maly-zus-plus-zmiany-od-2026_33_60253.htm)
- Wakacje składkowe: one month per calendar year without social contributions; health contribution still due. Conditions: CEIDG sole trader, up to 10 insured people including the owner, turnover up to EUR 2 million, not working for a former employer, within de minimis limit (EUR 300 000 over 3 years). Filed by RWS via eZUS in the preceding month; for December 2026, by 30 Nov 2026. — [inFakt, 30 Oct 2026](https://www.infakt.pl/blog/?p=84365)

**Ryczałt**
- 12% covers software-related services (PKWiU ex 62.01.1, ex 62.02), and 8,5% is the general services rate. — [podatki.biz](https://www.podatki.biz/artykuly/ryczalt-85-proc-w-it-nizsza-stawka-tylko-gdy-nie-jestes-programista_34_63591.htm)
- KIS (2023) applied 8,5% to maintaining web applications in a SaaS model under PKWiU 74.90.20.0. — [Gazeta Prawna](https://www.gazetaprawna.pl/podatki/artykuly/10756808,czy-stawka-podatku-dla-uslug-utrzymania-aplikacji-internetowych-w-modelu-saas-wynosi-85.html)
- KIS (Sept 2026, 0115-KDST2-2.4011.539.2026.3.KK) applied 8,5% to PKWiU 62.02.30.0 services not linked to software advisory. — [Infor](https://www.infor.pl/twoje-pieniadze/7541731,korzystna-interpretacja-kis-prowadzenie-projektow-it-ze-stawka-85.html)
- Writing source code is reported at 12%. Interpretations protect only the described facts. — [Infor](https://ksiegowosc.infor.pl/podatki/ryczalt/dzialalnosc-gospodarcza/6307807,uslugi-architekta-oprogramowania-jaka-stawka-ryczaltu-co-to-sa-uslugi-zwiazane-z-oprogramowaniem.html)

**Sp. z o.o.**
- S24 setup with 5 000 PLN capital: 274 PLN (250 PLN court fee, 24 PLN PCC). Notary route: up to 718,80 PLN plus extracts. The 100 PLN MSiG fee ended on 29 Nov 2025. — [Norman, law as of 9 Oct 2026](https://norman.finance/pl/narzedzia/ile-kosztuje-spolka-z-o-o)
- Model for a sole shareholder: 1 788,29 PLN ZUS + 830,58 PLN health + 600 PLN net accounting = 3 218,87 PLN per month, 38 900,44 PLN in year one, excluding CIT, dividend tax and salaries. The 600 PLN is the model's assumption. — [Norman](https://norman.finance/pl/narzedzia/ile-kosztuje-spolka-z-o-o)

**AIP (old sources)**
- AIP lends its legal personality. Its NIP and REGON appear on contracts and invoices, and pay-out is by civil-law contract signed by AIP. Cost about 300–500 PLN per month. Non-students can join. Article dated 2018. — [Bezprawnik](https://bezprawnik.pl/aip-czyli-firma-bez-zus-i-to-nie-tylko-dla-studentow-ale-cudow-nie-ma/)
- Limits: contract reviews up to 5 working days, slow withdrawals, and AIP can end cooperation if activity needs permits or licences it does not cover. Article dated 2014. — [Bankier](https://www.bankier.pl/wiadomosc/Firma-w-AIP-biznes-na-probe-Wady-i-zalety-7224184.html)

**Liability**
- A sole trader answers with all personal assets, with no separation from business assets. A non-entrepreneur spouse is not liable with personal assets, though community property can be reached. A civil partnership does not fix this. — [Poradnik Przedsiębiorcy](https://poradnikprzedsiebiorcy.pl/-odpowiedzialnosc-przedsiebiorcy-przy-jednoosobowej-dzialalnosci-gospodarczej); [FakturaXL](https://www.fakturaxl.pl/odpowiedzialnosc-wlasciciela-jednoosobowej-firmy)
- GDPR fines can hit a sole trader. They are not automatic: PUODO weighs remediation and cooperation. — [Poradnik Przedsiębiorcy](https://poradnikprzedsiebiorcy.pl/-rodo-w-firmie-jakie-sa-obowiazki-przedsiebiorcy); [Lege Artis, on CEIDG deregistration](https://czasopismo.legeartis.org/2025/02/wykreslenie-przedsiebiorcy-ceidg-status-administratora-rodo-umorzenie-postepowania-odpowiedzialnosci-wyrok/)
- Art. 299 KSH: board members answer for company debts when enforcement against the company fails, with defences including a timely bankruptcy petition. Separate rules cover tax arrears (art. 116 Ordynacja podatkowa) and ZUS (art. 31 of the social insurance act). — [Prawo.pl](https://www.prawo.pl/biznes/kto-odpowiada-z-art-299-kodeksu-spolek-handlowych,517070.html); [Standardy Prawa](https://standardyprawa.pl/standardy/10683)

### Inferences
Monthly mandatory contributions, calculated from the figures above. These exclude income tax, accounting, insurance and stack costs. They use 2026 rates, and 2027 will be higher.

| Form / phase | Without sickness cover | With sickness cover |
|---|---|---|
| Unregistered | 0 PLN ZUS (PIT on profit) | n/a |
| JDG, ulga na start, scale | 432,54 | n/a |
| JDG, ulga na start, ryczałt ≤60k | 498,35 | n/a |
| JDG, preferential, scale | 853,41 | 888,73 |
| JDG, preferential, ryczałt ≤60k | 919,22 | 954,54 |
| JDG, preferential, ryczałt 60–300k | 1 251,45 | 1 286,77 |
| JDG, full, scale minimum | 2 220,83 | 2 359,30 |
| JDG, full, ryczałt ≤60k | 2 286,64 | 2 425,11 |
| JDG, full, ryczałt 60–300k | 2 618,87 | 2 757,34 |
| Sp. z o.o., sole shareholder (Norman model) | 3 218,87 incl. 600 accounting | n/a |

- **5 000 PLN budget (the question does not say if monthly or total):** if monthly, every option fits, with about 1 781 PLN left in the sp. z o.o. model and over 4 500 PLN during ulga na start. If one-off, 5 000 PLN pays for about 10 months of ulga-phase ryczałt contributions before accounting, or about 1,5 months of the sp. z o.o. model after setup.
- **Sequencing:** ulga na start, then preferential ZUS. Mały ZUS Plus needs a prior-year record, so it comes later and depends on income. Wakacje składkowe saves one month of social contributions per year and requires CEIDG, so it does not apply to unregistered activity.
- **Liability by form:** an unregistered founder is a natural person, so I expect the same personal exposure as a JDG (no source addressed this directly). For a platform with payment or personal data, a breach claim or fine reaches personal assets in both. A sp. z o.o. creates a buffer but not immunity. Using a PCI-certified provider and never storing card data reduces exposure in practice but does not remove GDPR duties; lawyer review needed.
- **Practical recommendation for the report:** treat unregistered activity as a validation-stage option only if revenue is small, the VAT-R/VAT-UE steps are done, and no consulting or custom own-name money flow exists. If the platform will take customer funds, process significant data or scale fast, go directly to JDG, or to sp. z o.o. when liability dominates. A tax adviser should confirm this.
- **AIP:** a possible low-admin test, but the sources are old. A platform handling payments under another entity's NIP needs direct confirmation from AIP, including who is the data controller and merchant of record.

### Gaps
- ZUS's own 2026 table was not reached (the URL tried returned 404). All amounts are secondary, though consistent between iFirma and Norman where they overlap. **[ACCOUNTANT]**
- Art. 18 Prawo przedsiębiorców conditions for ulga na start (60-month and former-employer rules) were not confirmed from statute; check before relying on them. Only wakacje składkowe has a sourced former-employer condition.
- 2027 ZUS amounts were not found, apart from the minimum wage. By arithmetic, the preferential base becomes 1 485 PLN (30% × 4 950).
- No verified market price for JDG or sp. z o.o. accounting. Get quotes.
- PKWiU classification for this SaaS and for commission income is unresolved. PKWiU 58.29 and 63.11 had no usable ruling. A GUS classification opinion or a KIS interpretation is needed. **[ACCOUNTANT]**
- Art. 83 GDPR fine amounts were not sourced here, so they are not stated.
- Sole-shareholder versus multi-shareholder ZUS treatment was not verified.
- No 2026 AIP price list, terms or liability clause was found. A partial AIP regulation snippet mentioned liability but was truncated.

## 5. Does the founder's employment status matter?

### Takeaway
Yes, substantially, and it is **unknown here, so it must be asked**. Employment paying at least the minimum wage can remove compulsory social contributions on a JDG, but the health contribution generally remains. Being registered as unemployed sets a lower revenue threshold.

### Cited Findings
- If employment pays at least the minimum wage (4 806 PLN in 2026), no social contributions are due from the business. Below that, they are. Health is payable on both, with one narrow exception (social base not above the minimum, business revenue not above 50% of minimum wage per month, and ryczałt). The source names art. 6 ust. 1, art. 11 and art. 12 of the social insurance act. — [podatki.biz, 12.02.2026](https://www.podatki.biz/artykuly/zbieg-tytulow-do-ubezpieczen-zus-pracownika-na-etacie_33_52616.htm)
- A registered unemployed person may not exceed 50% of the minimum wage, 2 403 PLN in 2026. — [Zielona Linia](https://zielonalinia.gov.pl/bezrobotni-a-dzialalnosc-nierejestrowa/)
- Unregistered activity carries no ZUS contributions as a business activity. — [biznes.gov.pl](https://www.biznes.gov.pl/pl/portal/00115)
- Wakacje składkowe is unavailable if you work for your former employer. — [inFakt](https://www.infakt.pl/blog/?p=84365)

### Inferences
Questions to ask the founder:
1. Employed? Full-time, at or above the minimum wage? From 1 Jan 2027 that threshold is 4 950 PLN.
2. Any business activity in the last 60 months, including a past JDG? This could bar art. 5 and possibly ulga na start.
3. Registered as unemployed, student under 26, retired or on benefits? These change insurance and limits.
4. Will the SaaS sell to a current or former employer? This can affect reliefs.
5. Does an employment contract include a non-compete or IP-assignment clause? Check whether the employer could claim the code or object to the side business (lawyer).
6. Health insurance from another source? An unregistered person has no ZUS title from that activity, so coverage must come from elsewhere (inference; verify). **[ACCOUNTANT]**

Cost effect: a founder with qualifying employment may pay only the health contribution on a JDG. Compared with the table in section 4, this removes the social portion.

### Gaps
- Interplay of the 50% monthly unemployed limit with the 2026 quarterly test beyond the cited figure was not reviewed.
- Rules for students, pensioners and parental-benefit recipients were not researched, since their relevance is unknown.

## 6. Can an association (stowarzyszenie) run this?

### Takeaway
Technically it can run business activity, but it fits poorly: profits cannot go to members, and business cannot be the association's purpose. It should not be used for a founder-owned commercial SaaS.

### Cited Findings
- Art. 34 Prawo o stowarzyszeniach allows business activity, with income serving statutory goals and not being distributable to members. — [Lexlege, art. 34](https://lexlege.pl/prawo-o-stowarzyszeniach/art-34/); [Standardy Prawa](https://standardyprawa.pl/standardy/21114)
- A simple association (stowarzyszenie zwykłe) cannot run business activity (art. 42 ust. 1 pkt 3). — [ArsLege](https://arslege.pl/prawo-o-stowarzyszeniach/k35/s1233/) (from a search summary; page not opened, attribution to this exact page unconfirmed)
- A court has held that business activity cannot be an end in itself or the way to carry out statutory goals (XXIII Ga 1126/18). — [Dla Jurysty](https://dlajurysty.pl/orzeczenia/pokaz/292086/XXIII-Ga-1126-18)
- A business-running association is entered in the KRS business register. — [Rzeczpospolita](https://www.rp.pl/fundacje-i-stowarzyszenia/art16704891-jak-z-dzialacza-stac-sie-biznesmenem) (from a search summary; page not opened, attribution to this exact page unconfirmed)

### Inferences
- Registration, governance and accounts add burden with no ownership or profit route. It makes sense only for a genuine membership or mission project, not as a substitute for a company.

### Gaps
- The current consolidated text of the Act was not checked beyond the 2020 version shown by these sites. Not researched further, as instructed.
