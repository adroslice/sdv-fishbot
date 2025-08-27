namespace Fishbot;

using System.Text.RegularExpressions;
using StardewModdingAPI;

public sealed class ModConfig
{
    // Keybinds
    public SButton ToggleAutomationKey { get; set; } = SButton.F5;

    // Core Automations
    public bool DoAutoCast { get; set; } = true;
    public bool DoAutoHit { get; set; } = true;
    public bool DoAutoPlay { get; set; } = true;
    public bool DoAutoStow { get; set; } = true;
    public bool DoAutoLoot { get; set; } = true;

    // Secondary Automations
    public bool DoAutoEat { get; set; } = true;

    // Automation Options
    public float MaxCastPercentage { get; set; } = 1f;
    public float MaxSGPE { get; set; } = 1.93f; // 125/65e for Iridium Chub and Fisher profession
    public int PauseAfterTime { get; set; } = 0130;
    public bool AlwaysEnabled { get; set; } = false;

    // Utility
    public bool EnableBubbleRadar { get; set; } = false;

    // FishingAutomaton+ Strategy Parameters
    public int FA_PredictLinearFrames { get; set; } = 5;
    public float FA_RelativeOffsetBarPercentage { get; set; } = 0.7f;
    public float FA_DualTargetingBarPercentage { get; set; } = 0.8f;
    public float FA_PrioTreasureAbove { get; set; } = 0.70f;
    public float FA_PrioFishBelow { get; set; } = 0.35f;
    public float FA_MinBarTargetVelocity { get; set; } = 1.0F;
    public float FA_MaxBarVelocityNearBottom { get; set; } = 2.0F;
    public float FA_MaxBarVelocity { get; set; } = 6.0F;
    public float FA_BottomThreshold { get; set; } = 30f;

