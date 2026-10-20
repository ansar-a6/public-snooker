# Snooker Club Management System — Codex Implementation Blueprint

**Specification status:** Core workflow ready for a working MVP; supporting modules and a few edge cases remain open.  
**Prepared:** 19 October 2026  
**Project type:** Offline-first desktop application for one club and one primary operator.  
**Source of requirements:** Owner's explanations and draw.io dashboard, table-display, interaction, and preliminary database diagrams.  
**Purpose of this document:** Give Codex enough context to create a working application incrementally, without inventing unsupported features.

> **Important instruction to Codex:** Treat **CONFIRMED** rules as requirements. Treat **PROVISIONAL** items as implementation assumptions that must remain easy to change. Do not silently turn **OPEN** decisions into business policy. The owner's later conversational clarifications override older text embedded in draw.io diagrams and the earlier draft blueprint.

---

## 1. Product goal and boundaries

Build software that replaces the club owner's manual paper records for checking customers in/out, noting frames, tracking beverages and other canteen requests, calculating bills, taking payment, and preserving detailed records for future analysis.

The app is operated by the **same person who is owner, manager, and cashier**. The design must minimize clicks and typing. Do **not** add staff login, roles/permissions, manager approval, customer registration, remote services, or complex workflows to the MVP.

**Operating principle:** The owner manually enters real-world facts; the software reliably records, calculates, displays, and retrieves them. No sensors, table tracking hardware, automatic game detection, or payment gateway are required.

### 1.1 Core release (MVP)

Implement these features as working, persistent functionality:

1. Dashboard representing the real physical club layout.
2. Start and manage sessions on seven standard tables and one Private Room.
3. Billiards (frame-based), Century (minute-based), and Private Room (hour-block countdown) behaviors.
4. Append-only live input for frames and canteen requests; immediate persistence.
5. Fast full-view and quick-entry interactions (left/right click).
6. Session-ending and bill-review stage; record every pre-payment edit as a conflict/correction.
7. Mark full bill **Paid** (cash or online/bank transfer) **or Unpaid**; release the table in either case.
8. Later settlement of unpaid bills.
9. Product catalog and stock additions/discontinuations sufficient to support session purchases.
10. Crash recovery that restores saved active sessions and requests owner review of interrupted time.
11. Historical session/bill/event records and a basic export path for future analytics.

### 1.2 Later / separately planned (do not fully implement yet)

- The complete **Analysis** ribbon, reports, predictive models, dashboards, and profits.
- Advanced **Inventory** workflows involving cues, balls, chalk, purchase accounting, suppliers, or stock valuation.
- Detailed **Settings** screens beyond essential configurable prices and club basics.
- Cloud synchronization, multi-computer operation, remote access, or online backup.
- External payment integrations, receipt printer support, mobile/web clients.
- Additional staff accounts, authentication/approval systems, multi-branch operation.

These may appear as clearly labeled **Coming later** placeholders, but must not be represented as finished features.

---

## 2. Proposed technology and deployment

### 2.1 Confirmed deployment and working stack

| Component | Choice | Why |
|---|---|---|
| Desktop app | **C# with .NET WPF** | Responsive Windows desktop GUI; suitable for mouse interactions and custom club layout |
| Local persistence | **SQLite** | Free, embedded, offline, durable local database |
| Database access | .NET SQLite library; repository/data-access separation | Keeps storage details independent of the GUI |
| Future analysis | **Python**, in a separate optional analysis tool | Uses the owner's data-science skills and supports CSV/Excel-style exports |
| Testing | Automated tests for billing, state transitions, conflict history, and persistence | Financial/session rules must be repeatable |
| Source control | Git and a private GitHub repository | History and version control |

**Owner confirmation (20 October 2026):** The club application targets **Windows 10 and Windows 11**, using **C#/.NET WPF with SQLite**. Deliver a self-contained Windows executable; keep the local database in a writable user-data directory separate from the executable. Python remains optional for later analysis. Confirm the club computer's processor architecture and exact Windows edition/build before release packaging, and test the published application on both target operating systems. Runtime vendor support and actual application compatibility are separate checks; do not promise support for every Windows 10 edition/build.

Development currently takes place on Linux. WPF GUI execution and visual smoke testing require Windows. Project-local .NET SDK 10.0.402 is installed for compilation; Windows GUI testing remains necessary.

Do not introduce a Python subprocess into the daily cashier workflow merely to calculate totals. Keep Python analytics independent so the primary app can run without Python installed.

### 2.2 Nonfunctional goals

