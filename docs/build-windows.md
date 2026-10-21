# Build the Windows executable

1. Install the **.NET 10 SDK for Windows** from
   [Microsoft](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).
   Choose the SDK installer matching your build computer. Visual Studio is not
   required. Reopen your terminal after installing.
2. Open PowerShell or Command Prompt in the repository's `docs` folder.
3. Run:

   ```powershell
   .\build-windows.cmd
   ```

From the repository root, run `.\docs\build-windows.cmd` instead. The script
locates the project relative to itself and also works with spaces in the path.
Keep the script inside the full repository; copying it alone is not enough.

The first build needs internet access to restore Microsoft's dependency packages.
After a successful build, the executable is:

```text
artifacts\win-x64\SnookerClub.exe
```

Double-click that file on a Windows x64 computer. It includes the .NET runtime,
so the computer running the app does not need the SDK or a separate .NET install.
Close the application before rebuilding if Windows reports that the output is
locked. This is the M2 preview: configure rates in Settings, then start test sessions.
Session saving is enabled; billing and checkout are not yet available. See
[M2 verification](M2-smoke-tests.md).

The script uses Microsoft's
[`dotnet publish`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish)
command to create a self-contained, single-file executable. It returns a nonzero
exit code if the SDK is missing or publishing fails.

Generated files under `artifacts`, `bin`, and `obj` are deliberately ignored by
Git, so cloning the repository brings the source and build script, not the `.exe`.
Linux can cross-compile this Windows executable; running and visually testing
the WPF interface requires Windows.

The publish options were previously verified on Linux. The `.cmd` launcher itself
still needs execution on Windows.
