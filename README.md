# RimWorldRPC

Discord Rich Presence for **RimWorld 1.7 (unstable)** and .NET 10.
This branch targets reference assemblies for **1.7.5014-unstable**.
Earlier game versions require an earlier mod release.

Shows the colony name, colonist count, days survived, hour, quadrum, calendar year,
biome, and elapsed session time. Configure each field or enter custom top
and bottom text in the mod settings.

## Installation

1. Download `RWRichPresence.zip` from a release of this fork.
2. Extract the `RimWorldRPC` folder into RimWorld's `Mods` directory.
3. Enable the mod and run the Discord desktop client with activity sharing enabled.

The release includes `RimRPC.dll`, `DiscordRPC.dll`, and `Newtonsoft.Json.dll`.
No native `0discord-rpc` library or Harmony mod is required.

## Build

Install the .NET 10 SDK, then run from the repository root:

```sh
dotnet build RimRPC.slnx -c Release
```

The complete mod is in `RimWorldRPC/`; generated assemblies go to
`RimWorldRPC/1.7/Assemblies/` and are ignored by Git. Game reference assemblies
are used only for compilation and are never shipped with the mod.
Debug builds use `Sources/bin/Debug/`.

## Usage

The status updates on entering a game, changing the active map, returning to
menu, and at a configurable interval of 5–120 real seconds (including while
paused). The default is 20 seconds; old tick-based intervals migrate to seconds. Use **Update** in mod settings to apply text changes
immediately. The Discord connection retries when the desktop client is unavailable.

The updater subscribes directly to Waylith's application update and shutdown
hooks, so it does not require a Unity GameObject or physics callback.
Existing released settings keep their original mod handle and serialized type name.

## Credits

Original mod by [Weilbyte](https://github.com/Weilbyte/RWRichPresence), with
contributions by Jdalt40. Discord IPC uses
[Lachee's DiscordRichPresence](https://github.com/Lachee/discord-rpc-csharp).

[Original Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=1463057070)

## Custom Discord application

The optional Application ID setting uses the default application when blank.
Enter your own numeric ID and press the connection Apply / Retry button to reconnect without
restarting the game. Clear the field and apply to restore the default application.
By default, custom applications use image asset keys `logo` and `inmap`.
Change the large and small image keys in settings to match your application.

## Connection and preview

Connection settings have their own Apply / Retry button. The ID is kept as a
draft until accepted by the format check; only Discord's Ready response marks
it connected. Failures display their reason and retry automatically after
5–60 seconds. You can disable RPC without restarting the game.

The text preview uses exactly the same UTF-8 truncation as outgoing presence.
Large and small image asset keys are configurable; empty keys hide the image.
Reset restores all defaults, including the application ID and image keys.
In world view the calendar uses the first home map's location. If no map or
home map is available, local hour and quadrum are omitted.

## Tests

```sh
dotnet test Tests/RimRPC.Tests.csproj -c Release
python3 Tests/validate_package.py
```

The separate xUnit project links production connection, formatting, and settings
logic. Settings serialization and translation use small API stubs because game
reference assemblies contain no executable game code. Discord reconnection, runtime
hooks, and UI still require testing inside RimWorld 1.7.
CI builds the solution, runs xUnit, and validates mod layout and translations.
Steam publishing remains manual.
