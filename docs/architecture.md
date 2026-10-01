# Architecture & Process Diagrams

All 4 diagrams live in one [Claude Design canvas](https://claude.ai/code/artifact/de7f860c-7666-4ccb-86c7-49cb7c7a0d98) — System Overview, Production Process, IoT Data Flow, and Maintenance Workflow. Static PNG/PDF exports will replace this link once pulled from the canvas.

## 1. System Overview

The 4-part cycle the whole system runs on: **Shop Floor** (machines + sensors) → **Edge Gateway** (checks thresholds, raises alerts) → **FMMS Web Application** (work order management) → **Dashboard & Reports** (MTBF/MTTR, trends) — and the insights feed back into how machines are watched.

## 2. Production Process

The 7-stage tire manufacturing line this system monitors, end to end: raw materials → internal mixing → component prep (3 parallel branches: extrusion, calendering, bead winding) → tire building → **curing** (criticality level 1 — the line's single most critical process, since downtime here stalls every stage behind it) → inspection & testing (3 parallel checks) → finished goods.

This is the domain model every other diagram assumes — it's why the monitored machines are Curing Press, Extruder, and Banbury Mixer specifically, and why Curing Press gets the tightest response SLA.

## 3. IoT Data Flow

The 4-layer path a sensor reading takes from PLC to dashboard: **Shop Floor** (3 machines, 9 sensor channels, each with its own warning/critical threshold pair) → **Edge Gateway** (Node-RED: converts units, checks thresholds locally so a critical condition is still caught during a backend outage) → **Application Server** (.NET Core: authoritative re-evaluation, alert engine, work-order creation) → **Presentation** (live dashboard, work orders, analytics).

The alert logic this solves: a `CRITICAL` reading creates a work order immediately; a `WARNING` reading only notifies at first, and only escalates to a work order if it stays unresolved for 5 consecutive samples — so a transient spike doesn't page a technician, but a real developing fault doesn't get missed either. Priority and SLA (Urgent 1h / High 4h / Medium 24h / Low 72h) come from alert severity crossed with machine criticality.

## 4. Maintenance Workflow

The full lifecycle a problem goes through, across 4 actors: **Operator/System** detects it (manual report or automatic IoT alert) → **Supervisor** reviews and assigns a technician → **Technician** diagnoses, repairs, and closes it → **System/Analytics** recalculates MTBF/MTTR and surfaces recurring-failure patterns. This is the part that replaces the paper/spreadsheet process — one system tracks a problem from the moment it's noticed to the moment its data feeds back into deciding what to fix preventively next.

## Threshold reference

Full per-channel warning/critical values for all 9 monitored sensor channels (with the derivation rule for values not sourced from the original architecture sketch) live in the design-spec repo: [`fmms-system-design/01-SRS.md` §2.1](https://github.com/Oz05-jpg/fmms-system-design/blob/main/01-SRS.md).

## UI reference (for #005 Dashboard & Reports)

[MMS Live Demo v2](https://claude.ai/design/p/7f53dce8-fbfb-4a57-84df-a8744a2d6ac3?file=MMS+Live+Demo+v2.dc.html) — Claude Design mockup for the target UI, saved 2026-09-30. Reference only for layout/visual direction when #005 opens; Razor views still get built through teaching mode (guided, not generated wholesale) per the project's no-AI-generated-code rule.