- Fully functional without internet access.
- No monthly operating fees or paid APIs.
- Fast table-card interaction and a readable dark-themed dashboard.
- The UI must remain responsive while timers run and records are saved.
- Single operator / single local installation in v1.
- Preserve useful source details, not only totals, for later data analysis.
- Database/file errors must produce clear messages and not silently lose data.
- The executable and database must use suitable writable user-data locations; do not rely on a writable app installation directory.

---

## 3. Physical club layout and navigation (CONFIRMED)

There are **seven numbered standard tables**, **one Private Room**, and **a Counter**. The Counter represents the owner/cashier location and is **not** a bookable playing table. The canteen is run from this operating area.

Reproduce this approximate layout (left-to-right), not a generic table list:

```text
┌────────────────────────────────────────────────────────────────────┐
│ Inventory                                 Analysis       Settings │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│ [Table 5] [Table 4] [Table 3] [Table 2] [Table 1]               │
│                                                                    │
│ [Table 6] [Table 7]   [Counter]   [      Private Room      ]       │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

Actual spacing/card proportions should follow the supplied draw.io reference when available. The Private Room card occupies more horizontal space than a standard table card.

**Ribbon:** `Inventory` (left); `Analysis` and `Settings` (right). All three can open screens. Inventory's basic MVP operations are defined below; Analysis and advanced Settings are later-stage work.

### 3.1 Empty versus occupied

- An **empty** table is clearly labeled `Table N` with a **red** outline/glow and is available.
- On starting a session, it becomes an **occupied** card, with the **customer name** prominent instead of only the table number.
- Active-session game-type colors in the more recent Table Logic reference: **Billiards = orange**, **Century = green**, **Private Room = white**.
- Empty-state red and Private Room **expired-time alert red** are different situations and must be distinguished by a status label/icon as well as color. Do not depend on color alone.
- Upon completing or shelving an ended session as unpaid, the associated table becomes free for another customer.

### 3.2 The dashboard is a glanceable summary, not the database

Table cards should show **relevant current information**, not every stored field. Examples of compact content:

- Customer name in a large readable heading.
- Canteen item image/label and current quantity, as space permits.
- Game-specific count or time.
- A prominent status/footer, e.g. estimated charge, elapsed minutes, or Private Room countdown.

Use item names as fallback when images are missing. Do not make images a requirement for operation. The large Private Room card can show more detail than a small standard table card.

**Reference examples (illustrative sample data, not hardcoded records):**

| Display example | Game | Shown on card |
|---|---|---|
| Khuzzama | Billiards | Water bottle ×1, frame count 3, estimated charge Rs 660 |
| Shezad | Century | Soda bottle ×3, water bottle ×2, `36m` elapsed |
| Ghulam Ghous | Private Room | Customer name, large `59:59` countdown |

Do not infer catalog prices from the illustrative Rs 660 amount. Demo data must be clearly separated from real settings.

---

## 4. Definitive mouse interactions (CONFIRMED; overrides older drawing annotation)

| Table state | **Left-click** | **Right-click** |
|---|---|---|
| Empty standard table | Open **Start Session** | **No action** |
| Occupied standard table | Open **full live-session view** | Open **quick input to add items or session data** |
| Empty Private Room | Open **Start Private Room** | **No action** |
| Occupied Private Room | Open full session/extension details | Open quick entry for canteen/session additions |

The older draw.io text saying that left-click adds live items is **superseded** by this newer confirmed decision. No double-click requirement has been confirmed; use ordinary single left/right clicks.

During an active session, the normal quick-entry workflow is **add/increase only**. It must not casually delete or decrease prior entries. Changes/corrections are handled during the ended-but-unpaid review stage and preserved in history.

---

## 5. Start Session (CONFIRMED core, minor UI details provisional)

1. The owner left-clicks an empty **standard table**.
2. The app asks for **customer name** (required) and **game type** (`Billiards` or `Century`).
3. Player count defaults to **2 players** for the current release. The user has not defined pricing differences for other counts; do not invent any.
4. Confirming start records the real start timestamp and creates a unique session.
5. The table becomes occupied immediately; its card switches to game-specific content/color.
6. At start, canteen items are **not** assumed or required; the frame count begins at zero for Billiards.

The game type may have a UI default, but the selected game must be explicit and saved. Pricing is copied/snapshotted into the session so later price changes cannot rewrite historical bills.

### 5.1 Private Room start

- Starts by left-clicking the empty Private Room.
- Requires customer name.
- **No Billiards/Century selection** is required; customers can play whatever they wish.
- Its timer is a **one-hour countdown**, rather than the standard table's game-dependent timer/counter.
- It shares the live canteen additions and session-recording concept of ordinary tables.

### 5.2 Occupancy constraints

Only **one active session per physical location**. Never replace an ongoing session with another customer's details. A completed/ended session remains recorded even after another session begins on that location.

---

## 6. Live sessions and event recording (CONFIRMED)

**Mental model:** Customer activities are unknown when the session starts. The owner adds real-world events over time. This is what the owner has called *stream data*; in implementation use **durable timestamped session events/activity** rather than relying on an in-memory-only stream.

Example timeline:

```text
16:00  Start session — Dawood, Table 1, Billiards
16:25  Owner records one frame
16:45  Owner records one beverage
16:52  Owner records one cigarette
17:00  Owner records one more frame
17:10  Owner records another cigarette
...    Session continues until the owner ends it
```

### 6.1 Billiards

- Charged per **owner-recorded frame count**.
- Owner adds a frame manually while the session runs.
- **Billiards charge = recorded frames × configured price per frame.**
- Start/end timestamps and elapsed time are still recorded for history even though elapsed time does **not** add a separate game charge.
- No automatic billiards scorekeeping or camera recognition.

### 6.2 Century

- Charged per **minute played**, plus canteen charges.
- **Century game charge = billable minutes × configured price per minute.**
- UI shows elapsed minutes prominently (e.g. `36m`).
- No frame charge is added for a Century session.
- No pause button was requested for normal operation.

**OPEN billing edge case:** What happens to a partial minute at checkout (floor/ceil/fractional) is not explicitly finalized. Preserve full timestamps. To make a working MVP, a developer may use **completed whole minutes**, but must label this as a **provisional rounding policy** and keep it isolated in a testable billing rule. Do not silently round sessions to whole hours.

### 6.3 Private Room

- Charge by **one-hour blocks** rather than game type.
- At start, show a countdown beginning from `01:00:00`.
- If the customer leaves early (for example after 25 minutes), charge the **full first-hour amount**.
- At zero, the card enters a conspicuous **red alert state** with an explanation that the hour expired; do not silently end/delete the session.
- When the owner opens it, provide **Continue Session** so the customer can stay longer.
- Canteen requests continue to be associated with the same Private Room customer/session.

**OPEN extension details:** User confirmed continuation and a fixed one-hour timer, but did not explicitly state the price mechanism for every additional hour. The sensible **provisional** implementation is that explicitly confirming `Continue Session` adds a new one-hour block and charges another configured hourly amount, retaining a single linked session with extension events. The application should **not** automatically add or bill a new hour merely because the timer hit zero. If time expires and no action is taken, keep the alert and preserve timestamps; define any overrun policy separately rather than guessing.

### 6.4 Canteen / item events

- Customer asks the owner or attendant for an item; the **owner manually records** it for that customer's active session.
- Right-clicking an occupied location opens a quick item-entry screen/grid showing available catalog products (prefer images + short labels + quantities).
- One product click adds/increases the recorded quantity by one; repeat clicks increase it again. No decrease/remove button in the normal active-session UI.
- Support adding a **manual/unlisted charge** by entering an item description and price, linked to the active session. Its inventory implications are **not confirmed**; do not invent a catalog stock deduction for a non-catalog item.
- Save each addition with session, item identity/description, quantity increment, unit price used at the time, and timestamp. A displayed aggregate (e.g. cigarette ×3) can be calculated from these events.
- Canteen charges are added to the selected session's estimated running total. Do not add them to a different customer's session.

### 6.5 Catalog stock safety (earlier confirmed inventory rule)

For tracked physical products:

- Don't allow an addition that would consume more than the stock currently available; stock must never go negative.
- Products with zero stock are unavailable to add.
- Confirmed additions reduce stock **exactly once**, together with the session event, using one atomic transaction.
- If the quick-input interface supports selecting several items before confirmation, validate **the entire selection** before committing; do not partially commit a mixed insufficient-stock order.
- When a later conflict/correction changes a recorded sale, maintain a compensating stock movement where appropriate, rather than silently rewriting stock history.
- An unlisted/manual charge is an exception and must not manufacture inventory quantities.

**Developer note:** Decide and document the immediate-commit behavior consistently in the UI. The owner's repeated-click workflow naturally favors saving each accepted addition immediately. Prevent double-click duplicates through clear visual feedback/idempotent confirmation; do not silently discard genuinely intentional repeated clicks.

---

## 7. Full live-session screen (CONFIRMED concept)

Left-clicking an occupied table opens a larger session view containing:

- Customer name, physical location, game type (if applicable), start time.
- Billiards frames / Century elapsed time / Private Room countdown and hour status.
- Recorded canteen entries and quantities with price snapshots.
- Current estimated charge, separated into game and canteen portions where applicable.
- A clear **End Session** control.
- Visibility of conflicts or interruption/recovery notes if present.

The screen should be readable and quick, not an accountant's ledger. Changes to live data normally go through the right-click quick-entry interface. Don't add unrestricted live delete/edit controls by default.

---

## 8. End Session, review, conflicts, payment and table release

### 8.1 End Session (CONFIRMED)

1. Owner selects **End Session** for the active customer.
2. Store end timestamp and stop counting game time.
3. The playing session enters a **review/checkout** stage; ending is **not** equivalent to getting paid.
4. Display all components of the bill and allow the owner to review it before choosing a final payment status.
5. Preserve all original session/activity data; do not overwrite it with edited values.

### 8.2 All review-stage edits are conflicts (CONFIRMED)

The owner explicitly decided: **every edit counts as a conflict**, including corrections to:

- Billiards frame count.
- Century billable minutes.
- Canteen item quantities or entries.
- Any other charge-affecting session details offered for editing.

For every edit, preserve:

- Session/bill identifier.
- Field/item affected.
- Original value and corrected value.
- Timestamp and a clear reason/note where practical.
- Old/new financial impact where applicable.
- The link between the unchanged original event history and the effective bill after correction.

A bill may have multiple conflicts. History must support later analysis such as counts, frequency, categories, and total money changed. The owner is the only operator; no manager approval process is required.

**Critical distinction:** A conflict is a **recorded revision of pre-payment billing data**, not permission to delete original historical facts.

### 8.3 Paid vs Unpaid (CONFIRMED)

From the ended-session review screen, the owner chooses one of two paths:

**Paid**

- Record the final charge and payment method: `Cash` or `Online / Bank Transfer`.
- A bank reference or note may optionally be saved.
- No partial payments in the MVP: the bill is either fully paid or unpaid.
- After confirming, the session/bill is finalized. Show an **on-screen final bill/receipt view**.
- **Do not send output to a physical printer**. Earlier discussion of "print" was corrected to mean displaying the final bill on screen.
- Lock the paid bill against direct editing. A later post-payment correction, if introduced, must be a separately traceable adjustment; do not modify the original transaction.

**Unpaid**

- Save the ended bill in a distinct **Unpaid Sessions / Unpaid Bills** area.
- The **table immediately becomes available** for the next customer, regardless of outstanding payment.
- The unpaid bill remains linked to the original customer/session and may later be opened and settled.
- An unpaid bill must not be mistaken for an active playing session.

**Key business rule:** *Table occupancy is independent from whether an ended session's bill has been paid.*

### 8.4 Status model (suggested technical representation)

Keep location state distinct from session/bill state:

```text
Location:  AVAILABLE | IN_USE

