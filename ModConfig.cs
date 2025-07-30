namespace Fishbot;
using StardewModdingAPI;

public sealed class ModConfig
{
    // Keybinds
    public SButton ToggleAutomationKey { get; set; } = SButton.F5;

    // Automations
    public bool DoAutoCast { get; set; } = true;
    public bool DoAutoHit { get; set; } = true;
    public bool DoAutoPlay { get; set; } = true;
    public bool DoAutoStow { get; set; } = true;
    public bool DoAutoLoot { get; set; } = true;

    // Utility
    public bool EnableBubbleRadar { get; set; } = true;

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
            name: () => nameof(ModEntry.Config.FA_PredictLinearFrames),
            tooltip: () => "Predict fish position in X frames of continued linear movement. Use 0 to disable. (inaccurate, but helps track fast targets)",
            min: 0,
            max: 20,
            interval: 1
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_RelativeOffsetBarPercentage,
            setValue: value => ModEntry.Config.FA_RelativeOffsetBarPercentage = value,
            name: () => nameof(ModEntry.Config.FA_RelativeOffsetBarPercentage),
            tooltip: () => "Fishing bar offset towards the track center relative to size (multiplied by this value) and position. Use 0 for always keeping it centered. (Fish are more likely to move towards the center)",
            min: 0.0f,
            max: 1f,
            interval: 0.01f
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_DualTargetingBarPercentage,
            setValue: value => ModEntry.Config.FA_DualTargetingBarPercentage = value,
            name: () => nameof(ModEntry.Config.FA_DualTargetingBarPercentage),
            tooltip: () => "Amount of the fishing bar that both fish and treasure have to fit within to target both at once. Use 0.0 to avoid dual targeting.",
            min: 0.0f,
            max: 1f,
            interval: 0.01f
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_PrioTreasureAbove,
            setValue: value => ModEntry.Config.FA_PrioTreasureAbove = value,
            name: () => nameof(ModEntry.Config.FA_PrioTreasureAbove),
            tooltip: () => "Prioritize treasure if the capture progress exceeds this threshold. Use 1.0 to ignore treasure.",
            min: 0.0f,
            max: 1.0f,
            interval: 0.01f
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_PrioFishBelow,
            setValue: value => ModEntry.Config.FA_PrioFishBelow = value,
            name: () => nameof(ModEntry.Config.FA_PrioFishBelow),
            tooltip: () => "Prioritize fish if the capture progress falls below this threshold. This value needs to be smaller than FA_PrioTreasureAbove.",
            min: 0.0f,
            max: 1.0f,
            interval: 0.01f
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_MinBarTargetVelocity,
            setValue: value => ModEntry.Config.FA_MinBarTargetVelocity = value,
            name: () => nameof(ModEntry.Config.FA_MinBarTargetVelocity),
            tooltip: () => "Minimum target velocity when tracking fish.",
            min: 0.0f,
            max: 5.0f,
            interval: 0.01f
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_MaxBarVelocityNearBottom,
            setValue: value => ModEntry.Config.FA_MaxBarVelocityNearBottom = value,
            name: () => nameof(ModEntry.Config.FA_MaxBarVelocityNearBottom),
            tooltip: () => "Maximum velocity near the bottom edge, lower values mitigate bouncing.",
            min: 0.0f,
            max: 5.0f,
            interval: 0.01f
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_MaxBarVelocity,
            setValue: value => ModEntry.Config.FA_MaxBarVelocity = value,
            name: () => nameof(ModEntry.Config.FA_MaxBarVelocity),
            tooltip: () => "Maximum fishing bar speed, lower values mitigate overshoot.",
            min: 0.0f,
            max: 10.0f,
            interval: 0.01f
        );
        configMenu.AddNumberOption(
            mod: mod,
            getValue: () => ModEntry.Config.FA_BottomThreshold,
            setValue: value => ModEntry.Config.FA_BottomThreshold = value,
            name: () => nameof(ModEntry.Config.FA_BottomThreshold),
            tooltip: () => "How far from the edge FA_MaxBarVelocityNearBottom starts to take effect, higher values mitigate bouncing.",
            min: 0.0f,
            max: 100.0f,
            interval: 1f
        );
    }
}
