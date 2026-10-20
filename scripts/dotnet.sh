#!/usr/bin/env bash
set -euo pipefail
project_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
export DOTNET_ROOT="$project_root/.dotnet"
export DOTNET_CLI_HOME="$project_root/.dotnet-home"
export NUGET_PACKAGES="$project_root/.nuget/packages"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
if [[ ! -x "$DOTNET_ROOT/dotnet" ]]; then
  echo "Local SDK missing. See README.md for installation instructions." >&2
  exit 1
fi
exec "$DOTNET_ROOT/dotnet" "$@"
