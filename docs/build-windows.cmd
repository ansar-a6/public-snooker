@echo off
setlocal
rem Resolve the repository from this script, independent of the current folder.
pushd "%~dp0.." || exit /b 1

where dotnet >nul 2>&1
if errorlevel 1 goto missing_sdk
dotnet --version
if errorlevel 1 goto missing_sdk

echo Building the self-contained Windows x64 application...
echo The first build needs internet access to download dependencies.
dotnet publish "src\SnookerClub.Desktop\SnookerClub.Desktop.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -o "artifacts\win-x64"
if errorlevel 1 goto build_failed
if not exist "artifacts\win-x64\SnookerClub.exe" goto build_failed

echo.
echo Build succeeded. Your executable is:
echo "%CD%\artifacts\win-x64\SnookerClub.exe"
popd
exit /b 0

:missing_sdk
echo.
echo Install the .NET 10 SDK for Windows, then reopen your terminal:
echo https://dotnet.microsoft.com/en-us/download/dotnet/10.0
echo Choose SDK, not just Runtime. Visual Studio is not required.
popd
exit /b 1

:build_failed
echo.
echo Build failed. Read the error above; do not use an older executable as this build's result.
popd
exit /b 1
