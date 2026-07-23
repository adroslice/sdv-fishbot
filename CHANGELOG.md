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

### Patch 0.3.1
- Added i18n support and an official german translation
- Added an option to eat the first food item in your inventory instead of the one with the highest sGPE

### Patch 0.3.2
- Switched from single keybind to KeybindList to allow for key combinations
- Fixed Max sGPE not being applied when Eat First Food was enabled
- Fixed getting stuck in the menu on low energy when auto-eat was off and always enabled was on by making the auto-pausing conditional on full enablement

## Minor Release 0.4.0
- Translation Changes
- Replaced "Always Enabled" with Automation Mode, including a new "Stealth" mode
- Merged Auto-Hit => Auto-Cast, Auto-Stow => Auto-Loot & Auto-Eat options
- Removed config categories, further config changes under consideration

## Minor Release 0.5.0
- Added an option to pause automatically after running out of bait, enabled by default
- Made failed casts pause automatically

### Patch 0.5.1
- Restored the option to separately configure Auto-Cast and Auto-Hit, which were previously merged to simplify the config
- Made Auto-Pause effects only trigger when full automation is enabled

### Minor Release 0.6.0
- Translation Changes
- Removed old auto pause on no bait setting
- Added customizable Auto-Attachment options for bait and tackle
- Auto-Pause effects now show a HUD message in "Always Enabled" but not active mode instead of doing nothing

### Patch 0.6.1
- Fixed Bubble Radar not being at the center of the screen at non-standard zoom levels
- Fixed buggy detection of invalid casts
- Added a config option to specify a list of foods to prioritize before eating the first/best food
