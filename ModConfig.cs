namespace Fishbot;

using StardewModdingAPI;
using StardewModdingAPI.Utilities;

public sealed class ModConfig
{
    // Keybinds
    public KeybindList ToggleAutomationKey { get; set; } = KeybindList.Parse("F5");

    // Mode
    public string AutomationMode { get; set; } = "toggle";

    // Core Automations
    public bool DoAutoCast { get; set; } = true;
    public bool DoAutoPlay { get; set; } = true;
    public bool DoAutoLoot { get; set; } = true;

    // Secondary Automations
    public string AutoEatMode { get; set; } = "best";

    // Automation Options
    public float CastDistance { get; set; } = 1f;
    public float MaxSGPE { get; set; } = 1.93f; // 125/65e for Iridium Chub and Fisher profession
    public int PauseAfterTime { get; set; } = 0130;

    // Utility
    public bool EnableBubbleRadar { get; set; } = false;
    public bool DoAutoPauseOnNoBait { get; set; } = true;

    // FishingAutomaton+ Strategy Parameters
    public int FA_PredictFrames { get; set; } = 5;
    public float FA_CenterBiasBarPercentage { get; set; } = 0.7f;
    public float FA_DualTargetBarPercentage { get; set; } = 0.8f;
    public float FA_PrioTreasureAbove { get; set; } = 0.70f;
    public float FA_PrioFishBelow { get; set; } = 0.35f;
    public float FA_MinBarTrackingSpeed { get; set; } = 1.0F;
    public float FA_MaxBarSpeedNearBottom { get; set; } = 2.0F;
    public float FA_MaxBarSpeed { get; set; } = 6.0F;
    public float FA_BottomThreshold { get; set; } = 30f;

