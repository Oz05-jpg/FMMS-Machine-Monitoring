# SDLC Plan — FMMS-Machine-Monitoring

Overlay only — does **not** replace the FM-TICKET / teaching-mode workflow (pedagogy, Level 1→2→3, explain-back). This maps work already done and still to come onto the classic 6-phase SDLC model, so the project reads as a deliberate process, not just a ticket list.

Model reference: [Software Development Life Cycle (SDLC)](https://bigwater.consulting/2019/04/08/software-development-life-cycle-sdlc/) — Planning & Requirement Analysis → Defining Requirements → Design → Build/Development → Testing → Deployment & Maintenance.

## Phase map

| Phase | Status | Artifact | FM-TICKET |
|---|---|---|---|
| 1. Planning & Requirement Analysis | ✅ Done | [README.md](../README.md) — domain, "Why this exists", scope built against the real [JobThai #1871739](https://www.jobthai.com/th/job/1871739) posting | before #001 |
| 2. Defining Requirements | ✅ Done | [requirements.md](requirements.md) — user stories, 4 features × 4 personas | before #001 |
| 3. Design | 🟡 Partial | [architecture.md](architecture.md) — system/data-flow/workflow diagrams. ER schema exists only as EF Core migrations, not yet a standalone diagram | before #001, then re-opens per new entity |
| 4. Build | 🔵 In progress | #001 Machine CRUD ✅ · #002 Work Order + Technician ✅ · #003 IoT refresh (next) · #004 Preventive Maintenance · #005 Dashboard & Reports | #001–#00X |
| 5. Testing | ⬜ Not opened | Inline unit tests continue per ticket (same pattern as YMS TICKET #008/#020) — no dedicated integration/regression pass yet | opens after #005 |
| 6. Deployment & Maintenance | ⬜ Not opened | README checklist item "Deploy + demo video" | after Testing |

## Rules

1. Every new FM-TICKET header states its phase: `🎫 FM-TICKET #003 — [Build] IoT Refresh`.
2. Design isn't closed after #001 — a new entity (e.g. the PM schedule in #004) gets its data model added to `architecture.md`/an ER diagram **after the ticket's code, during Post-Coding Flow (same commit as the feature)** — not before. Doc-before-code was the original draft of this rule; SAGE audit (2026-09-14) flagged it as a phase-gate in disguise that hands "คิดมากก่อนเริ่ม" a built-in excuse to open a session with documentation instead of code, right on the two tickets (#003, #004) that introduce new entities. Code first, doc same session.
3. Testing opens as its own named phase once all 4 V1 features are built (before the "Deploy" checklist item) — this does not pause writing unit tests per ticket in the meantime, which continues as it already has.
4. Maintenance = post-deploy work (bug fixes, small features) — still logged as FM-TICKETs, phase-tagged `[Maintenance]`. This phase has no end date, same as the portfolio-doc workstream.

## Pacing constraint (added after SAGE audit, 2026-09-14)

FMMS has not landed a single ticket on a normal weekday in the last 5 tracked days (2026-09-07 → 09-11) — SENSEI's YokohamaMaintenanceSystem takes the weekday learning slot every time it competes. Both FM-TICKET #001 and #002 only happened on days off (2026-09-05, 09-06), and the most recent weekend (09-12/09-13) produced zero FMMS tickets. This isn't a lack-of-plan problem — it's a lack-of-locked-slot problem.

**Rule: FMMS runs on a holiday/rest-day budget only, not weekday time.** 3 Build tickets remain (#003, #004, #005) + Testing + Deployment — feasible inside a handful of rest days, but only if the next rest day is actually spent on FM-TICKET #003 code, not on more documentation. Next rest day = FM-TICKET #003, code first.

## Why overlay, not replace

The pedagogy (Session Opener, Exercise Levels, explain-back, Post-Coding Flow) is what makes the code defensible in an interview — that stays untouched. SDLC phase-tagging is a documentation layer on top, so the project's own README/commit history reads as "built via a deliberate SDLC process" rather than only "a series of tickets," which is a stronger interview narrative for a system-analyst-adjacent role.
