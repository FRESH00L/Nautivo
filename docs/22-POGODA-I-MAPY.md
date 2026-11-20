# Pogoda i mapy w module rejsów

Data weryfikacji źródeł: 2026-11-09.

## Fakty

### Windy API

Źródła: https://api.windy.com/point-forecast/pricing, https://api.windy.com/map-forecast/pricing, https://api.windy.com/point-forecast/docs

| Produkt | Testing | Professional |
|---|---|---|
| Point Forecast API (dane dla punktu) | darmowy, 500 zapytań dziennie, dane „losowo przetasowane i lekko zmienione”, tylko do developmentu | 990 € rocznie, 10 000 zapytań dziennie |
| Map Forecast API (mapa Windy) | darmowy, 500 sesji dziennie, tylko model GFS i warstwy: wiatr, temperatura, ciśnienie, tylko do developmentu | 990 € rocznie, 10 000 sesji dziennie; ECMWF to dodatkowe 1000 € |

Point Forecast zawiera wiatr (składowe u/v), porywy, fale, swell i prądy. Na stronach Windy nie znaleźliśmy warunków licencji komercyjnej. **Nieznane.**

### Open-Meteo

Źródła: https://open-meteo.com/en/pricing, https://open-meteo.com/en/docs, https://open-meteo.com/en/docs/marine-weather-api

- Dostęp darmowy bez klucza: 600 zapytań na minutę, 10 000 dziennie, 300 000 miesięcznie. Według strony **nie obejmuje użytku komercyjnego** i jest przeznaczony do testów oraz prototypów.
- Dostęp komercyjny wymaga subskrypcji z kluczem API. Stała cena miesięczna zależna od limitu zapytań. **Strona nie podaje kwot; cena nieznana.**
- Dane objęte licencją CC BY 4.0, która wymaga podania źródła.
- Prognoza pogody sięga do 16 dni. Wiatr można pobrać w węzłach. Prognoza fal sięga do 8 dni.

### Mapa i wyszukiwanie portów

- Podkład mapy to kafelki OpenStreetMap (`tile.openstreetmap.org`), wyświetlane przez Leaflet.
- Porty wyszukujemy w publicznym Nominatim (OpenStreetMap), najwyżej jedno zapytanie na sekundę.
- **Nie zweryfikowaliśmy zasad korzystania z obu serwisów OSM przy użyciu komercyjnym.**

## Co jest w prototypie

- Pod mapą trasy wyświetla się prognoza dla wybranego portu z Open-Meteo: pogoda, temperatura, wiatr w węzłach i stopniach Beauforta, porywy, fala i opad.
- Gdy dni rejsu mieszczą się w zasięgu prognozy, tabela pokazuje dni rejsu. W przeciwnym razie pokazuje 7 najbliższych dni dla portu.
- Zapytania wysyła przeglądarka. Zawierają tylko współrzędne portu, bez danych osobowych.

## Rekomendacja

1. **Do pokazów i testów: Open-Meteo.** Jest bezpłatny na etapie prototypu i ma wiatr, porywy i fale.
2. **Windy nie teraz.** Darmowy klucz zwraca celowo zmienione dane, więc nie nadaje się nawet do pokazów. 990 € rocznie to dużo przy budżecie 5000 zł.
3. **Przed pierwszym płacącym klientem** trzeba wybrać jedną z dróg:
   - subskrypcja Open-Meteo (najpierw zapytać o cenę),
   - własny serwer Open-Meteo (kod jest otwarty, ale to dodatkowe utrzymanie),
   - inny dostawca.

   Podobnie trzeba zdecydować o kafelkach map i wyszukiwarce portów: płatny dostawca albo potwierdzenie, że zasady OSM pozwalają na nasze użycie.

## Ryzyka

- Prognoza modelowa nie zastępuje komunikatów meteorologicznych dla żeglugi. W aplikacji jest o tym informacja.
- Wpięcie płatnego dostawcy wymaga przeniesienia zapytań na backend, żeby nie pokazywać klucza API w przeglądarce.
- Kwestie licencyjne (CC BY 4.0, warunki OSM i Windy) do potwierdzenia przez prawnika przed startem komercyjnym.
