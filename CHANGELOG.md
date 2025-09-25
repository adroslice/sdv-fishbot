# Version History
## Minor Release 0.0.x: Alpha
### Patch 0.0.1
- Initial Release

### Patch 0.0.2
- Fixed mod options not applying due to an oversight when I refactored the configuration

### Patch 0.0.3
- Fixed mod options never getting loaded from the file
- Improved input simulation performance by avoiding repeated reflection

## Minor Release 0.1.0: Beta
- Fixed fishbot not casting again sometimes after failing a catch
  - This was the last known major issue that kept fishbot in alpha
- FishingAutomatonPlus Strategy:
  - Gave strategy parameters more descriptive display names and some better descriptions
  - Allowed dual targeting to take effect while prioritizing fish, can help perfect-catch-with-treasure more often and matches human behavior better

## Minor Release 0.2.0
- Added Auto-Eating (and Auto-Pausing when disabled) below 10 energy, with a configurable max Sell Price to Energy threshold
- Added Auto-Pausing after a configurable time
- Added a Max Cast Distance parameter (useful for min-casting with a training rod on day 2)
- Turned bubble radar off by default to avoid further cluttering the UI of users with many mods

### Patch 0.2.1
- Added formatting for percentage-based settings
- Fixed regression that caused the fishing bar to stick to the top because it thinks there's a treasure there
- Made Fishbot disable itself on a new morning to avoid auto casting after passing out
- Added an experimental backgrounding fix, pending a way to simulate mouse position while the window is inactive

### Patch 0.2.2
- Added an "Always Enabled" option for everything except auto casting

### Patch 0.2.3
- Fixed "Always Enabled" with "Auto-Loot Treasure" trying to always grab items from regular chests

### Patch 0.2.4
- Added Support for Iconic Framework

## Minor Release 0.3.0
- Overhauled Input Simulation (removing key simulation in favor of harmony patches) to fix:
  - Needing to ensure mouse is positioned away from the toolbar as it would otherwise get the input
  - Immediately disabling itself when clicking the Iconic Framework Icon
  - Most if not all "stuck input" issues
  - Not respecting keybinds (becoming useless when rebinding use tool)
- Minor performance improvement

### 0.3.1
- Added i18n support and an official german translation
- Added an option to eat the first food item in your inventory instead of the one with the highest sGPE
