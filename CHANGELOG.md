# Changelog

## v0.1.0 — 2026-10-07

First public build.

- Displays XP gained from a creature kill in the normal Asheron's Call chat window.
- Pairs killing-blow combat text with the matching `CharacterFilter.TotalXP` increase.
- Uses a 2.5-second pairing window so either event can arrive first.
- Includes `/killxp on`, `off`, `status`, `test`, and debug commands.
- No HUD, XP/hour tracker, database, or combat automation.
- Self-contained Windows installer and uninstaller build process.
- Built and successfully tested with a real creature kill on an ACE server.
