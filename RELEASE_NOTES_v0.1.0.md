# AC Kill XP v0.1.0

Initial public release of AC Kill XP.

## Included

- XP-per-kill display in the normal Asheron's Call chat window
- No HUD or overlay
- No XP/hour tracking
- No database
- No combat automation
- Enable/disable, status, test, and debug chat commands
- Source code
- Self-contained installer/uninstaller builder

## Verified

v0.1.0 was built successfully on Windows with Decal 3.0, installed successfully, loaded in Asheron's Call, and displayed the correct XP after a real creature kill on an ACE server.

## Commands

```text
/killxp on
/killxp off
/killxp status
/killxp test
/killxp debug on
/killxp debug off
```

## Build

Run:

```text
BUILD RELEASE.cmd
```

The installer will be written to:

```text
release\AC Kill XP Setup v0.1.0.exe
```
