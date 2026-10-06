# Crown Barber Shop

A full website for a fictional barber shop, built with ASP.NET Core MVC and SQLite.

**Live site:** http://crownbarbershop.runasp.net/

## Pages

- **Home** — hero section, top services and calls to action
- **Services** — full price list (7 services, with durations)
- **About** — the shop's story and three barber profiles
- **Booking** — service, barber, date and time selection with a working booking flow
- **Terms & Conditions** — full legal page, including a privacy (POPIA) section

## Features

- **Working booking system** — select a service, barber, date and time; availability updates live and double-bookings are blocked server-side
- **Calendar integration** — confirmed bookings can be added to Google Calendar or downloaded as a `.ics` file (Apple Calendar / Outlook), with the correct service, barber, date, time and location carried through
- **Popup offer** — a first-visit discount modal that can be claimed or dismissed
- **Responsive design** — tested on desktop, tablet and mobile
- **Full header and footer** — navigation, contact details, opening hours, social links and legal links

## Tech stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core with SQLite
- Vanilla JavaScript and CSS (no frontend framework)

## Running locally

```
dotnet restore
dotnet run
```

Then open the local address printed in the terminal. Images for the hero section and barber profiles should be placed in `wwwroot/images/`.
