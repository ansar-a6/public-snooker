# Snooker Club Management System

An offline Windows 10/11 desktop application for one club owner to manage table
sessions, canteen purchases, inventory, bills, payments, and historical records.

## M1 status

Implemented: dark WPF dashboard, seven table cards in the physical layout,
Private Room, non-bookable Counter, navigation placeholders, and session form
previews. Session creation is disabled until M2 adds persistence. No production
prices or customer records are included.

C#/.NET 10 and WPF power the desktop application. SQLite starts in M2; Python is
optional future analytics. GUI runtime checks require Windows; see
[the smoke checklist](docs/M1-smoke-tests.md). Windows 10 edition/build and CPU
architecture must be checked before production deployment; Microsoft runtime
support is distinct from compatibility testing.

## Build on Windows

Install the [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0),
reopen your terminal, and run this from the repository root:

```powershell
.\docs\build-windows.cmd
```

Or run `.\build-windows.cmd` from inside `docs`. The result is
`artifacts\win-x64\SnookerClub.exe`. See [Windows build instructions](docs/build-windows.md).
Build outputs are ignored by Git; the executable is generated on your computer.

## Build on this Linux workspace

The SDK is installed locally in `.dotnet`; the wrapper also keeps NuGet packages
and CLI state inside ignored project folders. It does not change system PATH.

```bash
./scripts/dotnet.sh --info
./scripts/dotnet.sh build src/SnookerClub.Desktop -c Release
./scripts/dotnet.sh publish src/SnookerClub.Desktop -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -o artifacts/win-x64
```

The Windows x64 preview executable is `artifacts/win-x64/SnookerClub.exe`.
Copy it to Windows and double-click it. Native runtime libraries may extract to
the Windows user's temporary directory. No separate .NET installation is needed.
This preview creates no database. Future records will be stored separately from
the executable. Use the appropriate runtime identifier for other architectures.

To install the local SDK on a fresh Linux checkout:

```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/snooker-dotnet-install.sh
bash /tmp/snooker-dotnet-install.sh --channel 10.0 --install-dir "$PWD/.dotnet" --no-path
```

On Windows with the .NET 10 SDK installed, use `dotnet` in place of the wrapper:

```powershell
dotnet run --project src/SnookerClub.Desktop
```

## Development rules

Follow [the blueprint](docs/Snooker-Club-Management-System-Codex-Blueprint.md)
and [the implementation plan](docs/implementation-plan.md); later owner
clarifications take precedence. Work in small, tested milestones. Keep GUI,
business logic, and database access separate as those layers are introduced.
Preserve original events, correction history, and historical prices. Never commit
real customer data, databases, backups, or secrets. Daily operations must remain
offline without paid APIs or cloud dependencies.
