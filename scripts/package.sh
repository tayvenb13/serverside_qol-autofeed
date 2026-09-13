#!/usr/bin/env bash
# Builds the plugin at the given version and produces a Thunderstore-layout
# zip (manifest.json at root) that the server's ansible role can install.
set -euo pipefail
VERSION="${1:?usage: package.sh <version>}"
cd "$(dirname "$0")/.."

dotnet build -c Release -p:Version="$VERSION" src/ServersideQoL.AutoFeed/ServersideQoL.AutoFeed.csproj

STAGE="$(mktemp -d)"
BIN=src/ServersideQoL.AutoFeed/bin/Release/netstandard2.1
cp "$BIN/ServersideQoL.AutoFeed.dll" "$BIN/AutoFeed.Core.dll" README.md CHANGELOG.md "$STAGE/"

cat > "$STAGE/manifest.json" <<EOF
{
  "name": "ServersideQoL_AutoFeed",
  "version_number": "$VERSION",
  "website_url": "https://github.com/tayvenb13/serverside_qol-autofeed",
  "description": "Feeds tamed animals from nearby chests. Server-side only (ServersideQoL module).",
  "dependencies": ["ArgusMagnus-ServersideQoL-2.0.6"]
}
EOF

OUT="$PWD/ServersideQoL_AutoFeed-$VERSION.zip"
rm -f "$OUT"
(cd "$STAGE" && zip -r "$OUT" .)
echo "Wrote $OUT"
unzip -l "$OUT"
