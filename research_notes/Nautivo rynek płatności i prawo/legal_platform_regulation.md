# Nautivo: Polish and EU regulation of a sailing SaaS plus marketplace (3% commission)

**Verification date: 2026-11-05.** Input for a founder's lawyer brief. Not legal advice.

**How to read these notes**
- "Cited Findings" = statute text or a named source. "Inferences" = my reasoning, not law and not an official interpretation. Article numbers in Inferences repeat ones verified in Cited Findings.
- **Polish statutes were read directly** from Kancelaria Sejmu consolidated PDFs on ISAP, using the compilation dates printed in each file. A change after that date would not show:
  - Payment act: t.j. Dz.U. 2026 poz. 623, 875, 1074, 1206, compiled 2026-10-08.
  - Tourism act: t.j. Dz.U. 2026 poz. 925, 1006, compiled 2026-07-31.
  - Consumer act: t.j. Dz.U. 2026 poz. 1244, 1353, compiled 2026-10-29.
  - E-services act: compiled 2026-10-16.
  - Tax information exchange act: compiled 2026-10-08.
- **EUR-Lex could not be fetched by my tools.**
  - The DSA was read on an unofficial mirror (eu-digital-services-act.com).
  - The original texts of Directive 2015/2302, PSD2, P2B and GDPR were read on legislation.gov.uk's "adopted" views.
  - Directive 2026/1024 was read only through secondary sources.
  - All of these need a final check against EUR-Lex.
- **Unavailable to me:** the MSiT FAQ (HTTP 503), the ISAP PDF of the unfair market practices act (human-verification wall), and the EDPB Guidelines 07/2020 PDF (404 on the URL tried; only secondary summaries read).

---

## 1. Payment services: does the platform need a KNF licence or MIP registration?

### Takeaway
If Nautivo receives participant money and passes it to organisers, plan on that being a regulated payment service. The art. 6 pkt 2 commercial agent exclusion is limited to agents acting for only one side, and it gets narrower under the PSR. The workable design is a licensed PSP holding funds under its own licence, but counsel must test how much control over payouts Nautivo keeps.

### Cited Findings
**Licensing and sanctions (ustawa o usługach płatniczych)**
- Payment services include executing payment transactions (art. 3 ust. 1 pkt 2), acquiring (pkt 5) and money remittance (pkt 6). Art. 3 ust. 3 defines remittance as a service without a payer's payment account: funds received from a payer are transferred to the payee or the payee's provider, or received for a payee and made available to them. — [Ustawa o usługach płatniczych, t.j.](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20111991175/U/D20111175Lj.pdf)
- Art. 4 ust. 1: payment services may be provided only by payment service providers. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20111991175/U/D20111175Lj.pdf)
- Art. 150 ust. 1: unauthorised provision carries a fine of up to PLN 5,000,000, up to 2 years' imprisonment, or both. Ust. 3 extends this to whoever acts on behalf or in the interest of a natural person, legal person or organisational unit. Art. 150 ust. 2 also penalises using terms such as "usługi płatnicze" in a business name or advertising without being authorised. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20111991175/U/D20111175Lj.pdf)
- Art. 151 ust. 1: concluding a payment service contract in a provider's name or for a provider without its authorisation carries a fine of up to PLN 3,000,000, up to 2 years' imprisonment, or both. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20111991175/U/D20111175Lj.pdf)

**MIP (mała instytucja płatnicza)**
- Art. 2 pkt 17b: an MIP can be a natural person, legal person or organisational unit, entered in the register, providing services under art. 3 ust. 1 pkt 1–6 (not pkt 7–8). — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20111991175/U/D20111175Lj.pdf)
- Art. 117f: it is regulated activity (ust. 1), **only on Polish territory** (ust. 2), with a 12-month average monthly transaction total of up to EUR 1,500,000 (ust. 3). Other business is permitted (ust. 5). — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20111991175/U/D20111175Lj.pdf)
- Art. 117g: activity requires entry in the MIP register. Art. 117h requires AML organisation, a clean record for the manager, a 12-month programme and financial plan, and a risk procedure. Where it holds funds on user payment accounts, the limit is EUR 2,000 per user at any time (ust. 3). — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20111991175/U/D20111175Lj.pdf)
- Art. 117q: on exceeding the limit, within 30 days of the end of the period, scale down or apply for a permit under art. 60 ust. 1. Art. 117u: the MIP pays towards supervision costs, at a rate of up to 0.025% of total transaction value. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20111991175/U/D20111175Lj.pdf)
- Amendments are pending. For example, art. 4 ust. 2 pkt 2 gets a new wording from 11.01.2027 (Dz.U. 2026 poz. 1206). — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20111991175/U/D20111175Lj.pdf)

