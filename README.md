# FMMS Machine Monitoring

A machine-monitoring and maintenance web app for a generic factory: machine registry, simulated sensor data, threshold alerts, preventive maintenance, work orders, and a dashboard. Built with ASP.NET Core MVC, EF Core and SQL Server.

This is a self-directed portfolio project, not a commissioned or deployed system. Its scope is modeled on the needs described in real manufacturing-plant IT Developer job postings in Thailand, used as a realistic scope to build against. It has no affiliation with any company and was never assigned or delivered to one.

**All sensor data is simulated.** A background service generates readings; there is no PLC, Node-RED or real IoT hardware connected.

## How this repo relates to the other two FMMS repos

FMMS is one system idea across three separate repositories. They are separate codebases, not one project.

| Repo | What it is |
|---|---|
| [FMMS-System-Design](https://github.com/Oz05-jpg/FMMS-System-Design) | The specification: SRS, system analysis and design (IEEE-style). Its target architecture goes beyond what has been built. |
| [FMMS-Core](https://github.com/Oz05-jpg/FMMS-Core) | A full-feature base built ticket by ticket while learning: Identity/Roles, JWT + refresh tokens, SignalR, Web API + Swagger, Excel/PDF export, xUnit + Moq, Docker, CI. |
| **FMMS-Machine-Monitoring (this repo)** | Re-designed from that SRS with a deliberately smaller scope: machine monitoring, sensor simulation, alerts, preventive maintenance, work orders, dashboard. This is the main portfolio piece. |

"Release 1" and "Release 2" below refer to releases of this repo only.

## Why this exists

Unplanned machine downtime is expensive, and many plants still track repairs on paper and spreadsheets: history is scattered, shift handoffs lose context, and nobody can say which machines break most often. This project turns that into one system: detect a problem, track it to resolution, and see the pattern over time instead of just the one ticket in front of you.

The simulator knows a small sample plant of three machines (codes `CP-001`, `EX-002`, `BM-001`) with three sensor channels each. See [`docs/architecture.md`](docs/architecture.md) for the illustrative process and data-flow diagrams.

## Feature status

Labels: **Implemented** (works in the code today) · **Simplified** (works, deliberately reduced) · **Deferred** (not planned for Release 1) · **Planned (#ticket)** (scheduled).

| Feature | Status | Notes |
|---|---|---|
| Machine registry (CRUD) | Implemented | Code, name, model, location, install date, status (`Running`/`Down`/`UnderMaintenance`). Unique machine code. |
| Sensor simulation | Implemented | Simulated readings every 5 s for 3 machines × 3 channels. Not real data. |
| Threshold alerts | Implemented | Warning/Critical thresholds are illustrative constants in code (Simplified). Alert raised after 3 breaching samples, cleared after 5 normal ones. Stored in the DB only; no Alerts page yet. Known rule defects, see below. |
| Alert → work order | Implemented | Critical alerts create an Urgent work order; Warning alerts create one after a persistence rule. Known defects, see below. |
| Work orders (create / edit / delete) | Simplified | Open or closed only (closed = `ClosedDate` set), urgency `Low`/`Medium`/`High`/`Urgent`, type `Corrective`/`Preventive`. No status workflow, no root cause or parts-used fields. |
| Technician assignment | Simplified | A `TechnicianId` field on the work order. No technician management page. |
| Preventive maintenance | Simplified | Hourly background job creates a PM work order when a schedule is due within 7 days; closing it advances the schedule. Schedules are managed directly in the database (no UI). |
| Dashboard | Implemented | Machine status cards, open work orders by urgency, MTTR per machine with chart. |
| MTTR | Simplified | See the known limitation below. |
| Login + 4 roles (Operator / Technician / Supervisor / Manager) | Planned (#006), 6 Dec 2026 | **No authentication exists today.** The app is open to anyone who can reach it. |
| Alerts page + Acknowledge / Clear | Planned (#007), 27 Dec 2026 | |
| MTBF + top machines by breakdown | Planned (#008), 24 Jan 2027 | Not implemented. |
| xUnit tests | Planned (#009), 21 Feb 2027 | There are no automated tests yet. |
| Deployment + demo | Planned (#010), 21 Mar 2027 | |
| IoT ingestion endpoint (`X-Api-Key`) | Planned (#011), 31 Mar 2027 | |
| JWT, SignalR, LINE/SMTP notifications, PDF/Excel export, configurable threshold rules (`ThresholdRule` entity), Serilog, time-series DB | Deferred to Release 2 | Present in the design spec, not in this app. |
| Real PLC / Node-RED integration | Deferred to Release 2 | |

### Known limitations

- **MTTR is currently measured as work-order created → closed.** That includes time spent waiting for a technician, so it is closer to work-order lead time than true repair time. The dashboard footnote says so. Correct MTTR needs a work-order start timestamp, which does not exist yet.
- **Alert → work order rules have defects:** a Warning alert that later crosses the Critical threshold does not create a work order, and Warning alerts tend to create work orders even when the value returns to normal quickly. Both are scheduled to be fixed with the alert work in #007 and covered by tests in #009.
- **Deleting a machine also deletes its work orders, alerts, readings and schedules** (cascade). There is no history protection yet.
- **Timestamps mix local time and UTC** across work orders, and the close date is typed by the user. This affects MTTR accuracy.
- **Sensor readings are never purged**, so the table grows while the simulator runs.

## Tech stack

| Layer | Technology |
|---|---|
| Backend | C# on ASP.NET Core MVC (.NET 10) |
| ORM | Entity Framework Core (Code First, migrations) |
| Database | SQL Server |
| Frontend | Razor Views + Bootstrap 5, Chart.js (loaded from CDN) |
| Background work | Two hosted services (`BackgroundService` + `PeriodicTimer`): sensor simulation, preventive maintenance |
| Auth | None yet (Identity planned in #006) |
| Source control | GitHub |

No SPA framework and no microservices: a single MVC app that one developer can own end to end.

## How to run

Prerequisites: .NET 10 SDK, a SQL Server instance, and the `dotnet-ef` tool.

1. Edit `ConnectionStrings:DefaultConnection` in `FMMSMachineMonitoring/FMMSMachineMonitoring/appsettings.json`. The committed value points at the author's local SQL Server instance, so replace the server name with your own, for example `(localdb)\mssqllocaldb` or `.\SQLEXPRESS`.
2. From `FMMSMachineMonitoring/FMMSMachineMonitoring/`, create the database: `dotnet ef database update`
3. Start the app: `dotnet run`
4. There is no seed data. In the Machines page, create machines with the exact codes `CP-001`, `EX-002` and `BM-001`; the simulator only generates readings for those codes and logs a warning for any other machine.

Alerts and the work orders they create appear after a short while. The preventive-maintenance job runs once an hour, starting one hour after startup, and needs a `MaintenanceSchedule` row inserted directly in the database.

## Methodology

Built against the classic 6-phase SDLC (Planning & Requirement Analysis → Requirements → Design → Build → Testing → Deployment & Maintenance) as a documentation overlay on top of a ticket-based workflow. Phase map and artifacts: [`docs/sdlc-plan.md`](docs/sdlc-plan.md).

## Status

- [x] **[Planning]** Domain research, business context, process flowcharts
- [x] **[Requirements]** Scope defined — [docs/requirements.md](docs/requirements.md)
- [x] **[Design]** ER diagram — [docs/er-diagram.md](docs/er-diagram.md) (Machine, Technician, WorkOrder, SensorReading, Alert, MaintenanceSchedule)
- [x] **[Build]** Machine registry CRUD
- [x] **[Build]** Work orders (simplified: open/closed) and technician field
- [x] **[Build]** Preventive maintenance generator (no management UI)
- [x] **[Build]** Sensor simulation, threshold alerts, alert → work order (with known defects)
- [x] **[Build]** Dashboard: status cards, open work orders by urgency, MTTR table + chart
- [ ] **[Build]** #006 Login + 4 roles — 6 Dec 2026
- [ ] **[Build]** #007 Alerts page + Acknowledge/Clear — 27 Dec 2026
- [ ] **[Build]** #008 MTBF + top machines by breakdown — 24 Jan 2027
- [ ] **[Testing]** #009 xUnit tests — 21 Feb 2027
- [ ] **[Deployment]** #010 Deployment + demo — 21 Mar 2027
- [ ] **[Build]** #011 IoT ingestion endpoint (`X-Api-Key`) — 31 Mar 2027

## Screenshots

<!-- TODO: add dashboard screenshot (status cards, open work orders by urgency, MTTR chart) -->
<!-- TODO: add work order list screenshot -->

## Diagrams

<!-- TODO: add architecture diagram (committed image or Mermaid) -->
<!-- TODO: add work order / alert flow diagram -->

Existing process and architecture diagrams: [`docs/architecture.md`](docs/architecture.md)
