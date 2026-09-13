#!/usr/bin/env bash
# Populates deps/ with the reference assemblies the plugin compiles against:
# Valheim dedicated-server managed DLLs (anonymous Steam download), the
# BepInEx core DLLs, ServersideQoL core, and YamlDotNet — all at the exact
# versions pinned on the game server.
set -euo pipefail
cd "$(dirname "$0")/.."
mkdir -p deps .cache

if [ ! -f deps/assembly_valheim.dll ]; then
  # NOTE: the NuGet package "DepotDownloader" is an unrelated Steam API
  # library, not the SteamRE/DepotDownloader CLI (no tools/ folder, not a
  # dotnet-tool package) -- `dotnet tool install DepotDownloader` fails.
  # Fetch the real CLI as a self-contained binary from its GitHub releases.
  DEPOTDOWNLOADER_VERSION="3.4.0"
  if [ ! -x .cache/tools/DepotDownloader/DepotDownloader ]; then
    case "$(uname -s)" in
      Linux) dd_os="linux" ;;
      Darwin) dd_os="macos" ;;
      *) echo "ERROR: unsupported OS for DepotDownloader: $(uname -s)" >&2; exit 1 ;;
    esac
    case "$(uname -m)" in
      x86_64|amd64) dd_arch="x64" ;;
      arm64|aarch64) dd_arch="arm64" ;;
      *) echo "ERROR: unsupported arch for DepotDownloader: $(uname -m)" >&2; exit 1 ;;
    esac
    dd_zip=".cache/depotdownloader-${dd_os}-${dd_arch}.zip"
    curl -fsSL -o "$dd_zip" "https://github.com/SteamRE/DepotDownloader/releases/download/DepotDownloader_${DEPOTDOWNLOADER_VERSION}/DepotDownloader-${dd_os}-${dd_arch}.zip"
    rm -rf .cache/tools/DepotDownloader && mkdir -p .cache/tools/DepotDownloader
    unzip -oq "$dd_zip" -d .cache/tools/DepotDownloader
    chmod +x .cache/tools/DepotDownloader/DepotDownloader
  fi
  # NOTE: the actual depot layout is "valheim_server/Data/Managed/*.dll"
  # (not "valheim_server_Data" as the folder name might suggest) -- verified
  # against the depot 896663 manifest.
  printf 'regex:.*/Data/Managed/.*\\.dll\n' > .cache/filelist.txt
  .cache/tools/DepotDownloader/DepotDownloader -app 896660 -filelist .cache/filelist.txt -dir .cache/valheim
  find .cache/valheim -path '*/Data/Managed/*.dll' -exec cp {} deps/ \;
  [ -f deps/assembly_valheim.dll ] || { echo "ERROR: assembly_valheim.dll not found in Steam depot download" >&2; exit 1; }
fi

fetch_thunderstore() { # <url> <dllname> <marker-file>
  local url="$1" dll="$2" zip=".cache/$2.zip" extract=".cache/extract-$2"
  [ -f "deps/$dll" ] && return 0
  curl -fsSL -o "$zip" "$url"
  rm -rf "$extract" && mkdir -p "$extract"
  unzip -oq "$zip" -d "$extract"
  find "$extract" -name "$dll" -exec cp {} deps/ \;
  [ -f "deps/$dll" ] || { echo "ERROR: $dll not found in $url" >&2; exit 1; }
}

# BepInEx pack ships several core DLLs; extract them all from core/
if [ ! -f deps/BepInEx.dll ]; then
  curl -fsSL -o .cache/bepinex.zip "https://thunderstore.io/package/download/denikson/BepInExPack_Valheim/5.4.2350/"
  rm -rf .cache/extract-bepinex && mkdir -p .cache/extract-bepinex
  unzip -oq .cache/bepinex.zip -d .cache/extract-bepinex
  find .cache/extract-bepinex -path '*/BepInEx/core/*.dll' -exec cp {} deps/ \;
  [ -f deps/BepInEx.dll ] || { echo "ERROR: BepInEx.dll not extracted" >&2; exit 1; }
fi

fetch_thunderstore "https://thunderstore.io/package/download/ArgusMagnus/ServersideQoL/2.0.6/" ServersideQoL.dll
fetch_thunderstore "https://thunderstore.io/package/download/ValheimModding/YamlDotNet/16.3.1/" YamlDotNet.dll

echo "deps/ ready:"
ls deps/ | head -30
