# ServersideQoL.AutoFeed

A [ServersideQoL](https://thunderstore.io/package/ArgusMagnus/ServersideQoL/) module for Valheim dedicated servers that automatically feeds **tamed** animals from nearby chests, so bases with penned livestock don't need manual feeding runs.

## What it does

When a tamed creature (a taming that has completed — wild or in-progress tames are left alone) goes hungry, the mod searches containers within `ContainerRange` meters of it for an item on that creature's food list. If a match is found, one unit is consumed from the container, the creature's hunger timer is reset, and the search stops for that creature until it's hungry again. If no matching food is found nearby, the check is retried periodically.

Everything happens **entirely server-side** inside the ServersideQoL processor pipeline — there is no client mod, no BepInEx requirement on connecting players, and vanilla or console clients need nothing installed to benefit from it.

## Requirements

- A Valheim **dedicated server** running [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)
- [`ArgusMagnus-ServersideQoL`](https://thunderstore.io/package/ArgusMagnus/ServersideQoL/) **version 2.0.6** (this module is built and pinned against that exact release)

## Configuration

Settings live in `BepInEx/config/tayvenb13.ServersideQoL.AutoFeed.cfg`, under the `[AutoFeed]` section:

| Key | Default | Description |
|---|---|---|
| `Enabled` | `false` | Enables/disables the entire mod |
| `ContainerRange` | `10` | Radius in meters around a hungry tamed creature in which containers are searched for its food |

The mod ships disabled by default — set `Enabled = true` after installing to turn it on.

## Installation

Download the release zip and unpack it as you would any other ServersideQoL module — the zip is laid out Thunderstore-style, with `manifest.json` at the root alongside `ServersideQoL.AutoFeed.dll` and `AutoFeed.Core.dll`.

## Credits & provenance

- The idea for auto-feeding tamed animals from nearby containers comes from [Stephen-Cherry/AutoFeed](https://github.com/Stephen-Cherry/AutoFeed). No code from that project is reused here — it's a client-side mod with no license file, and this is an independent, from-scratch, server-side reimplementation of the concept.
- This module is built on top of [ArgusMagnus/ValheimServersideQoL](https://github.com/ArgusMagnus/ValheimServersideQoL), which has no license file in its upstream repository. This module links against ServersideQoL's released DLL as a build/runtime dependency and is published for personal-server use; it does not redistribute or modify ServersideQoL's own code.

## Development

See `scripts/fetch-deps.sh` to populate `deps/` with the pinned reference assemblies (Valheim server managed DLLs, BepInEx, ServersideQoL, YamlDotNet) and `scripts/package.sh <version>` to build a release-ready zip.