    public static string FormatPercentage(float x) => $"{x * 100}%";
    public static void SetupConfigOptions(IGenericModConfigMenuApi configMenu, IManifest mod, ITranslationHelper t)
    {
        configMenu.AddKeybindList(
            mod: mod,
            name: () => t.Get("config.keybinds.toggle-automations.name"),
            tooltip: () => t.Get("config.keybinds.toggle-automations.tooltip"),
            getValue: () => ModEntry.Config.ToggleAutomationKey,
            setValue: value => ModEntry.Config.ToggleAutomationKey = value
        );

        configMenu.AddTextOption(
            mod: mod,
            name: () => t.Get("config.automations.mode.name"),
            tooltip: () => t.Get("config.automations.mode.tooltip"),
            allowedValues: new[] {
                "toggle",
                "always",
                "stealth",
            },
            formatAllowedValue: value => t.Get($"config.automations.mode.{value}.name"),
            getValue: () => ModEntry.Config.AutomationMode,
            setValue: value => ModEntry.Config.AutomationMode = value
        );

        configMenu.AddBoolOption(
            mod: mod,
            name: () => t.Get("config.automations.auto-cast.name"),
            getValue: () => ModEntry.Config.DoAutoCast,
            setValue: value => { ModEntry.Config.DoAutoCast = value; }
        );
        configMenu.AddBoolOption(
            mod: mod,
            name: () => t.Get("config.automations.auto-play.name"),
            getValue: () => ModEntry.Config.DoAutoPlay,
            setValue: value => { ModEntry.Config.DoAutoPlay = value; }
        );
        configMenu.AddBoolOption(
            mod: mod,
            name: () => t.Get("config.automations.auto-loot.name"),
            getValue: () => ModEntry.Config.DoAutoLoot,
            setValue: value => { ModEntry.Config.DoAutoLoot = value; }
        );
        configMenu.AddTextOption(
            mod: mod,
            name: () => t.Get("config.automations.auto-eat-mode.name"),
            tooltip: () => t.Get("config.automations.auto-eat-mode.tooltip"),
            allowedValues: new[] {
                "disabled",
                "first",
                "best",
            },
            formatAllowedValue: value => t.Get($"config.automations.auto-eat-mode.{value}.name"),
            getValue: () => ModEntry.Config.AutoEatMode,
            setValue: value => ModEntry.Config.AutoEatMode = value
        );
        configMenu.AddNumberOption(
            mod: mod,
            name: () => t.Get("config.automations.cast-distance.name"),
            getValue: () => ModEntry.Config.CastDistance,
            setValue: value => { ModEntry.Config.CastDistance = value; },
            min: 0.01f,
            max: 1f,
            interval: 0.01f,
            formatValue: FormatPercentage
        );
        configMenu.AddNumberOption(
            mod: mod,
            name: () => t.Get("config.automations.auto-eat-max-sgpe.name"),
            getValue: () => ModEntry.Config.MaxSGPE,
            setValue: value => { ModEntry.Config.MaxSGPE = value; },
            min: 0f,
            max: 5f,
            interval: 0.01f,
            tooltip: () => t.Get("config.automations.auto-eat-max-sgpe.tooltip")
        );
        configMenu.AddNumberOption(
            mod: mod,
            name: () => t.Get("config.automations.pause-after-time.name"),
            getValue: () => ModEntry.Config.PauseAfterTime % 100 + ModEntry.Config.PauseAfterTime / 100 * 60,
            setValue: value => ModEntry.Config.PauseAfterTime = value % 60 + value / 60 * 100,
            tooltip: () => t.Get("config.automations.pause-after-time.tooltip"),
            min: 0,
            max: 24 * 60,
            interval: 10,
            formatValue: value => $"{value / 60:D2}:{value % 60:D2}"
        );

        configMenu.AddBoolOption(
            mod: mod,
            getValue: () => ModEntry.Config.EnableBubbleRadar,
            setValue: value => ModEntry.Config.EnableBubbleRadar = value,
            name: () => t.Get("config.utility.bubble-radar.name"),
            tooltip: () => t.Get("config.utility.bubble-radar.tooltip")
        );

        configMenu.AddBoolOption(
            mod: mod,
            getValue: () => ModEntry.Config.DoAutoPauseOnNoBait,
            setValue: value => ModEntry.Config.DoAutoPauseOnNoBait = value,
            name: () => t.Get("config.utility.auto-pause-no-bait.name"),
            tooltip: () => t.Get("config.utility.auto-pause-no-bait.tooltip")
        );

        configMenu.AddSectionTitle(mod: mod, text: () => t.Get("config.minigame-strategies.name"), tooltip: () => t.Get("config.minigame-strategies.tooltip"));
        configMenu.AddPageLink(
            mod: mod,
            pageId: nameof(ModEntry.MinigameStrategyFishingAutomatonPlus),
            text: () => t.Get("config.minigame-strategies.fishing-automaton-plus.name"),
            tooltip: () => t.Get("config.minigame-strategies.any.tooltip")
        );

        configMenu.AddPage(mod: mod, pageId: nameof(ModEntry.MinigameStrategyFishingAutomatonPlus), pageTitle: () => t.Get("config.minigame-strategies.fishing-automaton-plus.name"));
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_PredictFrames,
            setValue: value => ModEntry.Config.FA_PredictFrames = value,
            name: () => t.Get("config.minigame-strategies.fishing-automaton-plus.predict-frames.name"),
            tooltip: () => t.Get("config.minigame-strategies.fishing-automaton-plus.predict-frames.tooltip"),
            min: 0,
            max: 20,
            interval: 1
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_CenterBiasBarPercentage,
            setValue: value => ModEntry.Config.FA_CenterBiasBarPercentage = value,
            name: () => t.Get("config.minigame-strategies.fishing-automaton-plus.center-bias-bar-percentage.name"),
            tooltip: () => t.Get("config.minigame-strategies.fishing-automaton-plus.center-bias-bar-percentage.tooltip"),
            min: 0.0f,
            max: 1f,
            interval: 0.01f,
            formatValue: FormatPercentage
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_DualTargetBarPercentage,
            setValue: value => ModEntry.Config.FA_DualTargetBarPercentage = value,
            name: () => t.Get("config.minigame-strategies.fishing-automaton-plus.dual-target-bar-percentage.name"),
            tooltip: () => t.Get("config.minigame-strategies.fishing-automaton-plus.dual-target-bar-percentage.tooltip"),
            min: 0.0f,
            max: 1f,
            interval: 0.01f,
            formatValue: FormatPercentage
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_PrioTreasureAbove,
            setValue: value => ModEntry.Config.FA_PrioTreasureAbove = value,
            name: () => t.Get("config.minigame-strategies.fishing-automaton-plus.prio-treasure-above.name"),
            tooltip: () => t.Get("config.minigame-strategies.fishing-automaton-plus.prio-treasure-above.tooltip"),
            min: 0.0f,
            max: 1.0f,
            interval: 0.01f,
            formatValue: FormatPercentage
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_PrioFishBelow,
            setValue: value => ModEntry.Config.FA_PrioFishBelow = value,
            name: () => t.Get("config.minigame-strategies.fishing-automaton-plus.prio-fish-below.name"),
            tooltip: () => t.Get("config.minigame-strategies.fishing-automaton-plus.prio-fish-below.tooltip"),
            min: 0.0f,
            max: 1.0f,
            interval: 0.01f,
            formatValue: FormatPercentage
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_MinBarTrackingSpeed,
            setValue: value => ModEntry.Config.FA_MinBarTrackingSpeed = value,
            name: () => t.Get("config.minigame-strategies.fishing-automaton-plus.min-bar-tracking-speed.name"),
            tooltip: () => t.Get("config.minigame-strategies.fishing-automaton-plus.min-bar-tracking-speed.tooltip"),
            min: 0.0f,
            max: 5.0f,
            interval: 0.01f
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_MaxBarSpeedNearBottom,
            setValue: value => ModEntry.Config.FA_MaxBarSpeedNearBottom = value,
            name: () => t.Get("config.minigame-strategies.fishing-automaton-plus.max-speed-near-bottom.name"),
            tooltip: () => t.Get("config.minigame-strategies.fishing-automaton-plus.max-speed-near-bottom.tooltip"),
            min: 0.0f,
            max: 5.0f,
            interval: 0.01f
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_MaxBarSpeed,
            setValue: value => ModEntry.Config.FA_MaxBarSpeed = value,
            name: () => t.Get("config.minigame-strategies.fishing-automaton-plus.max-bar-speed.name"),
            tooltip: () => t.Get("config.minigame-strategies.fishing-automaton-plus.max-bar-speed.tooltip"),
            min: 0.0f,
            max: 10.0f,
            interval: 0.01f
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_BottomThreshold,
            setValue: value => ModEntry.Config.FA_BottomThreshold = value,
            name: () => t.Get("config.minigame-strategies.fishing-automaton-plus.bottom-threshold.name"),
            tooltip: () => t.Get("config.minigame-strategies.fishing-automaton-plus.bottom-threshold.tooltip"),
            min: 0.0f,
            max: 100.0f,
            interval: 1f
        );
    }
}