    public static string FormatPercentage(float x) => $"{x * 100}%";
    public static void SetupConfigOptions(IGenericModConfigMenuApi configMenu, IManifest mod)
    {
        configMenu.AddSectionTitle(mod: mod, text: () => "Keybinds");
        configMenu.AddKeybind(
            mod: mod,
            name: () => "Toggle Automation",
            tooltip: () => "Pressing this key toggles the fishing automation on and off.",
            getValue: () => ModEntry.Config.ToggleAutomationKey,
            setValue: value => ModEntry.Config.ToggleAutomationKey = value
        );

        configMenu.AddSectionTitle(mod: mod, text: () => "Automations", tooltip: () => "Here you can configure what you would like Fishbot to automate for you when enabled.");
        configMenu.AddBoolOption(
            mod: mod,
            name: () => "Auto-Cast",
            getValue: () => ModEntry.Config.DoAutoCast,
            setValue: value => { ModEntry.Config.DoAutoCast = value; }
        );
        configMenu.AddBoolOption(
            mod: mod,
            name: () => "Auto-Hit",
            getValue: () => ModEntry.Config.DoAutoHit,
            setValue: value => { ModEntry.Config.DoAutoHit = value; }
        );
        configMenu.AddBoolOption(
            mod: mod,
            name: () => "Auto-Play Minigame",
            getValue: () => ModEntry.Config.DoAutoPlay,
            setValue: value => { ModEntry.Config.DoAutoPlay = value; }
        );
        configMenu.AddBoolOption(
            mod: mod,
            name: () => "Auto-Stow Fish",
            getValue: () => ModEntry.Config.DoAutoStow,
            setValue: value => { ModEntry.Config.DoAutoStow = value; }
        );
        configMenu.AddBoolOption(
            mod: mod,
            name: () => "Auto-Loot Treasure",
            getValue: () => ModEntry.Config.DoAutoLoot,
            setValue: value => { ModEntry.Config.DoAutoLoot = value; }
        );
        configMenu.AddBoolOption(
            mod: mod,
            name: () => "Auto-Eat",
            getValue: () => ModEntry.Config.DoAutoEat,
            setValue: value => { ModEntry.Config.DoAutoEat = value; },
            tooltip: () => "When disabled, automatically pause the game at low energy."
        );
        configMenu.AddNumberOption(
            mod: mod,
            name: () => "Max Cast Percentage",
            getValue: () => ModEntry.Config.MaxCastPercentage,
            setValue: value => { ModEntry.Config.MaxCastPercentage = value; },
            min: 0.01f,
            max: 1f,
            interval: 0.01f,
            formatValue: FormatPercentage
        );
        configMenu.AddNumberOption(
            mod: mod,
            name: () => "Auto-Eat Max sGPE",
            getValue: () => ModEntry.Config.MaxSGPE,
            setValue: value => { ModEntry.Config.MaxSGPE = value; },
            min: 0f,
            max: 5f,
            interval: 0.01f,
            tooltip: () => "Will pause instead of eating if all available food exceeds this Sell Price to Energy threshold. (e.g. 1.93 for an iridium chub with the Fisher profession)"
        );
        configMenu.AddNumberOption(
            mod: mod,
            name: () => "Pause after",
            getValue: () => ModEntry.Config.PauseAfterTime % 100 + ModEntry.Config.PauseAfterTime / 100 * 60,
            setValue: value => ModEntry.Config.PauseAfterTime = value % 60 + value / 60 * 100,
            tooltip: () => "Once per day, pause after this time (24h format). Use 02:00 to disable.",
            min: 0,
            max: 24 * 60,
            interval: 10,
            formatValue: value => $"{value / 60:D2}:{value % 60:D2}"
        );
        configMenu.AddBoolOption(
            mod: mod,
            name: () => "Always Enabled",
            getValue: () => ModEntry.Config.AlwaysEnabled,
            setValue: value => { ModEntry.Config.AlwaysEnabled = value; },
            tooltip: () => "Always run selected automations, without showing the HUD message. Automatic Casting must be toggled seperately to avoid getting stuck in a fishing loop."
        );

        configMenu.AddSectionTitle(mod: mod, text: () => "Utility", tooltip: () => "Little features to improve your fishing experience.");
        configMenu.AddBoolOption(
            mod: mod,
            getValue: () => ModEntry.Config.EnableBubbleRadar,
            setValue: value => ModEntry.Config.EnableBubbleRadar = value,
            name: () => "Bubble Radar",
            tooltip: () => "Indicates the direction of active bubbles on with an arrow near your character."
        );

        configMenu.AddSectionTitle(mod: mod, text: () => "Minigame Strategies", tooltip: () => "Here you can decide on which strategy to use and fine-tune its values.");
        configMenu.AddPageLink(
            mod: mod,
            pageId: nameof(ModEntry.MinigameStrategyFishingAutomatonPlus),
            text: () => "FishingAutomatonPlus Strategy",
            tooltip: () => "You can fine tune the variables of this fishing strategy here."
        );

        configMenu.AddPage(mod: mod, pageId: nameof(ModEntry.MinigameStrategyFishingAutomatonPlus), pageTitle: () => "FishingAutomatonPlus Strategy");
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_PredictLinearFrames,
            setValue: value => ModEntry.Config.FA_PredictLinearFrames = value,
            name: () => "Predict N Linear Frames",
            tooltip: () => "Target fish position in N frames of continued linear movement. Use 0 to disable. (inaccurate, but helps track fast targets)",
            min: 0,
            max: 20,
            interval: 1
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_RelativeOffsetBarPercentage,
            setValue: value => ModEntry.Config.FA_RelativeOffsetBarPercentage = value,
            name: () => "Center Bias Bar Allowance %",
            tooltip: () => "Keep target within the given portion of the fishing bar when offsetting towards the center of the track. 0 keeps target centered. (Fish are more likely to move towards the center)",
            min: 0.0f,
            max: 1f,
            interval: 0.01f,
            formatValue: FormatPercentage
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_DualTargetingBarPercentage,
            setValue: value => ModEntry.Config.FA_DualTargetingBarPercentage = value,
            name: () => "Dual Target Bar Allowance %",
            tooltip: () => "Target fish and treasure if they fit within the given portion of the fishing bar. Use 0.0 to avoid dual targeting.",
            min: 0.0f,
            max: 1f,
            interval: 0.01f,
            formatValue: FormatPercentage
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_PrioTreasureAbove,
            setValue: value => ModEntry.Config.FA_PrioTreasureAbove = value,
            name: () => "Prio. Treasure Above",
            tooltip: () => "Prioritize treasure if the capture progress exceeds this threshold. Use 1.0 to ignore treasure.",
            min: 0.0f,
            max: 1.0f,
            interval: 0.01f,
            formatValue: FormatPercentage
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_PrioFishBelow,
            setValue: value => ModEntry.Config.FA_PrioFishBelow = value,
            name: () => "Prio. Fish Below",
            tooltip: () => "Prioritize fish if the capture progress falls below this threshold. This value needs to be smaller than FA_PrioTreasureAbove.",
            min: 0.0f,
            max: 1.0f,
            interval: 0.01f,
            formatValue: FormatPercentage
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_MinBarTargetVelocity,
            setValue: value => ModEntry.Config.FA_MinBarTargetVelocity = value,
            name: () => "Min. Tracking Speed",
            tooltip: () => "Minimum target velocity when tracking fish. Applied when bar is on target.",
            min: 0.0f,
            max: 5.0f,
            interval: 0.01f
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_MaxBarVelocityNearBottom,
            setValue: value => ModEntry.Config.FA_MaxBarVelocityNearBottom = value,
            name: () => "Max. Speed Near Bottom",
            tooltip: () => "Maximum bar velocity towards bottom edge when near it, lower values mitigate bouncing.",
            min: 0.0f,
            max: 5.0f,
            interval: 0.01f
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_MaxBarVelocity,
            setValue: value => ModEntry.Config.FA_MaxBarVelocity = value,
            name: () => "Max. Speed",
            tooltip: () => "Maximum fishing bar velocity, lower values mitigate overshoot.",
            min: 0.0f,
            max: 10.0f,
            interval: 0.01f
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_BottomThreshold,
            setValue: value => ModEntry.Config.FA_BottomThreshold = value,
            name: () => "Bottom Edge Threshold",
            tooltip: () => "How far from the edge the Max Speed Near Bottom starts to take effect, higher values mitigate bouncing.",
            min: 0.0f,
            max: 100.0f,
            interval: 1f
        );
    }
}
