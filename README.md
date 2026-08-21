# FMMS — Factory Maintenance Management System

A full-stack maintenance work-order system for a tire manufacturing plant — machine registry, work orders, preventive maintenance scheduling, and a reporting dashboard, with mock IoT sensor data standing in for real PLC/Node-RED input in V1.

This is a self-directed portfolio project, not a commissioned or deployed system. It models the operational needs described in a real manufacturing-plant IT Developer job posting — [Yokohama Tire Manufacturing (Thailand), JobThai #1871739](https://www.jobthai.com/th/job/1871739) — as a realistic scope to build against. It has no affiliation with that company and was never assigned or delivered to them.

## Why this exists

Unplanned machine downtime is one of the largest hidden costs in tire manufacturing — a breakdown at the wrong stage stops the whole line, not just one machine. The plants this system is modeled on run mostly on paper and spreadsheets today: repair history is scattered, shift handoffs lose context, and there's no data to say which machines actually break most often. FMMS turns that into one system: log a problem, track it to resolution, and see the pattern across months instead of just the one ticket in front of you.

## Domain

Tire manufacturing is a 7-stage line — raw materials → internal mixing → component prep (extrusion / calendering / bead winding) → tire building → **curing** → inspection → finished goods. Curing is the bottleneck: it's the single most critical machine on the line, because unplanned downtime there stalls every stage behind it. That's why the monitoring design treats it as criticality level 1 and everything else as level 2.

See [`docs/architecture.md`](docs/architecture.md) for the full production process, IoT data flow, and maintenance workflow diagrams.

## Scope — V1

| # | Feature | Summary |
|---|---------|---------|
| 1 | Machine Master | CRUD for machine registry — code, name, model, location, install date, status |
| 2 | Work Order Management | Create → assign → track (`NEW`/`ASSIGNED`/`IN_PROGRESS`/`ON_HOLD`/`COMPLETED`/`CLOSED`/`CANCELLED`) → close with root cause and parts used |
| 3 | Preventive Maintenance | Recurring schedules by interval; auto-generates a work order when due |
| 4 | Dashboard & Reports | Open/closed counts, top machines by breakdown frequency, MTBF/MTTR |

V1 uses **mock sensor data** (a script simulating temperature/vibration/pressure readings) in place of a real PLC/Node-RED connection — see [`docs/architecture.md`](docs/architecture.md) for what a V2 with real IoT hardware would add.

## User roles

- **Operator** — reports problems, checks status
- **Technician** — receives work orders, records repairs
- **Supervisor** — assigns work, reviews the dashboard
- **Manager** — reviews reports and KPIs

## Tech stack

| Layer | Technology |
|---|---|
| Backend | C# .NET Core MVC |
| ORM | Entity Framework Core (Code First) |
| Database | SQL Server (LocalDB in development) |
| Frontend | Razor Views + Bootstrap 5 |
| Auth | ASP.NET Core Identity |
| Source control | GitHub |

Chosen to match the tech stack in the reference job posting, not a generic default — no SPA framework, no microservices; a single MVC app one developer can own end to end.

## Status

- [x] Domain research, business context, and process flowcharts
- [x] Requirement scope and user roles defined
- [ ] Database design (ER diagram) — in progress
- [ ] Backend (models, controllers, EF Core)
- [ ] Frontend (dashboard, forms, views)
- [ ] Mock IoT data + live dashboard updates
- [ ] Deploy + demo video

## Diagrams

Full architecture and process diagrams: [`docs/architecture.md`](docs/architecture.md)
