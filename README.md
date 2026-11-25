# RimWorldRPC

Discord Rich Presence for RimWorld 1.7 (unstable) and .NET 10.

Shows your colony, colonists, date, biome, and session time in Discord. You can
choose which fields to display or set custom text in the mod settings.

## Installation

1. Download `RWRichPresence.zip` from the releases.
2. Extract `RimWorldRPC` into RimWorld's `Mods` directory and enable it.
3. Run the Discord desktop client with activity sharing enabled.

## Settings

Presence updates every 20 seconds by default. You can change the interval,
update the status manually, or disable the integration in the mod settings.
The mod reconnects automatically if Discord is unavailable.

You can also use a custom Discord Application ID and image keys. Leave the ID
empty to use the default application. Settings from older releases are not
migrated.

## Build and test

Install the .NET 10 SDK and run:

```sh
dotnet build RimRPC.slnx -c Release
dotnet test Tests/RimRPC.Tests.csproj -c Release
```

The release build is written to `RimWorldRPC/1.7/Assemblies/`. Runtime behavior
and the settings UI require testing in RimWorld.

## Credits

Original mod by [Weilbyte](https://github.com/Weilbyte/RWRichPresence), with
contributions by Jdalt40. Discord integration uses
[DiscordRichPresence](https://github.com/Lachee/discord-rpc-csharp).
