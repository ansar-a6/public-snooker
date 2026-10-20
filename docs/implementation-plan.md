# Implementation plan — M1 foundation

## Confirmed workflow

Dashboard → start session → persist live frame/item events → end and review →
record every correction → mark Paid or Unpaid → release table → retain history.
Unpaid bills can later be settled independently of the table's new customer.
Private Room continuation requires an explicit action; expiry alone adds no charge.

Windows 10 and Windows 11 are the confirmed deployment targets. Use C#/.NET
WPF and SQLite, with a self-contained Windows executable and separate local data.
Python analysis is optional future work, outside the cashier workflow.

## Planned structure

- `src/SnookerClub.Desktop`: WPF views, view models, theme, and navigation.
- `src/SnookerClub.Core`: session rules, billing, money values, and service interfaces.
- `src/SnookerClub.Infrastructure`: SQLite repositories, transactions, migrations, exports.
- `tests/SnookerClub.Core.Tests`: billing, transitions, and correction rules.
- `tests/SnookerClub.Infrastructure.Tests`: durability, stock atomicity, and recovery.

Core must not depend on WPF or SQLite. Desktop composes the services; infrastructure
implements storage interfaces. Create projects as needed for each milestone rather
than implementing future modules early. Database schema work begins in M2.

## M1 — desktop foundation

Create a dark WPF shell with seven standard table cards, the wider Private Room,
and a non-bookable Counter in the specified physical layout. Add Inventory,
Analysis, and Settings navigation and reusable card controls. Empty table
left-click opens the start-session form; right-click does nothing. Until M2,
forms must clearly indicate that session creation is not yet available and must
not pretend to save records. Advanced modules are labeled Coming later.

Validate by building the desktop project, then running Windows smoke checks:
verify layout, readable labels, resize behavior, left/right clicks, navigation,
and return to the dashboard. Test on Windows 10 and Windows 11 before claiming
compatibility. Business-rule tests start alongside actual business logic.

## Remaining decisions and validation

Production prices, partial-minute billing, Private Room extension/overrun rules,
backup policy, and detailed later analytics remain open. No arbitrary production
prices should be introduced. Completed-minute rounding and an explicitly purchased
additional room hour are provisional policies, to be isolated and tested later.

The development environment is Linux. Project-local .NET SDK 10.0.402 is installed
for cross-compilation; run GUI checks on Windows. Confirm exact Windows
edition/build and processor architecture before packaging. Choose and document
the runtime version after checking its current compatibility/support matrix:
https://learn.microsoft.com/en-us/dotnet/core/install/windows

M1 source implements the shell, reusable cards, navigation, and disabled session
form previews. Build/publish results are recorded in the M1 smoke-test document.
Windows GUI verification remains pending. Existing deleted legacy documentation
is left as found; the current blueprint is authoritative.
