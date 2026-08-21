# Requirements — V1 User Stories

Grouped by the 4 V1 features in the [README](../README.md#scope--v1). Each maps to a controller in the eventual codebase.

## Machine Master

- As an **Administrator**, I want to register a machine (code, name, model, location, install date) so it can be tracked and reported on.
- As a **Supervisor**, I want to see a machine's status at a glance (running / down / under maintenance) so I know what's affecting the line right now.

## Work Order Management

- As an **Operator**, I want to report a problem on a specific machine (issue description, urgency) so a technician gets notified without me chasing anyone down.
- As the **System**, when a `CRITICAL` sensor alert fires, I want to create a work order automatically so a real fault is never waiting on someone to notice it manually.
- As a **Supervisor**, I want to assign an open work order to a technician so responsibility for it is clear.
- As a **Technician**, I want to see my assigned work orders with machine history so I can diagnose faster.
- As a **Technician**, I want to close a work order with root cause, parts used, and repair time so that data feeds into MTTR instead of being lost.
- As a **Supervisor**, I want an unassigned work order older than a set window to escalate to me so nothing sits open by accident.

## Preventive Maintenance

- As an **Administrator**, I want to set a recurring maintenance schedule for a machine (e.g. every 90 days) so preventive work doesn't depend on someone remembering.
- As the **System**, I want to auto-generate a work order when a preventive schedule comes due so it enters the same tracked workflow as a breakdown.

## Dashboard & Reports

- As a **Manager**, I want to see open/closed work-order counts and the top 5 machines by breakdown frequency so I know where to focus budget.
- As a **Manager**, I want MTBF and MTTR per machine so I can tell whether reliability is improving or getting worse over time.

## Out of scope for V1

Real PLC/Node-RED connection (mock data stands in — see [`architecture.md`](architecture.md#3-iot-data-flow)), SignalR live-push (nice-to-have if time allows), multi-plant support, spare-parts inventory.
