AC KILL XP
Version 0.1.0 TEST BUILD
Publisher: EWARAC

PURPOSE
-------
A deliberately small Asheron's Call Decal plugin.

When the plugin detects one of AC's normal "you killed it" combat messages and
sees the character's total XP increase at the same time, it writes one line to
the normal AC chat window:

    [Kill XP] 12,345,678 XP

It does not create a HUD, track XP/hour, write a database, or automate combat.

HOW IT WORKS
------------
The plugin watches:
  1. AC combat text for the player's killing-blow messages.
  2. Decal CharacterFilter.TotalXP for the matching XP increase.

The two events may arrive in either order, so v0.1.0 keeps each event for up to
2.5 seconds and pairs them.

This deliberately avoids printing ordinary quest / hand-in XP as "kill XP"
because an XP increase is only shown when a killing-blow message is also seen.

COMMANDS
--------
/killxp on
/killxp off
/killxp status
/killxp test
/killxp debug on
/killxp debug off

The plugin is ON by default.

TEST
----
After installation and login:

  /killxp test

should print:

  [Kill XP] 12,345,678 XP

Then kill a normal creature. If its killing-blow text matches the current
v0.1.0 pattern set, the actual XP award should appear immediately afterwards.

If a kill does not produce an XP line, turn on:

  /killxp debug on

and repeat the kill. The debug output will tell us whether the plugin saw the
kill marker and the XP change separately.

BUILD
-----
This source package follows the same proven build style as AC Big Cursor.

Requirements on the Windows PC:
  - Decal 3.0 installed
  - .NET Framework 4.x compiler present

Double-click:

  BUILD RELEASE.cmd

The finished installer will be created at:

  release\AC Kill XP Setup v0.1.0.exe

INSTALL LOCATION
----------------
C:\Games\Decal Plugins\AC Kill XP

DECAL REGISTRATION
------------------
Plugin GUID:
{4EDC248A-E237-4516-A17C-5099F515ADC9}

Object:
ACKillXP.PluginCore

Assembly:
ACKillXP.dll

NOTES
-----
This is intentionally a first test build. AC has many different killing-blow
phrases. The initial pattern set covers the common retail-style messages used
by existing AC combat trackers, but emulator/server-specific text may require
adding a pattern after testing.

No settings file is created in v0.1.0. The plugin starts enabled each session.
