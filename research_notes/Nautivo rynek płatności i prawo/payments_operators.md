# Multi-seller payment operators in Poland for Nautivo

**Verification date: 2026-11-05. Currency: PLN.** Source quality tags: no tag means the page or PDF was read directly. **[snippet]** means it was seen only in a search-result summary, so re-check it before relying on it. Prices are as published; none of the sources reviewed says whether they include VAT (requires asking operator's sales). Legal conclusions are for a lawyer or accountant to confirm.

**Could not be read directly:** autopay.pl (HTTP 403), developers.przelewy24.pl (JavaScript-only), the full PayU price list, the PayU merchant agreement, Tpay's card annex, and the second half of the Stripe Services Agreement (only the first 100,000 of 242,177 characters).

## Availability, methods, flows and how the 3% is collected

### Takeaway
Stripe Connect is the only option here with full public self-serve documentation and public Connect prices, and it supports BLIK, P24 and cards with an application fee. Tpay, PayU, P24, Paynow and Autopay all have documented marketplace products, but access is gated or negotiated. Mangopay and Adyen are possible but contract-led, and Lemonway looks unsuitable. Important risk: Stripe's P24 rules prohibit several categories close to Nautivo's business.

### Cited Findings

**Stripe Connect**
- BLIK and P24 are both available to PL businesses and to Connect platforms: [BLIK](https://docs.stripe.com/payments/blik), [P24](https://docs.stripe.com/payments/p24). PL connected accounts list the `blik_payments`, `p24_payments`, `card_payments` and `transfers` capabilities: [Stripe selector for PL](https://docs.stripe.com/_endpoint/get-requirement-selections-for-platform-country?platformCountry=PL).
- There are three charge types. Direct charges sit on the connected account, and "Refunds and chargebacks reduce the connected account's balance". Destination charges and separate charges and transfers sit on the platform, whose balance takes refunds and chargebacks: [Connect charges](https://docs.stripe.com/connect/charges).
- Commission is set with `application_fee_amount` or `transfer_data[amount]`. A worked split, reporting via the Application Fee object, and the `on_behalf_of` option are documented: [destination charges](https://docs.stripe.com/connect/destination-charges.md?platform=web&ui=stripe-hosted).
- **P24 on Stripe prohibits these MCCs:** 4457 Boat Rentals and Leases, 4722 Travel Agencies/Tour Operators, 4411 Cruise Lines, 7033 Trailer Parks/Campgrounds, 5262 Marketplaces, 8398 Charitable fundraising organisations. Restricted MCCs include 7941 Sports Clubs/Fields, 7991 Tourist Attractions, 8299 Educational Services, 5551 Boat Dealers. Also prohibited by category: "Entertainment and event promotion services" and "Higher education and vocational training services". P24 also requires the website to show address, tax ID, registration number and refund policy: [Stripe P24](https://docs.stripe.com/payments/p24).
- BLIK on Stripe has a minimum of 2.00 PLN and a limit of 2,000 PLN per off-session recurring charge. Its documentation lists no MCC table: [BLIK](https://docs.stripe.com/payments/blik).
- Stripe's general list marks "Travel reservation services and clubs" as restricted. It also restricts payment facilitation, including receiving settlement proceeds on behalf of third-party sellers. Charitable fundraising is supported only in AU, CA, UK and US: [restricted businesses](https://stripe.com/legal/restricted-businesses).

**Przelewy24 marketplace**
- One cart is split between the platform owner and multiple registered P24 accounts; commission is "customised". Two models: commission on the whole cart from the owner's technical account, or commission on each seller's share. Other models go through sales: [P24 marketplace](https://www.przelewy24.pl/en/payment-solutions/marketplace).
- A separate [Marketplace REST documentation](https://developers.przelewy24.pl/marketplace/index.php?pl=) exists, but its contents could not be read. A `business_type` value 9 for association/foundation/public-benefit organisation was seen **[snippet]**.

**Tpay Marketplace**
- A parent transaction has child transactions per `merchant.id`, and the children must sum to the total: [Tpay marketplace API](https://docs-api.tpay.com/pl/marketplace/).
- Commission "automatycznie trafia do Ciebie". Listed payment methods are BLIK, transfer, instalments and deferred payment, with **no mention of cards**: [Tpay marketplace](https://tpay.com/dla-biznesu/marketplace). Only methods shared by all sellers in a cart are shown, and the documented list is BLIK, Pay By Link, Pekao/Alior instalments and Twisto **[snippet]**: [Tpay configuration](https://support.tpay.com/sprzedawca/platnosci-marketplace-instrukcja-konfiguracji).
- Partner Program membership and a signed agreement are required: [Tpay partner guide](https://support.tpay.com/sprzedawca/marketplace-instrukcja-dla-partnera).

**PayU Marketplace**
- Single cart with automatic split inside PayU, "you won't have to handle your submerchant's funds": [PayU marketplace docs](https://developers.payu.com/en/marketplace.html).
- `shoppingCarts[]` carries `extCustomerId` and a `fee` field per seller. Payouts run from a per-seller balance: [PayU integration](https://developers.payu.com/europe/docs/services/marketplace/integration/).
- PayU also has a separate Payment Facilitator model, in which the facilitator takes on much of PayU's responsibilities: [PayU PF](https://developers.payu.com/europe/docs/collaboration/payment-facilitator/).

**Paynow (mBank)**
- `transfers[]` carries `sellerId`, `grossAmount` and `feeAmount`; "feeAmount is deducted from the seller transfer". Support must activate the feature: [Paynow marketplace](https://docs.paynow.pl/docs/v3/marketplace).
- Marketplace and multipayout models are outside the standard list and are discussed under Enterprise terms (monthly turnover above 100,000 PLN): [Paynow prices](https://www.paynow.pl/cennik).

**Autopay**
- Basket payments use balance points; settlement is D+1 or on demand via `balancePayoff`. Under the multi-invoice models, commissions must be in the regulations and "mogą być wyłącznie procentowe" (which fits a flat 3%): [Autopay marketplace](https://developers.autopay.pl/online/marketplace).
- Functionality depends on individual arrangements with Autopay **[snippet]**: [Autopay gateway documentation](https://developers.autopay.pl/pdf?documentId=384).

**Other options**
- **Adyen:** split at authorization or capture **[snippet]**: [split payments](https://docs.adyen.com/marketplaces/split-transactions/split-payments-at-authorization). Poland appears on the hosted onboarding list, on pages that may be dated **[snippet]**: [onboarding](https://docs.adyen.com/platforms/onboard-users). BLIK is listed for PL/PLN **[snippet]**: [Adyen BLIK](https://docs.adyen.com/payment-methods/blik).
- **Mangopay:** BLIK is listed for Poland; P24 is not on the guide; "Payment methods require contract amendments and activation in all cases": [Mangopay methods](https://docs.mangopay.com/guides/payment-methods).
- **Lemonway:** PLN is mentioned only for bank transfers, on an older page **[snippet]**: [Lemonway methods](https://support.lemonway.com/hc/en-us/articles/360015763337-What-are-the-Payment-methods-and-currencies-supported-by-Lemonway). No BLIK or P24 evidence was found.

### Inferences
- If Nautivo or its organisers are classified as boat rental, tour operator or event promotion, P24 inside Stripe may be unavailable. Plan for BLIK plus cards only on Stripe until Stripe confirms the MCC.
- Direct charges with `application_fee_amount` map most simply to "3% of each actual payment". Each deposit, instalment or final payment is its own charge with its own fee.
- Lemonway should be dropped. Mangopay and Adyen are realistic only after a sales conversation, which is poor fit for a one-person launch.

### Gaps
- Whether Tpay Marketplace can accept cards: requires asking operator's sales.
- Whether P24's own marketplace (not via Stripe) has similar MCC exclusions for boat or travel organisers: requires asking operator's sales.
- Exact Stripe MCC for Nautivo-style organisers: requires asking Stripe.

## Fees and worked examples

### Takeaway
Only Stripe publishes Connect platform fees. For every Polish operator the marketplace price is non-public, so the figures below use standard merchant prices as an illustration. If the platform absorbs fees, 3% is thin at 100 PLN on Stripe and comfortable at larger amounts; if the organiser pays, the platform keeps the full 3%.

### Cited Findings

**Published prices**

| Operator / plan | BLIK | Transfers / P24 | Cards | Other | Source |
|---|---|---|---|---|---|
| Stripe PL | 1.6% + 1.00 | P24 1.9% + 1.00 | EEA standard 1.5% + 1.00; EEA premium 1.9% + 1.00; international 3.25% + 1.00 | Dispute 90 PLN; +2% if conversion | [Stripe pricing](https://stripe.com/pl/pricing) |
| Przelewy24 | 1.29% + 0.30 | 1.29% + 0.30 | 1.29% + 0.30 (Visa/MC 3DS) | Activation 59 PLN, refundable after 10 transactions in 2 months; 4 free payouts a month, then 0.99 | [P24 fees](https://www.przelewy24.pl/en/offer/commissions-and-fees) |
| Tpay Starter | 1.59% + 0.41 | 1.59% + 0.39 | 1.59% + 0.39 (see conflict below) | Activation 99; refund 1; payout 2; chargeback 120 | [Tpay offer](https://tpay.com/oferta) |
| Tpay Business | 0.99% + 0.02 | 0.99% | 0.99% (see conflict below) | 99 PLN/month, only in months with transactions; activation 1; chargeback 120 | [Tpay offer](https://tpay.com/oferta) |
| PayU | 1.10% + 0.32 (promotional), 1.49% + 0.32 (after promotion) | 1.10% or 1.49% + 0.30 | 1.10% or 1.49% + 0.30 | Activation 29 PLN (standard 199) | [PayU offer](https://poland.payu.com/oferta-handlowa/), [PayU T&C](https://poland.payu.com/wp-content/uploads/sites/14/2026/05/Terms-and-conditions.pdf) |
| Paynow | from 0.95% | from 0.95% | from 0.95% | No fixed, registration or subscription fees; chargeback fee amount not stated; mBank business account required | [Paynow prices](https://www.paynow.pl/cennik) |
| Autopay Standard | 1.25% + 0.34 **[snippet]** | 1.25% + 0.34 **[snippet]** | 1.25% + 0.34 **[snippet]** | Other Autopay pages show 1.19% + 0.34 and 1.19%; re-check | [Autopay offer](https://autopay.pl/oferta/platnosci-online) |

**Conflicts in the sources**
- **Tpay cards:** [tpay.com/oferta](https://tpay.com/oferta) shows the rates in the table, while the [2026 packages PDF](https://tpay.com/user/assets/files_for_download/pakiety-serwisowe-2026.pdf) says cards are "warunki ustalane indywidualnie". The PDF's table text extracted in a garbled layout, so its BLIK row could not be confirmed against the 0.41 PLN fixed fee on the offer page. Another Tpay PDF shows BLIK at 1.78% + 0.39 for new clients **[snippet]**: [Tpay offer PDF](https://tpay.com/user/assets/files_for_download/tpay-oferta.pdf). Confirm with Tpay.
- **PayU:** the offer in the T&C PDF states validity from 4 Sep 2025 to 31 May 2026, and its text mixes "first six months" with "after the first three months". The offer page still shows 1.1% and 1.49% without explaining the second figure. The full standard list was not read.

**Stripe Connect platform fees** ([Connect pricing](https://stripe.com/pl/connect/pricing))
- "Stripe handles pricing": "No fees for your platform"; no account or payout fees.
- "You handle pricing": 9 PLN per monthly active account (one that receives a payout in that month); 0.25% + 1.35 PLN per payout; a row for funds routing at 0.25% of payout volume; account debits 1.4%.
- Both: Instant Payouts 1%.
- Fees for refunds: original processing, Connect and conversion fees are not returned: [Stripe pricing](https://stripe.com/pl/pricing).
- "Stripe charges the platform directly for destination charges": [fee behaviour](https://docs.stripe.com/connect/direct-charges-fee-payer-behavior).

**Marketplace surcharges:** none are public for P24, Tpay, PayU, Paynow or Autopay. Requires asking operator's sales. Adyen's page shows "$0.13 + 1.5%" for BLIK and a minimum invoice depending on industry **[snippet]**: [Adyen pricing](https://www.adyen.com/pricing).

### Inferences

**Assumptions:** single transaction; standard list prices apply (marketplace prices may differ); no conversion; EEA standard card unless stated; no VAT adjustment; fees rounded to the grosz. Formula: fee = amount × % + fixed. 3% of 100 / 800 / 3000 = 3.00 / 24.00 / 90.00.

**Table 1: what remains of the 3%**
- "Platform absorbs" = 3% minus operator fee.
- "Organiser pays" = the platform keeps the full 3.00 / 24.00 / 90.00, and the organiser receives amount minus 3% minus operator fee.

| Rate | Fee at 100 / 800 / 3000 | Platform absorbs: left of 3% | Organiser pays: organiser receives |
|---|---|---|---|
| Stripe BLIK 1.6% + 1.00 | 2.60 / 13.80 / 49.00 | 0.40 / 10.20 / 41.00 | 94.40 / 762.20 / 2861.00 |
| Stripe card standard 1.5% + 1.00 | 2.50 / 13.00 / 46.00 | 0.50 / 11.00 / 44.00 | 94.50 / 763.00 / 2864.00 |
| Stripe card premium or P24 1.9% + 1.00 | 2.90 / 16.20 / 58.00 | 0.10 / 7.80 / 32.00 | 94.10 / 759.80 / 2852.00 |
| P24 1.29% + 0.30 | 1.59 / 10.62 / 39.00 | 1.41 / 13.38 / 51.00 | 95.41 / 765.38 / 2871.00 |
| Tpay Starter BLIK 1.59% + 0.41 | 2.00 / 13.13 / 48.11 | 1.00 / 10.87 / 41.89 | 95.00 / 762.87 / 2861.89 |
| Tpay Business BLIK 0.99% + 0.02 | 1.01 / 7.94 / 29.72 | 1.99 / 16.06 / 60.28 | 95.99 / 768.06 / 2880.28 |
| PayU BLIK 1.49% + 0.32 | 1.81 / 12.24 / 45.02 | 1.19 / 11.76 / 44.98 | 95.19 / 763.76 / 2864.98 |
| PayU BLIK promotional 1.10% + 0.32 | 1.42 / 9.12 / 33.32 | 1.58 / 14.88 / 56.68 | 95.58 / 766.88 / 2876.68 |
| Paynow 0.95% | 0.95 / 7.60 / 28.50 | 2.05 / 16.40 / 61.50 | 96.05 / 768.40 / 2881.50 |
| Autopay 1.25% + 0.34 | 1.59 / 10.34 / 37.84 | 1.41 / 13.66 / 52.16 | 95.41 / 765.66 / 2872.16 |

Example, Stripe BLIK at 800: 800 × 1.6% = 12.80; + 1.00 = 13.80; 24.00 − 13.80 = 10.20; organiser 800 − 24.00 − 13.80 = 762.20.

**Table 2: Stripe when the platform absorbs (destination charges, "you handle pricing")**

Extra assumptions: one payout per payment (worst case); 0.25% + 1.35 computed on the gross amount (a slight overestimate, as the payout is net of commission); the funds routing row is treated as the same 0.25%, not additive (unclear, requires asking Stripe); 9 PLN per active account per month excluded.

| Method | Payout fee at 100 / 800 / 3000 | Left of 3% |
|---|---|---|
| BLIK | 1.60 / 3.35 / 8.85 | 0.40 − 1.60 = **−1.20**; 10.20 − 3.35 = **6.85**; 41.00 − 8.85 = **32.15** |
| Card standard | 1.60 / 3.35 / 8.85 | **−1.10** / **7.65** / **35.15** |
| Card premium or P24 | 1.60 / 3.35 / 8.85 | **−1.50** / **4.45** / **23.15** |

Batching payouts reduces the 1.35 share. Then subtract 9 PLN per organiser paid out in that month.

**Other calculations**
- **Break-even** when absorbing (amount where 3% equals fee = fixed ÷ (3% − rate)): Stripe BLIK 71.43 PLN; Stripe premium/P24 90.91 PLN; P24 17.54 PLN; Tpay Starter BLIK 29.08 PLN. All before Connect fees.
- **Split payments:** 3000 PLN paid as 800 + 2200 via Stripe BLIK costs 13.80 + 36.20 = 50.00, against 49.00 in one payment. Each extra instalment adds a fixed fee.
- **Refund, Stripe BLIK 800 PLN, full refund:** the 13.80 fee is not returned. If the platform absorbs and refunds its 3%, the platform loses 13.80. If the platform absorbs and keeps its 3%, it nets 10.20 and the organiser bears the 24.00. If the organiser pays and the 3% is refunded, the organiser loses 13.80.
- **Lost card dispute, Stripe, 800 PLN:** 800 + 90 dispute fee.
- Cheapest public rates are not necessarily cheapest in marketplace form, so the tables should not be used to rank operators until quotes are obtained.
- The organiser-pays model protects the 3% and removes the small-payment loss. It suits the plan of 3% as net revenue, and the commission could be disclosed to organisers separately from the operator fee.

### Gaps
- Marketplace commission, active-seller, payout and volume fees for P24, Tpay, PayU, Paynow, Autopay: requires asking operator's sales.
- Mangopay and Adyen prices in PLN: requires asking operator's sales.
- Minimum volumes: none found published for P24, Tpay, PayU or Autopay marketplace; Paynow discussions start above 100,000 PLN a month. Requires asking operator's sales.

## Requirements for the platform

### Takeaway
Assume the platform needs a registered business or legal entity. No operator was found offering marketplace access to a person on działalność nierejestrowana. Autopay accepts nierejestrowana as an ordinary merchant, not as a confirmed marketplace operator, and without cards.

### Cited Findings
- **Stripe:** "Only businesses (including sole proprietors) and non-profit organisations located in Poland are eligible to apply"; use for "personal, family or household purposes" is barred: [Stripe SSA PL](https://stripe.com/legal/ssa/pl). Nothing read names działalność nierejestrowana.
- **Tpay:** an Akceptant is a natural person, civil partnership, legal person or organisational unit "będąca przedsiębiorcą i prowadząca działalność gospodarczą, inny podmiot niebędący osobą fizyczną, posiadający zdolność prawną i nieprowadzący działalności gospodarczej oraz osoby fizyczne prowadzące działalność rolniczą": [Tpay OWU](https://tpay.com/tpay-owu-2). Tpay says it works with active, registered entrepreneurs and has no offer for natural persons without a business **[snippet]**: [Tpay on identity documents](https://tpay.com/blog/dowod-tozsamosci-dlaczego-operator-platnosci-go-wymaga). Marketplace requires the Partner Program and "musisz podpisać odpowiednią umowę": [Tpay partner guide](https://support.tpay.com/sprzedawca/marketplace-instrukcja-dla-partnera).
- **PayU:** "A Merchant can be an adult natural person conducting business activity, a legal person, or an organizational unit without legal personality, to which the law grants legal capacity": [PayU T&C](https://poland.payu.com/wp-content/uploads/sites/14/2026/05/Terms-and-conditions.pdf). Activation fee, online agreements and a short form: [PayU marketplace](https://poland.payu.com/our-solutions/marketplace/). Register page does not mention nierejestrowana: [PayU register](https://developers.payu.com/europe/docs/get-started/set-up-account/register/).
- **P24:** the registration page says it is for companies and people without a registered business should not continue **[snippet]**: [P24 registration](https://registration.przelewy24.pl/new-account/0444ae7a-9865-4c69-9882-8893fd10b30c). The Polska Bezgotówkowa programme excludes nierejestrowana **[snippet]**: [P24 programme](https://www.przelewy24.pl/oferta/polska-bezgotowkowa). OWU version 1.1 (from 23 April 2025) lets the Akceptant declare business, public or statutory activity, "np. jako fundacja lub stowarzyszenie": [P24 OWU](https://www.przelewy24.pl/storage/app/media/owu/przelewy24/owu-przelewy24-v.1.1.pdf).
- **Autopay:** nierejestrowana can use online payments with 199 PLN activation, a 199 PLN non-refundable verification fee (the page may be describing one fee twice), 1.19% + 0.34, and no cards or wallets: "niemożliwe jest aktywowanie płatności kartą". Limit "75% minimalnego wynagrodzenia" monthly: [Autopay nierejestrowana](https://developers.autopay.pl/online/dzialalnosc-nierejestrowana). Another Autopay page refers to a quarterly limit, which conflicts: [Autopay who can use](https://developers.autopay.pl/online/faq/o-usludze/ktore-dzialalnosnosci-rejestrowane-moga-korzystac-z-platnosci-online).
- **Paynow:** requires an mBank business account and the application from the account holder: [Paynow prices](https://www.paynow.pl/cennik).
- **Website preconditions:** Autopay requires terms with real name, address and NIP, privacy policy, delivery/return/complaint rules, and at least five sample products or services (fewer if the seller only has one or two): [Autopay who can use](https://developers.autopay.pl/online/faq/o-usludze/ktore-dzialalnosnosci-rejestrowane-moga-korzystac-z-platnosci-online). PayU onboarding also needs an active website meeting its requirements **[snippet]**: [PayU offer](https://poland.payu.com/oferta-handlowa/).

### Inferences
- Plan on at least JDG (registered sole trader) or sp. z o.o. before any marketplace contract. Stripe's wording supports sole proprietors; for the others a legal entity is the safer reading.
- PayU's "natural person conducting business activity" could arguably cover nierejestrowana, but that is an untested reading. Do not rely on it.
- A nierejestrowana founder cannot accept cards through Autopay, and any revenue limit would need checking against how platform turnover is counted (accountant to confirm).

### Gaps
- Explicit answer on whether nierejestrowana can be a marketplace platform at each operator: not publicly stated, requires asking operator's sales.

## Requirements for sellers (organisers)

### Takeaway
Companies, sole traders and associations are all onboardable at most operators. The best public evidence for associations without business activity comes from Tpay's definition, P24's document list and Stripe's non-profit entity types. Individuals without a business are the weak spot.

### Cited Findings
- **Stripe:** PL entity types are `individual`, `company` and `non_profit` (`incorporated_non_profit`, `unincorporated_non_profit`): [Stripe selector](https://docs.stripe.com/_endpoint/get-requirement-selections-for-platform-country?platformCountry=PL). For PL non_profit (v1, Express, cards and transfers), required items are name, tax ID field (REGON in v2), address, representative details and DOB, directors, MCC, URL, product description, ToS acceptance and bank account. Proof of registration is an alternative path: [Stripe requirements query](https://docs.stripe.com/_endpoint/get-requirements-for-setups?account-setup-A[apiVersion]=v1&account-setup-A[platformCountry]=PL&account-setup-A[accountCountry]=PL&account-setup-A[dashboardType]=express&account-setup-A[tosType]=full&account-setup-A[legalEntityType]=non_profit&account-setup-A[capabilities][0]=card_payments&account-setup-A[capabilities][1]=transfers). PL only offers the `full` service agreement, not `recipient`.
- **P24 document list:**
  - JDG: owner ID.
  - Sp. z o.o.: representatives' ID, beneficial owners' PEP declarations, registry documents of supervising companies if applicable.
  - Foundation/association with KRS: IDs, PEP declarations, statute.
  - "Organizations, clubs and associations without KRS": the same plus "potwierdzenie powołania reprezentacji/zarządu".
  - Not covered: private individuals and nierejestrowana.
  - Source: [P24 documents](https://www.przelewy24.pl/centrum-pomocy/sprzedaje-z-przelewy24/rejestracja-w-przelewy24/zarejestrowalemam-sie-w-p24-jakie-dokumenty-mam-wyslac).
- **Tpay:** the OWU definition above expressly covers an entity that is not a natural person, has legal capacity and does not conduct business activity. Documents: representatives' ID, KRS/CEIDG or other registry: [Tpay FAQ](https://support.tpay.com/faq/jakie-dokumenty-sa-potrzebne-do-weryfikacji-konta-w-serwisie-tpay-com). Seller registration API with `legalForm` from `GET /accounts/legalForm`; examples shown are JDG and foundation: [Tpay merchant accounts](https://docs-api.tpay.com/pl/merchant-accounts/). Sellers new to Tpay go through standard verification: [Tpay partner guide](https://support.tpay.com/sprzedawca/marketplace-instrukcja-dla-partnera).
- **PayU:** web form boarding covers legal entities only, up to 20 sellers; API boarding covers legal entities and individuals (C2C) with no seller limit. Sellers have 90 days to submit documents. Unverified submerchants cannot have orders: [PayU boarding](https://developers.payu.com/europe/docs/services/marketplace/boarding/). Before verification completes, payments are capped at 2,500 PLN: [PayU marketplace](https://poland.payu.com/our-solutions/marketplace/). Associations need representatives' ID unless present in the register of associations: [PayU register](https://developers.payu.com/europe/docs/get-started/set-up-account/register/).
- **Paynow:** Seller Boarding statuses include `AML_VERIFICATION`: [Paynow marketplace](https://docs.paynow.pl/docs/v3/marketplace). The merchant condition of an mBank business account may or may not extend to marketplace sellers: unclear from the page.
- **Autopay:** registration link valid 24 h, then verification transfer, then the balance point activates after positive verification: [Autopay marketplace](https://developers.autopay.pl/online/marketplace). Associations are not mentioned: [Autopay who can use](https://developers.autopay.pl/online/faq/o-usludze/ktore-dzialalnosnosci-rejestrowane-moga-korzystac-z-platnosci-online).
- **Adyen:** PL supports individual, organization, sole proprietorship and unincorporated partnership **[snippet]**: [Adyen verification](https://docs.adyen.com/platforms/verification-requirements).

### Inferences
- An association with no business activity selling participation in statutory activity looks acceptable in principle at Tpay and P24. On Stripe the entity type exists, but charitable fundraising is not supported in PL, so payments must be described as services, not donations.
- Sailing clubs without KRS (for example ordinary associations or clubs) are an explicit case on P24's list, which is relevant to Nautivo's market. Stripe has `unincorporated_non_profit`, though real acceptance is untested.
- Tax treatment of an association charging fees without business activity needs an accountant.

### Gaps
- Stripe's actual acceptance rate and document needs for Polish stowarzyszenia without KRS: requires asking Stripe.
- Paynow and Autopay on associations as marketplace sellers: requires asking operator's sales.
- Individuals (non-entrepreneurs) as organisers: only PayU API (C2C) and Stripe `individual` have any public basis; Stripe's agreement restricts personal use. Confirm with each operator.

## Liability, refunds, partial payments, delayed payouts and deposits

### Takeaway
On Stripe, the charge type decides who carries the risk: with destination charges the platform is first in line; with direct charges and Stripe-managed losses the risk stays with the organiser and Stripe. Polish operators' published terms put refunds and chargebacks on the merchant or seller's balance, and marketplace-level liability is not public. A kaucja by authorisation works only on cards and has time limits, so BLIK and P24 kaucja must be charge-then-refund.

### Cited Findings

**Liability**
- **Stripe, indirect charges:** "Your platform is always responsible for its own negative balances". "If you use indirect charges, assign negative balance responsibility to your platform, not to Stripe." If Stripe is responsible for connected account negatives: Stripe covers those losses and handles KYC, no platform reserve, and the platform cannot pause payouts or debit accounts. If the platform is responsible: it may have a reserve, and after 180 days negative balances are collected from it: [Stripe risk](https://docs.stripe.com/connect/risk-management).
- Stripe's own warning example: during COVID hotel and accommodation providers failed, and customers who pre-paid submitted chargebacks "which were covered by the platforms": [Stripe risk](https://docs.stripe.com/connect/risk-management).
- **Stripe, direct charges:** disputed amounts debit the connected account. Dispute fee payer depends on `fees_collector`/`controller.fees.payer`: [fee behaviour](https://docs.stripe.com/connect/direct-charges-fee-payer-behavior). Merchant is responsible for goods and services, refunds and returns: [Stripe SSA PL](https://stripe.com/legal/ssa/pl).
- **Stripe P24 and BLIK disputes:** P24 has no chargebacks; BLIK has full disputes with 12 days for evidence, no dispute fee, no appeal: [P24](https://docs.stripe.com/payments/p24), [BLIK](https://docs.stripe.com/payments/blik).
- **Tpay:** refunds need sufficient seller balance; refund and fees are deducted from the seller's balance: [Tpay marketplace API](https://docs-api.tpay.com/pl/marketplace/). Verifying a refund's merits rests with the seller or marketplace operator: [Tpay marketplace](https://tpay.com/dla-biznesu/marketplace). Chargeback 120 PLN: [Tpay offer](https://tpay.com/oferta). Tpay may require security: withheld payouts, "rezerwy kroczącej, kaucji, gwarancji": [Tpay OWU](https://tpay.com/tpay-owu-2).
- **P24:** may offset chargebacks from payments due to the Akceptant; otherwise a debit note is issued; 100 PLN handling fee: [P24 OWU](https://www.przelewy24.pl/storage/app/media/owu/przelewy24/owu-przelewy24-v.1.1.pdf). These are merchant terms, not marketplace terms.
- **PayU:** the marketplace documentation reviewed does not cover liability: [PayU integration](https://developers.payu.com/europe/docs/services/marketplace/integration/).

**Refunds and commission**
- **Stripe:** by default "the destination account keeps the funds" unless `reverse_transfer=true`, and "the platform account keeps the funds from the application fee" unless `refund_application_fee=true`. Partial refunds reverse proportionally: [destination charges](https://docs.stripe.com/connect/destination-charges.md?platform=web&ui=stripe-hosted).
- **Tpay:** "prowizja Spółki nie podlega zwrotowi na rzecz Akceptanta", plus the refund cost; execution within 7 working days: [Tpay OWU](https://tpay.com/tpay-owu-2). Marketplace refunds are full or partial per seller or product, up to 100% per seller: [Tpay marketplace API](https://docs-api.tpay.com/pl/marketplace/).
- **PayU:** partial marketplace refunds need `extCustomerId`; above a seller's share returns `AMOUNT_EXCEEDED`: [PayU integration](https://developers.payu.com/europe/docs/services/marketplace/integration/). How the marketplace fee reacts to refunds is not stated.
- **Paynow:** refunds up to 180 days; insufficient funds give `INSUFFICIENT_BALANCE_FUNDS` with a bank-transfer alternative: [Paynow marketplace](https://docs.paynow.pl/docs/v3/marketplace), [Paynow prices](https://www.paynow.pl/cennik).
- **Autopay:** "Prowizja za transakcję nie jest zwracana"; refund window 6 months for card and BLIK, 12 months for others (page may be dated): [Autopay settlements](https://developers.autopay.pl/online/faq/rozliczenia).
- **Refund windows on Stripe:** BLIK 730 days, P24 180 days: [BLIK](https://docs.stripe.com/payments/blik), [P24](https://docs.stripe.com/payments/p24).

**Delayed payouts**
- Stripe does not provide escrow. Manual payouts must be made within 90 days in Poland ("All other countries"): [manual payouts](https://docs.stripe.com/connect/manual-payouts). Stripe advises against holding funds arbitrarily and recommends a clear purpose and legal advice: [account balances](https://docs.stripe.com/connect/account-balances).
- Paynow's Buyer Protection Program lets transfers be confirmed or cancelled, with automatic confirmation after a set time; needs activation: [Paynow marketplace](https://docs.paynow.pl/docs/v3/marketplace).
- Autopay offers D+1 or on-demand `balancePayoff`: [Autopay marketplace](https://developers.autopay.pl/online/marketplace).
- PayU pays from per-seller balances with explicit payout requests: [PayU integration](https://developers.payu.com/europe/docs/services/marketplace/integration/).

**Kaucja via authorisation**
- **Stripe:** cards only here. BLIK and P24 show manual capture as not supported: [BLIK](https://docs.stripe.com/payments/blik), [P24](https://docs.stripe.com/payments/p24). Cards hold 7 days as customer-initiated (Visa MIT about 5): [holds](https://docs.stripe.com/payments/place-a-hold-on-a-payment-method).
- **Stripe extended authorisation:** up to 30 days (Visa 29 days 18 hours). Offered on IC+ pricing; blended-pricing users must contact Stripe. Visa outside listed categories costs +0.08%. Stripe warns that many networks limit extended windows to cases where the final amount is unknown at authorisation: [extended authorisation](https://docs.stripe.com/payments/extended-authorization.md?platform=web&ui=stripe-hosted).
- **PayU:** Visa e-commerce authorisation 8 days, pre-authorisation 28 days; "partial capture is not available for marketplace payments" and requires the account manager otherwise: [PayU capture](https://developers.payu.com/europe/docs/payment-flows/capture-and-cancel/).

### Inferences
- For prepaid trips, destination charges put a one-person company directly in front of chargebacks if an organiser fails. Direct charges with Stripe responsible for losses are the lower-risk structure; the price is less control and the fee and refund handling described above.
- Refund policy on the 3% is a Nautivo contract decision, and the money flow must be coded per operator: retaining commission on refunded payments needs clear organiser terms.
- A deposit held for weeks by authorisation is not reliable. A kaucja for a cruise weeks away is better implemented as a charge followed by refund (fee lost, see above) or as an organiser-side arrangement. Whether a platform or organiser may hold a kaucja, and the classification of funds, is for a lawyer.
- P24's OWU says funds received are not a deposit in the Civil Code sense: [P24 OWU](https://www.przelewy24.pl/storage/app/media/owu/przelewy24/owu-przelewy24-v.1.1.pdf). This is not a route to holding kaucja through an operator balance.
- Partial payments map to separate transactions at all operators; none reviewed offers a booking-level instalment object that Nautivo could avoid building.

### Gaps
- Marketplace liability allocation and negative balance handling at P24, Tpay, PayU, Paynow and Autopay: requires asking operator's sales and reading their marketplace agreements.
- Authorisation or hold capability for cards at P24, Tpay, Paynow and Autopay marketplaces: not found, requires asking operator's sales.
- Maximum delay for held funds at PayU, P24, Tpay and Autopay marketplaces: not found.

## Webhooks, idempotency, reconciliation, sandbox and .NET SDK

### Takeaway
Stripe is clearly the strongest: official .NET SDK, documented idempotency keys, signed webhooks and full test tooling. No official .NET SDK was found for any Polish operator, so they would need hand-written REST clients.

### Cited Findings

| Operator | Webhooks | Idempotency | Sandbox | .NET SDK |
|---|---|---|---|---|
| Stripe | `Stripe-Signature` HMAC-SHA256; live retries up to 3 days; duplicates possible; no ordering guarantee ([webhooks](https://docs.stripe.com/webhooks)) | `Idempotency-Key` up to 255 characters; keys may be pruned after at least 24 h ([API](https://docs.stripe.com/api/idempotent_requests)) | Sandbox, test cards, CLI, BLIK failure simulation ([destination charges](https://docs.stripe.com/connect/destination-charges.md?platform=web&ui=stripe-hosted)) | Official; .NET Standard 2.0+, .NET Core 8+ ([stripe-dotnet](https://github.com/stripe/stripe-dotnet)) |
| Tpay | Payment notifications; JWS signature in `X-JWS-Signature` **[snippet]** ([OpenAPI](https://openapi.tpay.com/)) | Not found | Yes, test environment for sellers ([merchant accounts](https://docs-api.tpay.com/pl/merchant-accounts/)) | Official PHP, iOS and Android found; no .NET found ([PHP SDK](https://github.com/tpay-com/tpay-openapi-php)) |
| PayU | Notifications with signatures; MD5 with second key for payout notifications ([integration](https://developers.payu.com/europe/docs/services/marketplace/integration/)) | `extRefundId` with `REFUND_IDEMPOTENCY_MISMATCH` ([refunds](https://developers.payu.com/europe/docs/payment-flows/refunds/)); `extPayoutId` unique per shop | Marketplace sandbox, web-form boarding platforms only ([testing](https://developers.payu.com/europe/docs/services/marketplace/testing/)) | Only community NuGet packages seen, not verified **[snippet]** ([PayUSharp](https://www.nuget.org/packages/PayUSharp)) |
| P24 | Refund callback with status and signature, third-party copy of schema **[snippet]** ([schema](https://apis.io/schemas/przelewy24/TransactionRefundResult/)) | Not found | sandbox.przelewy24.pl in a third-party copy of an Ekspres spec **[snippet]** ([spec](https://raw.githubusercontent.com/api-evangelist/przelewy24/refs/heads/main/openapi/openapi-ekspres.yml)) | None found |
| Paynow | Seller status and refund notifications to `notificationUrl` ([marketplace](https://docs.paynow.pl/docs/v3/marketplace)) | Not covered on that page; `externalId` identifies payments | Yes, sandbox panel, free ([prices](https://www.paynow.pl/cennik)) | Not checked |
| Autopay | ITN named in gateway documentation **[snippet]** ([PDF](https://developers.autopay.pl/pdf?documentId=384)) | Not found | Sandbox login linked ([settlements](https://developers.autopay.pl/online/faq/rozliczenia)) | Not checked |

**Reconciliation**
- **Stripe:** Application Fee objects, dashboard reports, balance history export, `payout.*` events: [destination charges](https://docs.stripe.com/connect/destination-charges.md?platform=web&ui=stripe-hosted), [payouts](https://docs.stripe.com/connect/manage-connected-account-payouts).
- **Tpay:** Partner Panel shows parent and child transactions; each seller has its own view: [Tpay marketplace](https://tpay.com/dla-biznesu/marketplace).
- **PayU:** `GET /customers/ext/<EXT_CUSTOMER_ID>/balances`: [PayU integration](https://developers.payu.com/europe/docs/services/marketplace/integration/).
- **Autopay:** refund status in a daily transaction report for some setups: [Autopay settlements](https://developers.autopay.pl/online/faq/rozliczenia).

### Inferences
- Nautivo's planned stack (.NET 10) works directly with stripe-dotnet. On other operators, budget for REST clients and for your own idempotency and signature checks. The project's rule that webhooks be idempotent and signed applies to all.
- PayU's MD5-based payout signature is weaker than HMAC-SHA256 or JWS; check whether stronger options exist before committing.

### Gaps
- P24 marketplace API details, because its documentation is JavaScript-rendered: needs a browser read or operator confirmation.
- Tpay webhook retry policy: only community repositories describe it. Official confirmation needed.
- "No official .NET SDK" is absence of evidence from limited searches, not a statement by the operators.

## Non-marketplace alternative: own merchant account per organiser

### Takeaway
Technically yes at every Polish operator, since each sells ordinary merchant accounts to companies and, in some cases, to associations. No published term was found either allowing or banning a platform-run setup with commission invoiced afterwards, so it must be confirmed. It is the lowest-risk way to start, but commission collection becomes Nautivo's own credit risk.

### Cited Findings
- Merchant accounts for organisers exist at P24 (including associations and clubs without KRS): [P24 documents](https://www.przelewy24.pl/centrum-pomocy/sprzedaje-z-przelewy24/rejestracja-w-przelewy24/zarejestrowalemam-sie-w-p24-jakie-dokumenty-mam-wyslac); Tpay: [Tpay OWU](https://tpay.com/tpay-owu-2); PayU: [PayU T&C](https://poland.payu.com/wp-content/uploads/sites/14/2026/05/Terms-and-conditions.pdf); Paynow with an mBank business account: [Paynow prices](https://www.paynow.pl/cennik).
- Partner programmes can add income or terms. P24 partners earn commission on referred merchants' transactions after signing an agreement, and marketplace operators are listed as candidates **[snippet]**: [P24 partner programme](https://www.przelewy24.pl/en/partner-program). Tpay's Partner recommends services, with remuneration and rate limits in panel terms; regulation effective 16 April 2026: [Tpay partner regulation](https://tpay.com/user/assets/files_for_download/regulamin_programu_partnerskiego.pdf).
- Credentials are the merchant's responsibility: on Tpay, actions performed with panel credentials and API keys are the Akceptant's full responsibility: [Tpay OWU](https://tpay.com/tpay-owu-2).
- Autopay requires the merchant's site to be accessible with its own terms and details: [Autopay who can use](https://developers.autopay.pl/online/faq/o-usludze/ktore-dzialalnosnosci-rejestrowane-moga-korzystac-z-platnosci-online).
- The closest Stripe equivalent is direct charges with `stripe`/`account` as fee payer. This gives no Connect fees and an automatic application fee instead of an invoice: [Connect pricing](https://stripe.com/pl/connect/pricing), [fee behaviour](https://docs.stripe.com/connect/direct-charges-fee-payer-behavior).

### Inferences
**Own merchant accounts (each organiser), commission invoiced monthly**
- Advantages: Nautivo never holds participant money; refunds and chargebacks stay with the organiser; subscription-first revenue fits the project priorities; no marketplace agreement or negotiation needed.
- Disadvantages: every organiser must pass operator KYC and website review separately, likely including a question of whether a Nautivo-hosted page counts as their site; Nautivo must integrate several operators or mandate one; the 3% depends on webhooks or reports and on organisers paying invoices; Nautivo needs secure storage of each organiser's API keys; operator fees are the organiser's by default.

**Suggested ordering, for the report writer to evaluate**
1. Pilot: one operator with own merchant accounts, or Stripe direct charges if MCC and association onboarding are confirmed.
2. Move to a marketplace product only when volume justifies negotiated pricing.

Stripe direct charges offer a similar liability position with automatic commission, so they are the main comparison point; Stripe's P24 restrictions and card and BLIK coverage are the check.

This is a recommendation, not a verified fact. Whether either setup keeps Nautivo outside regulated payment services needs legal confirmation.

### Gaps
- Questions for each operator's sales:
  1. Can a platform register or integrate merchants in this way, using organiser credentials?
  2. Does a profile page on Nautivo satisfy the merchant website requirement?
  3. Which MCC applies for sailing trips, courses and boat rental?
  4. Marketplace price list, minimum volume and liability terms?
  5. Are associations without KRS or business activity acceptable as marketplace sellers?