**Commercial agent exclusion**
- Art. 6 pkt 2: the act does not apply to payment transactions between payer and payee made through an authorised person performing acts towards concluding, or concluding, a sale or paid service contract "w imieniu lub na rzecz wyłącznie płatnika albo wyłącznie odbiorcy". — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20111991175/U/D20111175Lj.pdf)
- PSD2 art. 3(b): excludes transactions through a commercial agent "authorised via an agreement to negotiate or conclude the sale or purchase of goods or services on behalf of only the payer or only the payee". Recital 11 records that the exclusion was applied very differently across Member States. — [Directive 2015/2366 (adopted text)](https://www.legislation.gov.uk/eudr/2015/2366/adopted?view=plain)
- EBA Q&A 2020_5355 (final): a B2C e-commerce platform "could be excluded" if authorised by the payee to negotiate or conclude the sale "and if it does not also act on behalf of the payer". The competent authority evaluates this on the business model, and "PSD2 does not provide criteria" for when a platform also acts for the payer. — [EBA Q&A 2020_5355](https://eba.europa.eu/single-rule-book-qa/qna/view/publicId/2020_5355)
- EBA Q&A 2020_5354 (final, answer prepared by the Commission): a platform receiving payment for the payee, where receipt settles the payer's debt, "is not to be excluded by default". Debt settlement is not in itself a reason to exclude it. — [EBA Q&A 2020_5354](https://www.eba.europa.eu/single-rule-book-qa/qna/view/publicId/2020_5354)
- EBA states that Q&As refer to provisions at publication and are not systematically reviewed. — [same](https://www.eba.europa.eu/single-rule-book-qa/qna/view/publicId/2020_5354)
- Commentary: a platform acts for both sides where the payer pays into an account the platform controls and it then forwards funds under a separate agreement with the payee. — [Osborne Clarke](https://osborneclarke.com/insights/sharing-economy-platform-providers-how-they-may-be-caught-by-eu-payments-regulation) (search excerpt only, page not opened)

**PSD3/PSR status**
- Political agreement was reached on 27 Nov 2025. — [Open Banking Tracker](https://www.openbankingtracker.com/guides/psd3-psr-readiness)
- The Council published final compromise texts in April 2026 (PSR ST-8221-2026, PSD3 ST-8222-2026), and formal adoption, OJ publication and entry into force remained. — [Arthur Cox, 30 Apr 2026](https://www.arthurcox.com/insights/psd3-and-psr-final-compromise-texts-published/)
- A law-firm tracker reviewed in late August 2026 still listed plenary vote, Council adoption, signature and OJ publication as pending; another guide expected adoption by December 2026. These came from search summaries that did not tie each statement to a page, so treat them as indicative. — [Open Banking Tracker](https://www.openbankingtracker.com/guides/psd3-psr-readiness); [Safeheld](https://safeheld.com/resources/psd3-psr-safeguarding)
- Compromise text, dated 17 April 2026 and marked LIMITE, **art. 2(2)(b)**: the exclusion requires (i) an agreement authorising the agent to negotiate or conclude the sale "on behalf of only the payer or only the payee, but not both of them" and (ii) that the agreement "gives the commercial agent a real scope to negotiate with the payer or payee or conclude the sale". — [Council doc. 8221/26](https://data.consilium.europa.eu/doc/document/ST-8221-2026-INIT/en/pdf)
- **Recital 11:** this applies "regardless of whether or not the commercial agent is in the possession of client's funds". E-commerce platforms acting for both individual buyers and sellers "without buyers or sellers having any real scope or autonomy to negotiate" should not be excluded. EBA guidelines may include a repository of use cases. — [same](https://data.consilium.europa.eu/doc/document/ST-8221-2026-INIT/en/pdf)
- **Art. 2(7):** EBA guidelines on this exclusion are due one year after entry into force. — [same](https://data.consilium.europa.eu/doc/document/ST-8221-2026-INIT/en/pdf)
- **Application:** the PSR applies 21 months after entry into force, and arts. 50 and 57 at 27 months. — [same](https://data.consilium.europa.eu/doc/document/ST-8221-2026-INIT/en/pdf). Some trackers still report 18 or 24 months, which look stale against the Council text.
- The PSR scope list also reaches providers of hosting services for arts. 59a, 78 and 91 (fraud-related), and recitals refer to online platforms' art. 30 DSA data as relevant fraud-prevention information. I did not read those operative articles in full. — [same](https://data.consilium.europa.eu/doc/document/ST-8221-2026-INIT/en/pdf)

**PSP-holds-funds model**
- Stripe (a vendor, guide updated 2021) describes a model where money owed by buyer to seller is never in the platform's ownership or control: funds go to a regulated client-money account and the licensed provider pays the seller. The guide says platforms must verify their own position. — [Stripe, PSD2 and marketplaces](https://stripe.com/sv-fi/guides/how-psd2-impacts-marketplaces-and-platforms)

### Inferences
- A flow of participant to Nautivo account to organiser, minus 3%, most resembles execution or remittance under art. 3. If unlicensed, it exposes both the entity and the individuals acting for it under art. 150 ust. 3.
- A single-side agent argument (Nautivo as the organiser's agent only) is available in principle under art. 6 pkt 2. It is weak where Nautivo also has terms with and duties to participants, such as a marketplace account, cancellations or dispute handling. Standard terms with no negotiating role also look hard to fit into PSR art. 2(2)(b)(ii). **Do not build the business on the exclusion.**
- MIP is a fallback, not a target. It brings regulated status and AML, is Poland-only, and sits awkwardly with EU organisers or cross-border payers.
- "Never touches funds" is a functional claim that depends on contracts and on control. Questions for counsel on the PSP model:
  - Can Nautivo direct, delay or reverse payouts?
  - Is the 3% an application fee split by the PSP?
  - Whose name does the participant's statement show?
  - Who is the PSP's customer?
  - Does Nautivo sign anything on the PSP's behalf (art. 151 and agent status)?
- Payment and tourism rules interact. An organiser using a tourist trust account as its security must take participant payments **only on that account** (tourism act art. 7 ust. 2 pkt 3), which may be incompatible with a PSP marketplace balance. See section 2.

### Gaps
- **No KNF position on marketplaces or art. 6 pkt 2 was found** (searches of knf.gov.pl surfaced nothing; a KNF page URL I tried had no relevant content). A pre-launch KNF inquiry or counsel's knowledge of KNF practice is needed.
- I could not confirm PSR/PSD3 OJ publication on EUR-Lex; no source I saw reports it as of 2026-11-05. One title, "PSD3 and PSR published" ([Matheson](https://www.matheson.com/insights/psd3-and-psr-published-whats-changing-for-eu-payment-services/)), was not opened and probably refers to the compromise texts. **Check OJ directly.**
- Recital 11 of PSD2 was only partly extracted; the sentence on platforms should be read in the original.
- AML status of Nautivo outside the MIP route was not researched.

---

## 2. Tourism act: when is a sailing product a package, and when does the platform become organiser?

### Takeaway
A multi-day cruise with skipper and berths at one price is, on the statute and recital 17, likely a package (impreza turystyczna), though no official sailing-specific interpretation was found. The platform's largest tourism risk comes from product design: combining services in a basket, passing data on, or facilitating add-ons can make it organiser or LTA facilitator, and those duties cannot be contracted away towards travellers.

### Cited Findings
**Definitions (ustawa o imprezach turystycznych i powiązanych usługach turystycznych)**
- Art. 4 pkt 1, tourist services: (a) passenger carriage; (b) accommodation for purposes other than residential that is not an inseparable element of carriage; (c) rental of cars or other motor vehicles; (d) other services provided to travellers not integral to (a)–(c). — [Ustawa, t.j.](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 4 pkt 2: a package combines at least two different types of tourist service for the same trip or holiday and meets art. 5 ust. 1. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 5 ust. 1: a package arises when services are combined by one trader before contract, including at the traveller's request (pkt 1). It also arises, regardless of separate contracts, when they are (pkt 2):
  - (a) bought at one point of sale and selected before the traveller agrees to pay;
  - (b) offered or sold at an inclusive or total price;
  - (c) advertised using "impreza turystyczna" or similar;
  - (d) combined after a contract allowing choice among service types;
  - (e) bought through linked online booking where the first trader passes name, email and payment details to another trader and that contract is concluded within 24 hours of confirming the first booking. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 5 ust. 2: no package if one service of type (a), (b) or (c) is combined with type (d) services that are under 25% of the combined value and neither advertised as nor otherwise an essential element, or that are chosen and bought after the main service starts. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 3 exclusions: (1) offered or facilitated occasionally, non-profit and only to a limited group; (2) under a general contract for business travel; (3) lasting under 24 hours unless including an overnight stay. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 4 pkt 11: "point of sale" includes websites and online points of sale, including where presented to travellers as a single point of contact. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Directive recital 17: overnight accommodation as part of passenger transport is not a separate service if "the main component is clearly transport", "unlike in the case of a cruise". Accommodation for residential purposes, including long-term language courses, is outside. — [Directive 2015/2302 (adopted text)](https://www.legislation.gov.uk/eudr/2015/2302/adopted?view=plain)
- Directive recital 18: other tourist services include admission to sport events, excursions, guided tours, ski passes and "rental of sports equipment". At 25% or more of the value they count as significant. — [same](https://www.legislation.gov.uk/eudr/2015/2302/adopted?view=plain)
- Lawyers at WDG Hauslag argue that sailing cruises with accommodation and transport usually make the organiser a tourism entrepreneur. They report that the **Ministry replied, without naming yachts, that the duty applies to whoever actually meets the criteria regardless of self-classification, and that PZŻ took no position**. The article paraphrases; no letter reference is given. — [nowezagle.pl, Sept/Nov 2025](https://nowezagle.pl/organizacja-rejsow-morskich-a-obowiazki-prawne/)

**Organiser duties**
- Art. 22 ust. 2: organising packages and facilitating LTAs requires entry in the register, which is kept by the marshal (art. 23). Art. 22 ust. 1: regulated-activity rules apply. Art. 24 sets out the application. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 7 ust. 1–2: insolvency protection (continuation or return, refund), filing security with the marshal, a contract register in or supplied to UFG, declarations to UFG, information duties. Security takes the form of a bank or insurance guarantee, insurance, or a tourist trust account (**accepting payments only on that account, and only for services wholly in Poland**, pkt 3), plus TFG contributions (pkt 4). — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 36: TFG contribution of at most PLN 30 per traveller, due on contract or earlier payment. The actual rate sits in an implementing regulation I did not read. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 48 (organiser liable for all services, whoever performs them), art. 50 (price reduction, damages; 3-year limitation; contractual cap allowed at no less than 3× price, but not for personal injury, intent or negligence), art. 47 (traveller may withdraw any time before start, subject to a reasonable fee), art. 54 (no waiver; less favourable terms void). — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 55: fines up to PLN 50,000 or PLN 30,000, tied to the passenger-rights regulations 1177/2010 and 181/2011. Art. 59: acting without entry, including as a **board member of a company, cooperative, foundation or association** or as a person running the activity (ust. 2), carries a fine, restriction of liberty or up to 3 years' imprisonment. Art. 60: same penalty for operating without required security. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)

**Platform-relevant roles**
- Art. 4 pkt 8: an organiser is a tourism trader who **creates and sells or offers for sale** packages directly, through or together with another trader, **or** who passes traveller data under art. 5 ust. 1 pkt 2 lit. e. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 4 pkt 9: a tourist agent is a trader other than an organiser who sells or offers packages created by an organiser **under an agency contract**. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 7 ust. 4: an agent must act only for registered organisers or organisers seated in an EU or art. 8 ust. 2 state, identify the organiser in the contract, act under a valid agency contract, and meet annex information duties. Art. 22 ust. 2 names organisers and facilitators, not agents, as subject to registration. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 39 ust. 1 and art. 40 ust. 1: pre-contract information and standard form are owed by the organiser and also by the agent where sold through it. Art. 51: travellers may address complaints to the agent, which forwards them, and receipt counts as receipt by the organiser. Art. 53: organiser, facilitator or agent is liable to the traveller for its own **booking errors**. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 6 ust. 1: LTA arises if a trader facilitates (1) during one visit or contact with its point of sale, with separate selection and payment for each service, or (2) in a **targeted way**, acquiring at least one additional service from another trader, with that contract concluded within 24 hours of confirming the first. Art. 6 ust. 2: 25% carve-out. Facilitators bear art. 7 security and registration. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 10 ust. 1: if a facilitator fails art. 7 ust. 1 and 2, arts. 43 and 47–52 apply to the services in the LTA, which amounts to organiser-type liability. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Directive recital 22: a trader, typically a high-street or online travel agent, may act as retailer or intermediary only where another trader is the organiser. — [Directive 2015/2302](https://www.legislation.gov.uk/eudr/2015/2302/adopted?view=plain)

**Revised Package Travel Directive**
- Directive (EU) 2026/1024 amends 2015/2302. The Council signed off on 30 March 2026; the OJ date is 8 May 2026; it entered into force on 28 May 2026. — [European Commission, Transition Pathways](https://transition-pathways.europa.eu/tourism/legislation/revised-package-travel-directive-now-force); [Agence Europe](https://agenceurope.eu/en/bulletin/article/13827/21/european-parliament-approves-agreement-with-eu-council-on-revision-of-package-travel-directive). Sources differ on the Parliament vote date (12 or 6 March 2026).
- Transposition is due by 29 Sept 2028 and national application from 29 March 2029. LTA is removed as a separate category and folded into the package definition. The five-limb test includes single point of sale, total price, "package" advertising, post-contract choice, and linked online purchases within 24 hours. — [Garrigues](https://blogturismo.garrigues.com/en/european-union/the-european-union-discontinues-linked-travel-arrangements-and-reorganizes-the-legal-framework-governing-package-travel)
- Refunds within 6 months in most insolvency cases (9 for complex ones). Complaint acknowledgment within 7 days and full response within 60. — [Transition Pathways](https://transition-pathways.europa.eu/tourism/legislation/revised-package-travel-directive-now-force)
- Directive 2025/2647 (ADR) also amends 2015/2302. — [search summary; EAPIL](https://eapil.org/2026/01/23/directive-eu-2025-2647-amending-the-adr-framework-for-consumer-disputes/)

### Inferences
**Product classification (my reasoning; every item needs counsel)**

| Product | Likely result | Why |
|---|---|---|
| Multi-day cruise, skipper, berths aboard, one price | **Likely package** | Art. 4 pkt 1 lit. a + b, art. 5 ust. 1 pkt 2 lit. b; recital 17 distinguishes cruises from mere overnight transport. Counter-argument: art. 4 pkt 1 lit. b excludes accommodation inseparable from carriage, and participants acting as crew may not be "passengers". |
| Bareboat charter, single service | **Probably no package** (one service type) | Needs two types. Lit. c says "motor vehicles", and it is unclear whether a sailing yacht counts; no source found. |
| Charter + paid skipper, transfers, provisioning or similar | **Depends** | 25% or essential-element test (art. 5 ust. 2). |
| Course, no accommodation or transport | **No package** | One type (lit. d). |
| Course with lodging (aboard or ashore), over 24h or with overnight stay | **Likely package** | Lit. b + d. Training is plausibly over 25% of value or the essential element. The residential long-term course carve-out in recital 17 looks unlike a short sailing course. |
| Day event or regatta, under 24h, no overnight | **Outside** | Art. 3 pkt 3. |

**Platform role by design**
- **Listing a complete package created by one organiser**, with that organiser as contract party: the platform does not "create" (art. 4 pkt 8). But a commission-per-sale marketplace may be functioning as an **agent** if there is an agency contract or its substance. Art. 7 ust. 4 pkt 1 would then bar selling for unregistered organisers. Whether a platform that only connects parties is an agent is a key open issue.
- **Cart or checkout** letting a user combine a berth with a separate transfer, hotel or course from different suppliers before paying: risks art. 5 ust. 1 pkt 2 lit. a, making Nautivo a possible organiser.
- **Separate selection and payment** in one visit: risks art. 6 ust. 1 pkt 1 (LTA).
- **Targeted cross-sell after booking** (e.g. "add a transfer"): risks art. 6 ust. 1 pkt 2.
- **Passing name, email and payment details** to another trader within 24 hours: art. 5 ust. 1 pkt 2 lit. e and art. 4 pkt 8 make the **passer** an organiser. Payment details are the sensitive part of the design.
- **Nautivo-branded bundles or "Nautivo cruises" wording:** risks art. 5 ust. 1 pkt 2 lit. b–c.
- **Terms of service cannot fix this towards travellers** (art. 54).
- **Practical controls:**
  - capture and check each organiser's register entry or a declared art. 3 basis;
  - no multi-supplier cart;
  - no cross-supplier targeted upsell;
  - one organiser named on every contract.
- **Trust-account organisers** (art. 7 ust. 2 pkt 3) may not be able to take payment via a PSP marketplace flow, so the checkout must route to that account or exclude them. A 3% deduction before funds reach the trust account is a specific lawyer question.
- **From 2029 (subject to Polish transposition)**, what today is an LTA falls into the package analysis, so facilitation designs that merely tolerate LTA status should be avoided now.

### Gaps
- **No official guidance on yacht cruises, bareboat charter or courses** was found. The Ministry position is known only second-hand. No case law was found.
- Primary text of Directive 2026/1024 not read (EUR-Lex unavailable). Facilitator or retailer changes under it unverified.
- **Polish transposition status of Directive 2026/1024: nothing found.**
- Effect of the 7 Nov 2025 amendment (Dz.U. 2025 poz. 1806) was not separately analysed, though its result is in the consolidated text.
- Minimum guarantee amounts and TFG rates (implementing regulations) not read.

---

## 3. Associations: can an association organise cruises for members without registration?

### Takeaway
There is no status-based exemption for associations in the act. The only route found is art. 3 pkt 1 (occasional, non-profit, limited group), and it applies to the activity. A regular programme, or a public marketplace listing, may fall outside it. Association board members are expressly within the criminal provision.

### Cited Findings
- Art. 3 pkt 1 sets the three conditions: occasional, non-profit ("na zasadach niezarobkowych") and limited group only. The statute does not name associations or members. — [Ustawa, t.j.](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 4 pkt 7: a tourism entrepreneur may be an entrepreneur under art. 43[1] KC **or a person "prowadzący działalność odpłatną"**. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 59 ust. 1 pkt 3: criminal liability covers the management board of a foundation or association engaging in the activity without entry. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Directive recital 19: trips "not more than a few times a year by charities, sports clubs or schools for their members" exemplify the excluded case. — [Directive 2015/2302](https://www.legislation.gov.uk/eudr/2015/2302/adopted?view=plain)
- Polish Chamber of Tourism (trade body, not an authority, document dated to 2023 by metadata): associations and foundations organising non-commercial events for defined groups occasionally, "maksimum kilka razy w roku", are excluded, usually for their own members. — [PIT position](https://cmrt.com.pl/wp-content/uploads/2023/11/Jak-identyfikowac-impreze-turystyczna-Stanowisko-Rady-Polskiej-Izby-Turystyki.pdf)
- The 1997 act expressly exempted non-profit trips by associations for their members. That wording is from the old law. — [1997 act text](https://www.infor.pl/akt-prawny/DZU.1997.133.0000884,ustawa-o-uslugach-turystycznych.html)
- MSiT publishes an FAQ with entries on whether associations and unions are subject to the act and on trips for members. **The page returned 503 and only the question titles were seen in search.** — [MSiT FAQ](https://www.msit.gov.pl/pl/turystyka/prawo/7951,Najczesciej-zadawane-pytania-dot-Ustawy-o-imprezach-turystycznych-i-powiazanych-.html)

### Inferences
- Likely safer: a club runs a few cruises a year, at cost, for an existing membership.
- Likely unsafe: a programme running most of the season, a surplus or paid staff margin, or a membership obtained on booking.
- **A public Nautivo marketplace listing may undercut "limited group".** Consider restricting listings relying on art. 3 pkt 1 to closed, member-only visibility, and require a declaration from the organiser.
- Whether a 3% platform fee, or a price above cost, affects "niezarobkowo" is unresolved.

### Gaps
- **Read the MSiT FAQ answer manually** (page unavailable to me). No formal interpretation, marshal decision or judgment on sailing clubs was found.
- No definition of "occasional", "limited group" or "non-profit" was found in the statute. The "few times a year" wording comes from a recital.

---

## 4. Platform duties: DSA, Polish act, e-services act, DAC7, P2B, Omnibus

### Takeaway
Hosting duties, including notice-and-action, apply regardless of size, and the micro/small exemptions remove much of the rest while Nautivo stays small. Poland's first DSA act enters into force on **6 Nov 2026, the day after this verification**. DAC7 is the most immediate operational duty: **service sellers have no minimum threshold**, and data collection and payout holds should be designed in from the start.

### Cited Findings
**DSA (Regulation 2022/2065; unofficial mirror, verify on EUR-Lex)**
- Art. 6(1): hosting provider not liable without actual knowledge, if it acts expeditiously on gaining knowledge. Art. 6(2): not where the user acts under the provider's authority or control. **Art. 6(3): no shield under consumer law where a platform leads an average consumer to believe the item or service comes from the platform** or a user under its control. — [DSA art. 6](https://www.eu-digital-services-act.com/Digital_Services_Act_Article_6.html)
- Art. 16: notice mechanism for hosting providers, with required notice contents, receipt confirmation, decision and redress information, diligent handling. A sufficiently precise notice can give knowledge under art. 6. — [DSA art. 16](https://www.eu-digital-services-act.com/Digital_Services_Act_Article_16.html)
- Art. 15: annual transparency reports. Art. 15(2): not for micro or small enterprises (unless VLOP). — [DSA art. 15](https://www.eu-digital-services-act.com/Digital_Services_Act_Article_15.html)
- Art. 19: the online platform section, except art. 24(3), does not apply to micro or small enterprises, with a 12-month continuation after losing that status. — [DSA art. 19](https://www.eu-digital-services-act.com/Digital_Services_Act_Article_19.html)
- Art. 24(2)–(3): publication of average monthly recipients every 6 months, and provision on request to the coordinator or Commission. — [DSA art. 24](https://www.eu-digital-services-act.com/Digital_Services_Act_Article_24.html)
- Art. 29: the B2C section does not apply to micro or small enterprises, same 12-month rule. — [DSA art. 29](https://www.eu-digital-services-act.com/Digital_Services_Act_Article_29.html)
- Art. 30: if not exempt, trader traceability: name, address, phone, email, ID copy or e-ID, payment account details, trade register entry, self-certification. Best-efforts verification, correction or suspension, complaints via arts. 20–21. — [DSA art. 30](https://www.eu-digital-services-act.com/Digital_Services_Act_Article_30.html)

**Polish implementation**
- Act of 4 Sept 2026 amending ustawa o świadczeniu usług drogą elektroniczną, Dz.U. 2026 poz. 1296, published 6 Oct 2026. The President vetoed the earlier bill on 9 Jan 2026 and signed this one on 25 Sept 2026. — [Demagog](https://demagog.org.pl/na-biezaco/weto-do-ustawy-wdrazajacej-dsa-jakie-sa-argumenty-nawrockiego/); [DSK Kancelaria](https://dsk-kancelaria.pl/blog/prezydent-rp-podpisal-jedna-z-ustaw-wdrazajacych-dsa-w-polsce); [Lege Artis](https://czasopismo.legeartis.org/2026/10/koordynator-spraw-uslug-cyfrowych-urzad-komunikacji-elektronicznej-rozporzadzenie-dsa/)
- **Entry into force: 6 Nov 2026.** Art. 15b: UKE President is coordinator. Art. 22zh: complaints to the coordinator. ISAP margin note: chapter 3 of the act (arts. 12–15) is repealed from that date, so hosting liability moves to the DSA. — [Ustawa o ś.u.d.e., consolidated with margin notes](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20021441204/U/D20021204Lj.pdf)
- UOKiK supervises platforms on consumer matters. Fines up to 6% of worldwide turnover, 1% for incorrect information. — [Lege Artis](https://czasopismo.legeartis.org/2026/10/koordynator-spraw-uslug-cyfrowych-urzad-komunikacji-elektronicznej-rozporzadzenie-dsa/)
- A second bill (removal and blocking orders, druk 2693) is still in progress. Sources disagree on whether it is at the Sejm or the Senate stage. — [Lege Artis](https://czasopismo.legeartis.org/2026/10/koordynator-spraw-uslug-cyfrowych-urzad-komunikacji-elektronicznej-rozporzadzenie-dsa/); [DSK Kancelaria](https://dsk-kancelaria.pl/blog/prezydent-rp-podpisal-jedna-z-ustaw-wdrazajacych-dsa-w-polsce)

**E-services act**
- Art. 8: provider sets a regulamin and gives it free before contract. The user is not bound by provisions not made available (ust. 2). Contents (ust. 3): types and scope of services, technical requirements, ban on illegal content, conclusion and termination, complaints. Provider acts according to it (ust. 4). Art. 5: identification data, including email and permit details where required. — [Ustawa o ś.u.d.e.](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20021441204/U/D20021204Lj.pdf)

**DAC7 (ustawa o wymianie informacji podatkowych z innymi państwami, dział IIIA)**
- Art. 75a ust. 1 pkt 10: operator = a "podmiot" contracting with sellers to make a platform available. Pkt 13: "podmiot" = legal person, unit without personality, trust, or a contract such as a civil partnership. Pkt 12: platform = software connecting sellers with users for a relevant activity, **plus arrangements for collecting and paying remuneration**. — [Ustawa, t.j.](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170000648/U/D20170648Lj.pdf)
- Art. 75a ust. 2: not a platform where software, without intervening in the activity, only processes payments, only lets users list or advertise, or only redirects. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170000648/U/D20170648Lj.pdf)
- Pkt 20, relevant activities for remuneration: property, **personal services**, goods, **making available a means of transport**. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170000648/U/D20170648Lj.pdf)
- Pkt 24, excluded sellers: government bodies, listed entities and affiliates, entities with over 2,000 property transactions per property group, and sellers with **under 30 goods sales and no more than EUR 2,000**. So the threshold covers goods only. MF: services, property and transport are reportable from the first transaction. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170000648/U/D20170648Lj.pdf); [MF](https://podatki-arch.mf.gov.pl/wyjasnienia/co-warto-wiedziec-o-dyrektywie-dac7/)
- Pkt 23 and art. 75b ust. 4: **excluded operator** route, where the model excludes reportable sellers: must indicate this no later than the start of activity and by end of month following each period. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170000648/U/D20170648Lj.pdf)
- Art. 75b ust. 1: report to Szef KAS by the end of the month following the reporting year (i.e. end of January). Art. 75g: inform the seller. Art. 75j: keep records 5 years. Art. 75l: due diligence. Art. 75i: summons with 30 days to comply. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170000648/U/D20170648Lj.pdf)
- Art. 75o: 20 days to reply, then re-request. After two re-requests with no data within 60 days, **must withhold payout** (ust. 5), or block the activity if withholding is impossible (ust. 6). — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170000648/U/D20170648Lj.pdf)
- Art. 90b and art. 91: fine up to PLN 1,000,000. Art. 75u and following: registration concerns non-EU operators; a Polish resident operator's route is via reporting. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170000648/U/D20170648Lj.pdf)
- Art. 75a ust. 4: an EU operator tax resident elsewhere in the EU has no Polish duties under this chapter. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170000648/U/D20170648Lj.pdf)

**P2B Regulation 2019/1150**
- Applies when business users established in the EU offer to EU consumers (art. 1(2)). Online intermediation services are defined in art. 2(2). Terms: plain language, 15 days' notice of change (art. 3). Reasons for restriction and 30 days for termination (art. 4). Ranking parameters (art. 5). Data access (art. 9). Internal complaint system, not for small enterprises (art. 11(5)). Mediator identification, not for small enterprises (art. 12(7)). — [Regulation 2019/1150 (adopted text)](https://www.legislation.gov.uk/eur/2019/1150/adopted?view=plain)

**Omnibus: marketplace disclosure and reviews**
- Consumer act art. 2 pkt 8–9: definitions of online marketplace and provider. Art. 12a: before a consumer is bound by a distance contract, the provider must tell them (1) main ranking parameters and their importance, (2) **whether the third party is a trader, on that party's own declaration**, (3) that consumer rules do not apply if not, and (4) **how obligations are split between third party and provider**. — [Ustawa o prawach konsumenta, t.j.](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20140000827/U/D20140827Lj.pdf)
- Reviews: art. 7 pkt 25 of the unfair market practices act treats as a banned practice claiming reviews come from real users without reasonable and proportionate checks. — [Lege Artis, 2022](https://czasopismo.legeartis.org/2022/12/zamieszczanie-falszywych-opinii-konsumentow-obowiazek-weryfikacji-recenzji-klientow/) (statute not read directly)
- UOKiK enforcement: Samsung Electronics Polska was charged over review verification and unexplained "authentic" labels, with fines up to 10% of turnover. — [GSMOnline](https://gsmonline.pl/artykuly/samsung-z-zarzutami-uokik-sprawa-dotyczy-weryfikacji-opinii)
- An April 2026 press piece claims changes to burden of proof and a 12-month withdrawal right related to reviews. **Unconfirmed; do not rely on it.** — [Spider's Web](https://spidersweb.pl/2026/04/uokik-nieuczciwe-opinie-kary.html)

### Inferences
- Nautivo's marketplace probably qualifies as a DSA online platform and P2B service. SaaS-only tenants may be hosting without being an online platform, which would change which DSA section applies.
- Micro or small status should be tested at group level with any linked enterprises. Even when exempt, keep art. 30 style KYC because DAC7 and likely PSPs require similar data.
- **DAC7 mapping (needs a tax adviser):**
  - charter looks like "means of transport";
  - cruises and courses may be "personal services", though the definition refers to work by a natural person on user demand, so a packaged entity service is arguable;
  - a Nautivo booking and commission flow looks beyond art. 75a ust. 2.
- **DAC7 and the payment model interact:** art. 75o ust. 5 assumes the ability to hold payouts, so a PSP contract needs that function, or art. 75o ust. 6 blocking is the fallback. Any such hold should be reviewed against the "control over funds" question in section 1.
- If reviews are shown, restrict them to verified participants and disclose the method. Reviews tied to a completed booking are an obvious mechanism.

### Gaps
- DSA arts. 11–14, 17–18, 20–23, 25–28, 31–32 were **not re-read** in this session. From memory, arts. 11–14 and 17–18 apply to all relevant providers. Verify.
- Recommendation 2003/361 thresholds (commonly cited as under 10 staff and EUR 2m for micro, under 50 and EUR 10m for small) were not verified here.
- Polish unfair practices act text not read (ISAP verification wall), including any art. 6 duty to disclose how reviews are checked.
- Podatki.gov.pl current DAC7 page returned 404; only the archived MF explainer of 25.04.2025 was read. Current XML schema and any B2B portal registration procedure not verified.
- Art. 75c seller data fields not listed here; read directly.

---

## 5. Consumer law: withdrawal, pre-contract information, who is the seller, ODR/ADR

### Takeaway
Dated leisure services are exempt from the 14-day distance withdrawal right, under **art. 38 ust. 1 pkt 12** of the consumer act. That does not help for packages, where the tourism act governs and gives a different, any-time withdrawal right with a fee. The ODR platform is closed, so remove any link, and ADR is being reformed.

### Cited Findings
- Art. 38 ust. 1 pkt 12: no withdrawal right for contracts for services of accommodation other than residential, carriage of goods, car rental, catering, "usług związanych z wypoczynkiem, wydarzeniami rozrywkowymi, sportowymi lub kulturalnymi, jeżeli w umowie oznaczono dzień lub okres świadczenia usługi". — [Ustawa o prawach konsumenta, t.j.](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20140000827/U/D20140827Lj.pdf)
- Art. 3 ust. 1 pkt 8: package contracts are outside the consumer act, except art. 10, art. 11, art. 12 ust. 1 pkt 1, 5, 16 and 17, art. 17 and art. 20 ust. 2. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20140000827/U/D20140827Lj.pdf)
- Tourism act art. 47 ust. 1–2: traveller may withdraw any time before start, subject to a reasonable fee. — [Ustawa o imprezach, t.j.](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Art. 12 ust. 1: pre-contract information from the trader, including main characteristics and trader identity (pkt 1–3). Art. 17: distance order button rules. Art. 12 ust. 1 pkt 9–11 and art. 13: withdrawal information and model form. Art. 27: 14-day withdrawal. Art. 7: no waiver. — [Ustawa o prawach konsumenta, t.j.](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20140000827/U/D20140827Lj.pdf)
- Art. 7aa: chapters 4, 5a and 5b consumer provisions extend to a sole trader whose contract is not of a professional nature for them. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20140000827/U/D20140827Lj.pdf)
- Dz.U. 2026 poz. 1353 adds art. 2 pkt 10–14 and other changes from 27.09.2027 (e.g. durability guarantee, art. 7ac). I did not analyse these. — [same](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20140000827/U/D20140827Lj.pdf)
- ODR: Regulation 2024/3228 repealed the ODR Regulation from 20 July 2025, and traders had to remove references by then. — [Prawo.pl, citing UOKiK](https://www.prawo.pl/biznes/uokik-e-sklepy-musza-zmienic-zapisy-o-platformie-odr,531199.html)
- ADR: Directive (EU) 2025/2647 amends 2013/11/EU, OJ 30 Dec 2025, in force 19 Jan 2026. **Sources conflict on timing: transposition by 20 Mar 2028, or application from 20 Sept 2028.** — [EAPIL](https://eapil.org/2026/01/23/directive-eu-2025-2647-amending-the-adr-framework-for-consumer-disputes/); [Digital Policy Alert](https://digitalpolicyalert.org/change/7397)

### Inferences
- Art. 38 ust. 1 pkt 12 likely covers dated cruises, charters (as leisure services) and scheduled courses that are not packages. It likely does not cover undated vouchers or open-date courses. The fit for charter and training, as opposed to pure leisure, has no authority I found.
- The withdrawal exemption is not a "no refunds" rule, and organisers' cancellation terms must separately respect the tourism act where it applies.
- **Who is the seller:** structure the organiser as the contract party and trader, and Nautivo as provider under art. 12a. Art. 12a pkt 4 requires disclosing the division of duties. Nautivo branding, single-brand checkout and confirmations risk the DSA art. 6(3) "average consumer" issue, which is the point where a marketplace can lose the shield. Nautivo's own contract with consumers (account, marketplace) is a separate contract with its own information duties.
- Non-trader sellers, such as individual skippers or some associations, trigger art. 12a pkt 2–3 disclosure, and consumer rights would then not apply against them.
- **Commission presentation:** if the 3% is charged on top rather than deducted from the organiser's price, total-price presentation needs review (art. 12 ust. 1 pkt 5 applies even to packages by virtue of art. 3).

### Gaps
- Polish ADR act (ustawa z 23.09.2016) text not read (my download failed), so the trader information duty on ADR and the specific article numbers are unverified.
- CJEU case law on intermediary as seller (e.g. Wathelet C-149/15) could not be verified. Only Airbnb Ireland C-390/18 was surfaced: an intermediation platform may be an information society service. — [SCL](https://www.scl.org/10767-airbnb-does-not-require-an-estate-agent-s-licence-says-cjeu/) (search summary). Counsel should confirm relevance, including Uber-line "composite service" arguments.
- No official interpretation was found on the art. 38 fit for sailing.

---

## 6. GDPR: controller/processor split, health data, minors, logs

### Takeaway
The intended split (organiser = controller for its participant data in the SaaS, Nautivo = processor, Nautivo = controller for marketplace accounts) is a coherent starting point, but the roles depend on what actually happens. Any Nautivo use of tenant data for its own purposes moves it to controller for that use, and shared marketplace flows carry joint-controllership risk. No UODO guidance specific to this model was found.

### Cited Findings
- GDPR art. 28(1): use only processors with sufficient guarantees. Art. 28(2): no further processor without specific or general written authorisation. Art. 28(3): binding contract. Art. 28(10): a processor who determines purposes and means "shall be considered to be a controller in respect of that processing". — [GDPR art. 28 (adopted text)](https://www.legislation.gov.uk/eur/2016/679/article/28/adopted)
- EDPB Guidelines 07/2020 (v2.1, 7 July 2021) treat roles as functional concepts. Joint controllership doesn't require direct data access. Several actors do not automatically mean joint control. Art. 26 arrangements should be in a binding document, and joint responsibility is not equal responsibility. — [Hunton summary](https://www.hunton.com/privacy-and-cybersecurity-law-blog/edpb-publishes-guidelines-on-the-concepts-of-controller-and-processor-in-the-gdpr), [Guidelines (PL)](https://www.edpb.europa.eu:443/system/files/2023-10/edpb_guidelines_202007_controllerprocessor_final_pl.pdf) (full text not opened)
- EDPB Opinion 22/2024 on obligations when relying on processors and sub-processors. EDPB page also notes Joint Opinion 01/2025 on the art. 30(5) SME record-keeping proposal. — [EDPB](https://www.edpb.europa.eu/topics/key-gdpr-concepts/controllerprocessor_pl)
- Health data are a special category under art. 9 and processing is prohibited unless an art. 9(2) condition applies, such as explicit consent. — [Kluczesoft commentary on art. 9](https://kluczesoft.pl/wiedza/bezpieczenstwo/art-9-rodo) (secondary, search excerpt; check against the GDPR text)
- UODO has guidance on minors' data in the context of child-protection standards, including that parental consent for images must be specific and time-limited. — [Poradnik Przedsiębiorcy on UODO guidance](https://poradnikprzedsiebiorcy.pl/-standardy-ochrony-maloletnich-a-rodo-wytyczne-i-zalecenia-uodo) (secondary)

### Inferences
| Processing | Probable role of Nautivo | Note |
|---|---|---|
| Organiser's participant list, crew forms, medical notes, in SaaS | Processor | Art. 28 contract, sub-processor list. |
| Marketplace account, search, booking, reviews | Controller | Own purposes. |
| Passing booking data to organiser | Separate controllers (Nautivo to organiser) | Needs privacy notice. |
| DAC7, tax, fraud | Controller | Legal obligation; organiser-sellers are the data subjects. |
| Global analytics or matching across tenants, marketing to tenant's participants | Controller (art. 28(10) risk if undisclosed) | Avoid or separate. |
| Joint promotion, shared participant profile, shared rankings | Possible joint controllers | Art. 26 arrangement. |
| Audit logs in SaaS | Mixed | Security and accountability logs may have Nautivo as controller for its own security purposes; contents of tenant data remain processor. To be decided by counsel. |

- Health data on crew forms should be organiser-only and minimised (e.g. "fit to sail" and emergency notes), with restricted access, encryption and deletion soon after the event. Marketplace should not collect it.
- Children: a parent or guardian account and consent flow, restricted image use, and an organiser-side duty to check. Nautivo should not make minors' profiles publicly visible.
- Retention needs a schedule by data category, with DAC7's 5-year record duty as one input.

### Gaps
- **Not verified in this session (from memory, do not cite without checking):** art. 4(7)–(8) definitions; arts. 5(1)(e), 6, 8, 13–14, 30, 32, 35, 37; the Polish age for art. 8 consent (commonly stated as 16); CJEU Wirtschaftsakademie (C-210/16) and Fashion ID (C-40/17).
- **No UODO guidance on log retention, SaaS booking platforms, or health data for organisers of youth trips was found.** Direct review of uodo.gov.pl is required.
- Child-protection law (Kamilek law) and the rules on youth holiday organisation, which may require data on minors, were not researched.

---

## 7. Liability, insurance, and the platform-versus-organiser split

### Takeaway
Package liability falls on the organiser (art. 48) and cannot be contractually waived towards travellers. The platform's own exposure comes from role reclassification, booking errors, information duties and unauthorised payment activity. **Acting as an unregistered individual leaves those exposures personal, and several are criminal. This is a lawyer question.**

### Cited Findings
- Tourism act art. 48, art. 50, art. 53, art. 54 as summarised in section 2. — [Ustawa, t.j.](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)
- Personal criminal exposure is written into the text of art. 59 ust. 2 and art. 60 of the tourism act, and art. 150 ust. 1 and 3 of the payment act. Art. 4 pkt 7 of the tourism act covers a person "prowadzący działalność odpłatną". — [Tourism act](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf); [Payment act](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20111991175/U/D20111175Lj.pdf)
- DAC7 art. 75a ust. 1 pkt 10 and 13 define operator by reference to "podmiot", which on its text lists legal persons, units and contracts, and does not name natural persons. — [Ustawa, t.j.](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170000648/U/D20170648Lj.pdf)
- Art. 7 tourism security is insolvency protection, not third-party liability insurance. — [Ustawa, t.j.](https://isap.sejm.gov.pl/isap.nsf/download.xsp/WDU20170002361/U/D20172361Lj.pdf)

### Inferences
**Split of duties (working matrix, assumes a marketplace listing packages created by one organiser and a PSP holding funds)**

| Topic | Organiser | Nautivo |
|---|---|---|
| Package: register, security, TFG, contract, performance | Yes (art. 7, 22, 48, 50) | No, unless it creates or combines (art. 4 pkt 8, art. 5) |
| Selling for organiser | Names agent; forwards duties | If agent: art. 7 ust. 4, 39, 40, 51, 53 |
| LTA | Possibly | If facilitating: art. 6, 7, 10, 22 |
| Payment | Payee, PSP's customer | No funds control; else art. 3, 4, 150 |
| Consumer info | Art. 12 or tourism act art. 39–40 | Art. 12a; art. 12 for own services |
| Withdrawal, cancellation | Own terms within the law | Shows terms; handles nothing it can't perform |
| DSA, P2B, e-services | Trader's self-certification, accuracy | Art. 16 etc; art. 5, 8 ś.u.d.e.; P2B |
| DAC7 | Provides data | Reporting operator (art. 75a–75o, 90b) |
| Reviews | Doesn't fake them | Art. 7 pkt 25 u.p.n.p.r. checks |
| GDPR | Controller of participant data | Processor in SaaS; controller for marketplace |

**Insurance (no statutory platform requirement was found; market check required):** consider professional liability/tech E&O, cyber, and D&O once incorporated. Require organisers to evidence their own cover in onboarding, and check whether exclusions touch tourism-law reclassification.

### Gaps: questions for the lawyer
1. **Personal liability before incorporation:** can the founder run this as an unregistered individual (działalność nierejestrowana)? Regulated activity seems to rule it out if any regulated role applies. Exposure under art. 59–60 tourism act, art. 150 payment act, DAC7 art. 75a and 90b. Recommend the legal form (sp. z o.o. vs. others) and board liability, including whether criminal exposure follows the board member anyway. Prawo przedsiębiorców and KSH were not researched.
2. **Payments:** does the chosen PSP flow keep Nautivo outside art. 3? Is a KNF inquiry advisable? Effect of art. 2(2)(b) PSR.
3. **Agent status:** does a 3% marketplace need an agency contract, and is it a tourist agent under art. 4 pkt 9? What follows if an organiser is unregistered?
4. **Sailing classification:** cruise, charter, course, whether a skipper-plus-crew arrangement counts as carriage; consider a written request to MSiT or a marshal, or a tax-style interpretation route if available.
5. **Organiser or LTA tests** on the actual checkout and any upsell (art. 5 ust. 1 pkt 2 lit. a, e; art. 6) and readiness for Directive 2026/1024.
6. **Trust-account organisers** and the 3% deduction (art. 7 ust. 2 pkt 3, art. 11).
7. **Associations:** art. 3 pkt 1 boundaries; effect of public listing; board exposure.
8. **DAC7:** "personal service" and "means of transport" mapping; first report date; payout-hold function.
9. **DSA:** micro or small status; online platform vs hosting split of SaaS and marketplace.
10. **Art. 38 ust. 1 pkt 12** fit, cancellation clause validity, and terms review (including abusive clauses, not researched).
11. **GDPR:** role map, art. 28 contract, DPIA need, minors and health data, log retention.
12. **Liability:** civil code tort and contract exposure for accidents on listed cruises, limits in terms, insurance. Maritime and inland navigation rules, skipper qualification and yacht safety rules are outside this research; a verified source was not found.
13. **VAT, including margin scheme for tourism and tax treatment of the commission**, was not in scope.
