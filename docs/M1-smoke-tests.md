# M1 Windows smoke checks

Status: pending execution on Windows 10 and Windows 11. Linux compilation does
not verify WPF rendering, keyboard behavior, or runtime compatibility.

Use a Windows x64 machine for the win-x64 preview. Extract the entire published
folder if distributed as a zip, then double-click SnookerClub.exe. No separate
.NET runtime installation should be needed for the self-contained publication.

1. Launch: dark dashboard appears with no database requirement or network prompt.
2. Check physical order: top row 5, 4, 3, 2, 1; bottom row 6, 7, Counter,
   Private Room. Room spans two columns. Counter cannot be booked.
3. All eight locations have a red outline and a visible AVAILABLE label.
4. Single left-click each standard table: correct name appears in the modal form;
   customer name, Billiards/Century, and two-player default are visible.
5. Private Room opens its own form without game selection; one-hour description
   is visible. Neither form displays an invented production price.
6. Start session is disabled. Cancel, Escape, and the window close button dismiss
   the form. Reopening starts with an empty customer name; no records are saved.
7. Right-click each empty location: no form, context menu, or session appears.
8. Open Inventory, Analysis, and Settings. Each has clear planned status. Return
   through Back to dashboard and through the top Dashboard button.
9. Resize down to 900 × 620, maximize, and test at 100%, 150%, and 200% display
   scaling. Text remains readable; vertical scrolling exposes lower content.
10. Tab through buttons and fields, activate buttons using Space/Enter, and verify
    focus is visible. The Counter must not act as a button.
11. Close and relaunch. No occupied tables or persisted customer names are implied.
12. Repeat the launch check on a Windows machine without a separately installed
    .NET runtime. Record Windows edition/build, architecture, and results below.

## Compilation and publication results

- SDK: .NET 10.0.402, installed locally under `.dotnet` on Linux x64.
- Release solution build: passed, zero warnings and zero errors.
- Self-contained win-x64 single-file publication: passed.
- Artifact: `artifacts/win-x64/SnookerClub.exe`, approximately 134 MiB.
- File inspection: Windows GUI PE32+ x86-64 executable.
- `git diff --check`: passed.
- WPF runtime/visual checks: not executed; Linux cannot run the WPF app.
- No business-rule tests yet: M1 contains no billing or persistence logic.
- No milestone commit yet: Windows GUI smoke verification remains pending.

## Results

| System / build / scaling | Result | Notes |
|---|---|---|
| Windows 10 | Pending | Requires Windows machine |
| Windows 11 | Pending | Requires Windows machine |

M1 has no billing, timers, live additions, inventory storage, payment, or recovery
implementation. Those are deliberately outside this milestone.
