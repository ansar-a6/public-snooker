# M2 — persistent sessions and local storage

## Implemented

- SQLite schema version 1, created transactionally; seven standard locations and
  one Private Room are seeded without fictional customers or prices.
- Persistent club name and optional PKR game rates. Blank rates prevent starting
  that game. An explicitly entered zero is allowed; decimal precision is two places.
- Start Billiards, Century, and Private Room sessions with a required customer
  name, unique request/session identifiers, UTC start time, two-player default,
  and immutable rate snapshot. No bill is calculated in M2.
- Save each session and its start event in the same transaction. A database
  unique index prevents two occupying sessions at one location. Repeating an
  identical request returns the already-saved session without a duplicate event.
- Rebuild occupied table cards from saved data. Show customer, location, mode,
  saved start time/rate, and activity in the session detail window.
- Mark active sessions found on app startup as requiring recovery review and
  preserve a recovery-required event. The first detection is recorded once while
  review remains pending; further restarts do not duplicate it. This applies even
  after a normal close because the app cannot know what happened while closed.
- Keep storage work off the GUI thread. Disable card actions if occupancy cannot
  be loaded. Show failures instead of treating an unreadable database as empty.
- Prevent a second application instance from opening the same local data file.

## Storage and scope

Windows data location:

```text
%LOCALAPPDATA%\SnookerClub\club.db
```

The database lives outside the checkout and executable folder. Rebuilding or
replacing the executable preserves it. SQLite may create `club.db-wal` and
`club.db-shm`; a `club.db.lock` file supports the application-instance lock.
Do not remove these files while the app is running. Local persistence is not a
backup strategy; backup/restore remains an open later requirement.

Money is stored as integer minor units, dates as UTC timestamps, and displayed
start times use the computer's local time. SQLite foreign keys, WAL journaling,
and FULL synchronous durability are enabled. Migrations never reset an existing
database; a newer schema version is rejected.

M2 is still a development preview. Use fictional customers. It does **not** yet
add frames/items, calculate elapsed-time bills, close sessions, release tables,
accept payment, run the Private Room countdown/extensions, or resolve recovery
reviews. After filling all eight locations, they remain occupied until a later
milestone adds checkout. There is intentionally no delete/reset workaround in the
app. M3 will work with these saved sessions.

Restored-session warnings do not decide whether downtime is billable. Manual
interruption-time review arrives in M6. The original start time and rate remain
unchanged. No partial-minute or additional-hour billing policy was introduced.

## Automated checks

From the repository root on Windows with the .NET 10 SDK:

```powershell
dotnet build SnookerClub.slnx -c Release
dotnet run --project tests/SnookerClub.Tests -c Release
```

On the current Linux workspace, substitute `./scripts/dotnet.sh` for `dotnet`.
This is an executable test harness (not a `dotnet test` project); a failing check
returns a nonzero exit code. Each test uses an isolated temporary database with
fictional data; it does not open the application's user-data database.

Verified on Linux:

- Release solution build: passed, 0 warnings and 0 errors.
- Self-contained Windows x64 single-file publish: passed. Output:
  `artifacts/win-x64/SnookerClub.exe` (approximately 136 MiB), including SQLite.
- `git diff --check`: passed.
- All 20 checks passed: schema initialization/reopening, all three modes,
  required names, game/location validation, missing/changed rates, occupied-table
  protection, idempotent requests, historical rate preservation, recovery event
  deduplication, transaction rollback on injected write failures, schema-version
  protection, settings validation, exact money parsing, SQL parameter handling,
  concurrent starts, and a killed writer process followed by recovery.

## Windows manual checks — pending

Build the updated executable using `docs\build-windows.cmd`. These checks require
Windows; Linux compilation cannot verify the WPF GUI or Windows native SQLite load.

1. On first launch, all eight locations are available. Start a standard table:
   the form shows that its rate must be set; Start session is disabled.
2. Open Settings, set a fictional club name and test rates, and save. Verify the
   club name changes. Close/reopen Settings and confirm saved values.
3. Enter a negative rate or three decimal places: saving is rejected. Blank rates
   remain unset. No production defaults are supplied by the app.
4. Start Table 1 as Billiards with a fictional customer. An empty name is rejected;
   a valid name creates an occupied orange card. Double-clicking Start must not
   duplicate the session. Its detail view shows the saved name, time, and rate.
5. Start Table 2 as Century; its card is green. Start the Private Room; its form
   has no Billiards/Century selector and its occupied card is white.
6. Click an occupied table: the correct details open, not a new-start form.
   Right-click an empty location does nothing. Right-click an occupied location
   explains that M3 quick entry is not yet implemented; no item is recorded.
7. Change a rate in Settings. Existing session details retain the old rate; a new
   session on another table shows the new rate before saving.
8. Close/reopen the app. Names, modes, start times, and rates are restored. Cards
   show RECOVERY REVIEW, and the detail view explains manual review is pending.
9. With sessions saved, use Task Manager to end SnookerClub. Reopen it. Verify
   the same sessions remain, no location has duplicate activity, and reopening
   again does not duplicate the pending recovery event.
10. Open another copy while the first is running. It should report that the app
    may already be open and must not open another dashboard.
11. Navigate away/back, resize, and use Tab/Enter/Space/Escape. Verify forms fit
    or scroll, focus is visible, and a second session cannot occupy a used table.
12. Repeat launch and persistence checks on Windows 10 and Windows 11, including
    a target machine without a separate .NET installation.

| System / edition / build | GUI + SQLite result |
|---|---|
| Windows 10 | Pending |
| Windows 11 | Pending |

## Implementation references

Storage uses Microsoft's [SQLite transactions](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/transactions).
Database calls run on a worker thread because Microsoft.Data.Sqlite's
[async calls execute synchronously](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/async).
