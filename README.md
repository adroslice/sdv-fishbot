# Fishbot
This is a Stardew Valley mod that can automate the following tasks in a way that doesn't feel cheaty:
- Casting your fishing rod by a configurable maximum distance
- Reeling in a fish when it nibbles
- **Emulate a real player in playing the fishing minigame by using a sophisticated strategy, catching fish and treasure**
- Stow away your catch
- Loot any caught treasure
- Eat the best available food when falling below 10 energy with a configurable max sGPE (Sell-Price in G per Energy)
- Pause (by opening the menu) when falling below 10 energy if auto-eating is disabled or no good food is available
- Pause once per day after a configurable time

Any of these can be disabled individually in the configuration (GMCM is supported), much like the handy little **Bubble Radar** that'll point you to success.

A configurable hotkey (F5 by default) is used to toggle the enabled automations. You must have your fishing rod equipped for this. The default options should serve most players very well!

## About Strategies: Help wanted!
If you can come up with a better control system than the following strategy, please reach out! I would love to add more options to fit every play situation, though the current algorithm already gets pretty close to "perfect human" play, except that it doesn't addapt based on difficulty.

## About the Strategy: FishingAutomatonPlus
I developed this mod because I wanted an auto-fisher for my future min/max journeys that performed well without feeling cheaty. Most of the auto-fisher mods I have looked at basically just skip the minigame and guarantee you a perfect catch and treasure, but that's simply way too overpowered.

I did come across one mod that set out to achieve what I wanted it to: [Fishing Automaton](https://www.nexusmods.com/stardewvalley/mods/2527?tab=posts). Unfortunately, this mod has been broken for quite a while, and it seems unlikely that the Author will pick it back up, but huge credits to Drynwynn. **The main strategy implemented now is named FishingAutomatonPlus, because it is a heavily modified version of the strategy used in that mod**. I will mark [E]nhancements I've made in the following full description.

All significant parameters can be modified through the configuration, though the default values should be fairly well-tuned.

### Here's how it works:
1. Decide to prioritize the fish or the treasure based on two configurable capture progress thresholds
2. Determine a Target Position:
  - [E] Go for both if fish and treasure fit within a configurable percentage of the fishing bar
  - Go for treasure if prioritized
  - Otherwise, target the fish with the following adjustments:
    - [E] A configurable number of frames of continued linear movement of the fish predicted (But never overshooting the fishs own internal target position)
    - [E] A bias towards the center of the track because further down a fish is more likely to move up, with a configurable padding to keep the fish well within the fishing bar's target area
3. Apply these rules in sequence:
   - If the *fishing bar is exceeding a configurable maximum velocity*, **brake**
   - If the *target is already inside the bar*, and it is *going faster than a target velocity* that scales down depending on how close it is (using a parabolic function), **brake**
   - If the *fishing bar approaching the bottom edge*, and is *exceeding another configurable speed limit*, **brake**
   - Finally, *if all other conditions were false*, **accelerate towards the target** position.

### Here's how it performs:
I have tested this strategy on a rainy day of early spring, starting on fishing level 5 and going all the way up to level 8 across 300 catches. While in a small window, it never missed **any** catches, and got ~15% perfect catches on Catfish. Unless it was going for treasure, it managed to perfect catch almost every chub.
On a max-luck day, with fishing level 10 and nothing more than a fiberglass rod and regular bait, it managed to perfect catch 36% of catfish.

I am still investigating why having the game in fullscreen seems to negatively affect the performance of this strategy. At the moment, I can only assume it's an issue of reduced update rates.

## About Control
I set myself a goal of not using Harmony patches for this mod. While I've succeeded so far, I should note that the way I'm **directly** emulating key presses through SMAPIs internal method has the downside of not working in the background.

## Future Plans
These features are on my radar, but won't be implemented unless they're requested or I find a personal need for them.
- Add other selectable minigame strategies should I discover a better algorithm
- Auto-Buff with Jellies and other fishing/luck foods
- Diagnostic Overlay for developing or just appreciating the algorithm at work
- Another way of simulating input. The current method only seems to work when the game window has focus, and it would be nice if it could run in the background.
- Configurable delays for auto-cast, auto-hit and looting treasure
- A way to offset the fishing rod cast (for hitting tricky bubble spots)
