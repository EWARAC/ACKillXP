# AC Kill XP

A lightweight **Asheron's Call Decal plugin** that displays the XP gained from each creature kill directly in the normal in-game chat window.

## What it does

When AC Kill XP detects one of Asheron's Call's normal killing-blow combat messages and sees the character's total XP increase at the same time, it prints a single line such as:

```text
[Kill XP] 12,345,678 XP
```

It does **not** create a HUD, track XP/hour, write a database, or automate combat.

## Status

**v0.1.0** is the first public build. It has been built, installed, loaded by Decal, and successfully tested with a real creature kill on an ACE server.

## Commands

```text
/killxp on
/killxp off
/killxp status
/killxp test
/killxp debug on
/killxp debug off
```

The plugin starts **ON** each session.

`/killxp test` prints:

```text
[Kill XP] 12,345,678 XP
```

## How it works

AC Kill XP watches two things:

1. AC combat chat for the player's killing-blow message.
2. `CharacterFilter.TotalXP` for the corresponding increase in total XP.

The events can arrive in either order, so v0.1.0 keeps each event for up to **2.5 seconds** and pairs them.

This helps avoid reporting ordinary quest or hand-in XP as kill XP, because an XP increase is only displayed when a killing-blow message is also detected.

## Building

Requirements:

- Windows
- Decal 3.0
- .NET Framework 4.x
- 32-bit .NET Framework C# compiler

Double-click:

```text
BUILD RELEASE.cmd
```

The finished installer is created at:

```text
release\AC Kill XP Setup v0.1.0.exe
```

The build targets **.NET Framework 4.8 / x86**.

## Installation

The self-contained installer installs to:

```text
C:\Games\Decal Plugins\AC Kill XP
```

Decal plugin GUID:

```text
{4EDC248A-E237-4516-A17C-5099F515ADC9}
```

Plugin object:

```text
ACKillXP.PluginCore
```

## Compatibility notes

Asheron's Call has many killing-blow phrases. The current pattern set covers common retail-style messages used by existing AC combat trackers. Emulator- or server-specific kill text may require an additional pattern.

If a kill does not show XP, use:

```text
/killxp debug on
```

and repeat the kill to see whether the plugin detected the kill marker and XP change separately.

## Publisher

**EWARAC**