Session:   ACTIVE -> ENDED_REVIEW -> CLOSED
                            \-> ENDED_UNPAID -> CLOSED (after payment)

Bill:      DRAFT -> UNPAID -> PAID
                    \-----> PAID
```

Status names are implementation choices. What matters is the behaviors: no double-booking, ended/unpaid bills survive, and every finished payment is recorded once. Do not simultaneously leave a location occupied and available.

### 8.5 Sample billing examples (illustrative only)

**Example A — Zeeshan, Billiards**

- 5 frames × Rs 200 = Rs 1,000.
- 3 regular bottles × Rs 80 = Rs 240.
- 4 cigarettes × Rs 10 = Rs 40.
- **Total = Rs 1,280**.
- The 2-hour visit duration is saved for analysis but **does not add time billing**.

**Example B — Bajwa, Billiards**

- 2 frames × Rs 200 = Rs 400.
- 1 regular bottle × Rs 80 = Rs 80.
- 1 mineral bottle × Rs 60 = Rs 60.
- 3 cigarettes × Rs 10 = Rs 30.
- **Total = Rs 570**.

**Example C — Century**

- 103 minutes × Rs 10/minute = Rs 1,030, **plus** any separately priced canteen items.
- No exact final total can be asserted without the prices of all ordered items.

**Prices above are example inputs, NOT default or hardcoded production prices.**

---

## 9. Crash recovery and persistent live sessions (CONFIRMED)

The user's requirement is stronger than "save on checkout": **active session state must survive an unexpected shutdown**.

### 9.1 Persistence rules

- Save a new session **as soon as it starts**.
- Persist each accepted frame, canteen addition, Private Room extension, and significant state transition immediately using durable transactions.
- Use the database as the source of truth; don't keep the only copy of a current session in memory.
- On startup, load active sessions and re-create all occupied table cards from saved data.
- Do not require an internet connection or cloud account to restore.

### 9.2 Manual interruption-time review (CONFIRMED)

The user explicitly selected **manual review**, not automatic inclusion or exclusion of outage duration.

Example:

```text
16:00 — Century session starts
16:30 — Application crashes
16:40 — Application restarts
```

On recovery:

1. Restore saved customer, location, game, frames/items, and original timestamps.
2. Show a clear **Session recovery / time review required** state.
3. Show the interruption/restart information available (a precise crash moment may be unknowable if the process died without saving).
4. Allow the owner to **confirm or correct billable elapsed time** before resuming/finalizing time-based billing.
5. Save original timestamps and the owner's review decision/correction as a linked event.
6. Resume use of the session, preserving all successfully saved events.

Never silently decide that all closed-application time should be charged, or that it should all be excluded. The original session must not disappear during a crash.

**Implementation nuance:** A live on-screen countdown/clock can be derived from durable time anchors, but billing after recovery must honor the owner's reviewed time adjustment. Display a visible recovery-required warning until resolved.

---

## 10. Inventory ribbon — minimal working behavior (CONFIRMED scope)

The Inventory ribbon is used for managing canteen saleable items. It should provide:

- **Add product** with name, optional picture, category/type, selling price, starting quantity (if any), and low-stock threshold where useful.
- **Edit product** details and selling price.
- **Discontinue/deactivate product**, meaning no longer offered for new session additions; **do not delete its historical sales or old bills**.
- **Add stock/restock** to an existing product, with quantity and timestamp; preserve a stock movement record.
- Show current quantities and basic `Available`, `Low Stock`, or `Out of Stock` indications.

Historical sold item prices must stay fixed even when the catalog price later changes.

The early diagram mentions cues, balls and chalk. Those are **potential equipment inventory**, not confirmed as part of the first working release. Do not conflate them with saleable canteen stock without additional requirements.

**OPEN:** Complete supplier/purchase-cost tracking, returns, stock auditing, expense integration, and stand-alone canteen transactions (sales to non-playing visitors) will be planned later. Do not invent workflows for them in the MVP.

---

## 11. Analysis and Settings ribbons — intentionally limited for now

### 11.1 Analysis

The owner wants **rich historical data** later for revenue, profit/loss, trends, conflict frequencies, comparisons over time, export, and potentially machine-learning/prediction using Python.

For MVP:

- Preserve event-level history and pricing snapshots sufficient to support these future questions.
- Provide a simple list/search/filter for past sessions, paid/unpaid bills, and conflicts if practical.
- Provide a basic **CSV export** of historical records or an export service suitable for a future Analysis screen.
- Mark rich reports/forecasting as **planned**, not completed.

Keep confidential local customer/financial data out of the source repository.

### 11.2 Settings

At minimum, business rates and labels should not be hardcoded:

- Billiards **price per frame**.
- Century **price per minute**.
- Private Room **price per 1-hour period**.
- Business/club display name and currency (`Rs` / PKR).

Detailed Settings behavior is still pending. The MVP can use a minimal editable rates screen and an otherwise clear placeholder. Prices are snapshotted into each session or bill when applied.

---

## 12. Logical data requirements (NOT a fixed SQL schema)

Design a normalized, locally persistent model that preserves both **current state** and **historical activity**. The user's early diagram labels such as `snooker` and `canteen` are illustrative, not mandated table names. Codex may design proper SQLite tables, keys, foreign keys, indexes, and migrations at the storage implementation phase.

Suggested logical record types and required information:

| Record type | Important data |
|---|---|
| Location/Table | Stable location ID, visible name, kind (`standard`/`private`), position on dashboard, active session reference or derived occupancy |
| Session | Unique ID, customer name, location ID, start/end timestamps, mode, player count, status, rate snapshot, resume/recovery state |
| Session event | Unique event ID, session ID, type (`frame_added`, `product_added`, `room_extended`, etc.), quantity/change, timestamp, operator/context, optional event key |
| Product | Stable ID, name, type, image reference, current price, current available stock, active/discontinued status |
| Stock movement | Product ID, increase/decrease, old/new quantities or calculable result, reason/source, related session event ID, timestamp |
| Bill and bill lines | Session ID, original values, effective values, game charge, item charge, total, status; immutable price snapshots |
| Conflict/correction | Session/bill ID, affected field/event, old value, new value, time, reason if available, charge delta, link to original record |
| Payment | Bill ID, full amount, method (cash/online), optional transfer reference, paid timestamp |
| Private Room block | Session ID, hour-block number, start/end/extension details, block price, expiry/continuation action |
| Recovery event | Session ID, last reliable saved time, restart time if known, owner-reviewed billable time decision, timestamp |
| Rates/settings | Configurable prices and business defaults, with versioned/snapshotted value per session |

**Data principles:**

1. Unique IDs and real referential integrity for relationships.
2. Store timestamps as unambiguous date-times; show local human-friendly time in GUI.
3. Use precise **decimal or integer minor-unit money**, never binary floating-point for money.
4. Preserve sale-time prices, even if catalog prices change.
5. Keep the original events and create linked correction events; never erase conflict history.
6. Atomic transactions for bill completion, stock decrements, and payments.
7. Prevent accidental duplicate event/payment inserts.
8. Keep active/recoverable state in the same reliable local persistence system unless a compelling reason requires otherwise.
9. Structure the data to support safe CSV/Excel-compatible export later.
10. Schema design and migrations are an implementation task; don't make the provisional field names in early drawings contractual.

### 12.1 Important distinctions

- **Location availability** is not **bill payment status**.
- A **completed playing session** can have an **unpaid bill**.
- A **live event** is not the same as its **current aggregate quantity**.
- A **correction** changes the effective bill but does not delete its original evidence.
- **Displayed elapsed time** and **reviewed billable time after a crash** may differ.

---

## 13. Required screens and GUI interactions

| Screen | Minimum working requirements |
|---|---|
| Dashboard | Physical seven-table layout, Private Room, Counter, colored status/cards, ribbon, dynamic customer details, correct click actions |
| Start Session | Customer name, selected location, Billiards/Century for standard table, default 2 players, show applicable rate, confirm/cancel |
| Live Session Detail | Playing info, canteen quantities, elapsed/counter, estimated total, End Session; Private Room continuation info |
| Quick Add / Right-click | Product image/name grid, quick increment, frame increment if Billiards, manual item name+price, immediate feedback/stock checks |
| End Session Review | Ended time, original bill detail, edit/correction controls, conflict history, recalculate totals, Paid / Move to Unpaid |
| Unpaid Sessions | Searchable/listable ended unpaid bills, view original session, record later full payment, keep table free |
| Final Bill (on screen) | Customer, session, rate/frames/minutes/hour blocks, product lines, original/corrected totals if relevant, payment method/date; no printing |
| Inventory | Add/edit/discontinue product, add stock, product count/status; safe historical retention |
| History / Data export | Session/bill status, time range, recorded events, conflicts; basic export |
| Recovery Review | Interrupted active sessions, saved state, timestamps, manual billable-time review, confirm/resume |
| Analysis / Settings | Minimal rate settings; deeper analysis and settings clearly shown as future features |

Navigation back to Dashboard should not lose current data. Use clear confirmation for irreversible transitions such as ending a session or recording payment. Never make the owner navigate through multiple dialogs just to record an ordinary beverage or frame.

---

## 14. Business-rule identifiers for Codex and tests

- **BR-01** Only one active session per table/Private Room.
- **BR-02** Empty + left-click opens Start Session; empty + right-click does nothing.
- **BR-03** Occupied + left-click opens full session detail; occupied + right-click opens quick input.
- **BR-04** Standard tables support Billiards or Century, not both billing modes in one session.
- **BR-05** Billiards is charged per recorded frame; elapsed time is saved but not separately billed.
- **BR-06** Century is charged by elapsed/billable minutes; no frame charge.
- **BR-07** Default player count is 2; no unrequested player-based surcharge.
- **BR-08** Private Room uses one-hour paid blocks, a countdown, and an expiry alert.
- **BR-09** Private Room checkout before the end of the first hour still charges the complete first-hour amount.
- **BR-10** Owner explicitly selects continuation; expiry alone does not automatically end/extend/charge a new block.
- **BR-11** Live input adds/increases entries; routine live deletion/decrease is not offered.
- **BR-12** Every edit at ended-but-unpaid review is a logged conflict with original and corrected data.
- **BR-13** A paid bill's original record is locked against direct editing.
- **BR-14** Paid and Unpaid are mutually exclusive final payment choices for an ended session until an unpaid bill is later settled; no partial payment in v1.
- **BR-15** Moving a session to Unpaid immediately releases its playing location.
- **BR-16** Later settling an unpaid bill does not alter a different customer's current use of that table.
- **BR-17** Physical printing is not part of MVP; show an on-screen final bill.
- **BR-18** Accepted active-session changes are durably recorded and recoverable after app restart.
- **BR-19** After an interruption, owner manually reviews billable time; never silently include/exclude all downtime.
- **BR-20** Stock never goes negative; failed insufficient-stock inputs do not create a sale.
- **BR-21** Every stock-affecting addition changes stock exactly once, atomically with its session event.
- **BR-22** Discontinuing a product does not erase historical sales.
- **BR-23** Price changes do not retroactively change historic bills.
- **BR-24** No account management, staff approval, cloud access, or paid services required in first release.
- **BR-25** Preserve details adequate for future analysis, not just a single final amount.

---

## 15. Acceptance tests (functional and failure cases)

Codex must implement automated core-logic tests and provide reproducible manual GUI test instructions.

### 15.1 Dashboard and session tests

- **SES-01** Launch with all 7 standard tables and Private Room available; Counter is non-bookable.
- **SES-02** Left-click empty Table 2, enter customer + Billiards, start; card becomes occupied with customer's name.
- **SES-03** Right-click an empty table performs no action and creates no session.
- **SES-04** Left-click occupied table opens correct full-session information.
- **SES-05** Right-click occupied table opens input bound to that **same** session.
- **SES-06** Attempt second active session on same table is rejected.
- **SES-07** Other free tables can start sessions independently.
- **SES-08** Start Century and verify elapsed time display; frame charge never added to it.
- **SES-09** End a session and place it in Unpaid; table becomes free, bill remains searchable.
- **SES-10** A new customer can begin on a freed table while the previous customer's unpaid bill remains stored.

### 15.2 Billing and input tests

- **BIL-01** Billiards: 5 × 200 + 3 × 80 + 4 × 10 = 1280 using example data.
- **BIL-02** Billiards: 2 × 200 + 80 + 60 + 3 × 10 = 570 using example data.
- **BIL-03** Century: exactly 103 billable minutes × 10 = 1030 before canteen.
- **BIL-04** Adding the same product three times results in quantity 3 and three correctly persisted increments (or equivalent auditable events).
- **BIL-05** Normal active-session input has no decrease/delete action.
- **BIL-06** Unlisted manual-priced item appears on the correct bill without inventing catalog stock.
- **BIL-07** Ending the session stops its current play timer and produces reviewable charges.
- **BIL-08** Each review edit creates a conflict record with before/after values and the new effective total.
- **BIL-09** Multiple edits create multiple traceable conflict records; prior values are not lost.
- **BIL-10** Cash payment records full amount and locks completed bill.
- **BIL-11** Online/bank transfer records full amount and optional note/reference.
- **BIL-12** Partial payment cannot be entered in MVP.
- **BIL-13** Old bills retain old item prices after catalog price is edited.

### 15.3 Private Room and recovery tests

- **PR-01** Private Room starts without required game choice and counts down from one hour.
- **PR-02** Ending after less than an hour still charges the full initial hour.
- **PR-03** At countdown zero, red expiry alert appears; session data remains intact.
- **PR-04** Continue Session acts on the existing room session and records an extension event (under the documented provisional additional-hour policy).
- **REC-01** Start session, add a frame/item, force-close app, reopen; all committed events are restored.
- **REC-02** Interrupted Century session prompts manual review of billable time rather than deciding automatically.
- **REC-03** Owner's recovered-time decision and original timestamps are preserved for later review.
- **REC-04** An interruption does not create a duplicate frame, stock movement, payment, or session.

### 15.4 Inventory/history tests

- **INV-01** Add product; it appears in the quick-entry grid.
- **INV-02** Restock increases available quantity and records movement.
- **INV-03** An item purchase decreases stock exactly once and attaches to correct session.
- **INV-04** Insufficient stock blocks entry and never makes stock negative.
- **INV-05** Zero-stock product cannot be purchased.
- **INV-06** Discontinued product disappears from new-sale choices; historical bills stay readable.
- **INV-07** Changing product price does not change prices on earlier session events/bills.
- **DATA-01** Past session histories expose timestamps, item details, game details, payment status, and conflicts.
- **DATA-02** Exported data identifies rows clearly enough to join sessions, events, products, bills, and corrections.

---

## 16. Incremental build order for Codex

**Do not ask Codex to write the entire software in one giant untested pass.** Build milestones and complete tests/checkpoints between them.

| Milestone | Deliverable | Must be demonstrable before advancing |
|---|---|---|
| **M0 — Review** | Read this specification and summarize confirmed/provisional/open decisions; identify project files and Windows dependency | No code changes yet; architecture outline only |
| **M1 — Foundation** | New desktop app, dark theme, physical dashboard/ribbon, reusable table-card controls, navigation | Opens and navigates without database-dependent screens |
| **M2 — Persistence + sessions** | Local SQLite + migrations, locations, start sessions, crash-safe session state | Create/reopen multiple live sessions correctly |
| **M3 — Live input** | Right-click product/frame actions, product catalog, stock safety, event history, estimated totals | Clicking adds once, only to intended session; survives restart |
| **M4 — End/review/pay** | End Session, review, conflicts, Paid/Unpaid, released tables, unpaid settlement, on-screen bill | Complete Zeeshan/Bajwa sample workflows and corrections |
| **M5 — Private Room** | Hour countdown, expiry alert, continuation, full first-hour charge | Timer/alert/continue and history tested |
| **M6 — Recovery, exports, hardening** | Manual interruption time review, error handling, basic history/CSV, packaging | Crash simulation and recovery test pass |
| **M7 — Future modules** | Full Analysis, richer Inventory, Settings, Python analytics | **Out of scope until separately specified** |

At each milestone:

1. Run automated tests and basic GUI smoke checks.
2. Report implemented vs not implemented functionality.
3. Explain how to run the app.
4. List files changed, test results, and outstanding provisional assumptions.
5. Make a focused Git commit only after successful local verification.
6. Never silently discard previously captured real records or perform destructive schema resets.

---

## 17. Explicitly open decisions (do not hide these)

The core MVP can be prototyped despite these gaps, but production billing must not quietly guess the rules.

1. **Deployment OS — resolved:** Windows 10 and Windows 11 confirmed by the owner. Exact edition/build and processor architecture remain release-validation details.
2. **Production prices:** Billiards per-frame, Century per-minute, Private Room per-hour, product prices; examples are not defaults.
3. **Partial-minute billing:** Exact rule for seconds/partial minutes is not yet confirmed.
4. **Private Room added hours:** Provisional next full-hour charge on explicit continuation; precise extension accounting/overrun treatment must be confirmed.
5. **Manual/unlisted products:** No catalog stock deduction presumed; full inventory treatment undecided.
6. **Canteen-only non-playing customers:** Not requested or explained; do not implement yet.
7. **Inventory advanced accounting:** Purchase cost, equipment stock, suppliers and profit formula not specified.
8. **Analysis:** Detailed reports, charts, AI predictions and Excel export format pending. CSV is an MVP interoperability path, not a replacement for later requirements.
9. **Settings:** Only essential rates/configuration are known so far.
10. **Backup/restore:** Policy not specified yet; do not imply that local SQLite alone is a backup strategy.
11. **Full settlement of an already unpaid bill:** Must be supported, but whether owners may still edit the unpaid bill after shelving is inferred from the generic "ended and unpaid can be edited" rule. Keep post-session pre-payment conflict logging consistent and make this behavior configurable/adjustable.

Avoid hardcoding arbitrary rates or fictional policies as if the owner approved them.

---

## 18. Codex kickoff prompt (paste into Codex after saving this file)

```text
You are building the Snooker Club Management System as a real, local-first desktop application.

