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