Read the entire `docs/CODEX_BLUEPRINT.md` file before writing code. Respect CONFIRMED, PROVISIONAL, and OPEN distinctions. Later owner clarifications override old notes in diagrams.

For your FIRST response:
1. Summarize the main workflow: dashboard -> start -> live events -> end/review/conflicts -> paid or unpaid -> history.
2. Identify any blocking deployment decision (especially Windows-only WPF).
3. Propose a small, maintainable project structure with separation between GUI, business logic, and SQLite data access.
4. Describe milestone M1 and how you will test it.
5. Do NOT implement the whole project yet; only begin coding when I ask you to execute a milestone.

Constraints:
- Low-cost/offline-first; no paid APIs or cloud services.
- C#/.NET WPF and SQLite are the proposed Windows stack, Python is for future optional analysis.
- Preserve event-level details, historical prices, all conflicts, and crash-recoverable active sessions.
- Never invent prices or silently decide an OPEN billing policy.
- Prioritize a fast owner-operated interface over unnecessary enterprise features.
- Don't change confirmed requirements without my approval.
```

### Recommended local placement

```text
Snooker-Management-System/
├── README.md
├── docs/
│   └── CODEX_BLUEPRINT.md      <-- save this file here
├── src/                         <-- created by Codex later
├── tests/                       <-- created by Codex later
└── .gitignore
```

**Important Git hygiene:** Ignore database files containing real club/customer/payment information, runtime logs with personal/financial details, generated backups, local secrets, and build outputs. Example/demo data may be committed only when obviously fictional.

---

## 19. Final scope reminder

The diagrams define the physical dashboard and its readable cards. The owner's **most recent chat clarifications** define actual button behavior and business rules. These are complementary: **visual layout is not the full storage model**, and **historical storage is richer than what's shown on a table card**.

A successful first working release lets the owner:

> Click an empty table, begin a customer session, record frames and canteen requests instantly, see a live summary, end the session, review and log any corrections, mark the full bill paid or leave it unpaid while immediately freeing the table, reopen the app after a crash without losing committed records, and view/export historical data.

That is the target. Do not claim advanced inventory, prediction, complete accounting, or cloud features are finished until separately specified and tested.
