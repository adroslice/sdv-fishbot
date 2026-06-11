namespace Fishbot;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using HarmonyLib;
using System.Linq;

internal sealed class ModEntry : Mod
{
    public static ModConfig Config = new();
    private static bool AutomationEnabled = false;
    private int? restorableDirection = null;
    private bool pausedTimeToday = false;
    private bool hadBait = false;
    private bool wasCasting = false;
    private bool PassedPauseTime => !pausedTimeToday && Game1.timeOfDay % 2400 >= Config.PauseAfterTime && Game1.timeOfDay % 2400 < Config.PauseAfterTime + 100;
    private string fishbotActiveText = "ui.hud.fishbot-active";

    public override void Entry(IModHelper helper)
    {
        Config = this.Helper.ReadConfig<ModConfig>() ?? new();

        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.Patch(
            original: AccessTools.PropertyGetter("Microsoft.Xna.Framework.GamePlatform:IsActive"),
            prefix: new HarmonyMethod(this.GetType(), nameof(this.Pre_XNA_GamePlatform_IsActive))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(Game1), "isOneOfTheseKeysDown", new Type[] { typeof(KeyboardState), typeof(InputButton[]) }),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Pre_Game1_IsOneOfTheseKeysDown))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(BobberBar), "update"),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Pre_BobberBar_Update)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(Post_BobberBar_Update))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(Game1), "areAllOfTheseKeysUp", new Type[] { typeof(KeyboardState), typeof(InputButton[]) }),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Pre_Game1_AreAllOfTheseKeysUp))
        );
        harmony.Patch(
            original: AccessTools.Method(typeof(FishingRod), "tickUpdate"),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(Pre_FishingRod_TickUpdate)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(Post_FishingRod_TickUpdate))
        );

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.DayStarted += (_, _) =>
        {
            fishbotActiveText = this.Helper.Translation.Get("ui.hud.fishbot-active");
            pausedTimeToday = false;
            hadBait = false;
            wasCasting = false;
            AutomationEnabled = false;
        };
        helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdate;
        helper.Events.Display.RenderingHud += OnRenderingHud;
    }

    public static bool Pre_XNA_GamePlatform_IsActive(ref bool __result)
    {
        if (AutomationEnabled) __result = true;
        else return true;
        return false;
    }

    // Patches for playing the minigame
    private static bool _isInBobberBarUpdate = false;
    private static bool _shouldPressFishingButton = false;
    public static void Pre_BobberBar_Update(BobberBar __instance) => _isInBobberBarUpdate = true;
    public static void Post_BobberBar_Update() => _isInBobberBarUpdate = false;
    public static bool Pre_Game1_IsOneOfTheseKeysDown(KeyboardState state, InputButton[] keys, ref bool __result)
    {
        if ((AutomationEnabled || Config.AutomationMode != "toggle") && Config.DoAutoPlay && _isInBobberBarUpdate && keys != null && keys.Contains(Game1.options.useToolButton[0]))
        {
            __result = _shouldPressFishingButton;
            return false;
        }
        return true;
    }

    // Patching for timing the fishing rod cast
    private static bool _isInFishingRodUpdate = false;
    private static bool _shouldReleaseTimingCast = false;
    public static void Pre_FishingRod_TickUpdate(FishingRod __instance) => _isInFishingRodUpdate = true;
    public static void Post_FishingRod_TickUpdate() => _isInFishingRodUpdate = false;
    public static bool Pre_Game1_AreAllOfTheseKeysUp(KeyboardState state, InputButton[] keys, ref bool __result)
    {
        // Handle FishingRod timing cast case
        if ((AutomationEnabled || Config.AutomationMode != "toggle") && Config.DoAutoCast && _isInFishingRodUpdate && keys != null && keys.Contains(Game1.options.useToolButton[0]))
        {
            __result = _shouldReleaseTimingCast;
            if (_shouldReleaseTimingCast) _shouldReleaseTimingCast = false;
            return false;
        }
        return true;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var iconicFramework = this.Helper.ModRegistry.GetApi<IIconicFrameworkApi>("furyx639.ToolbarIcons");
        if (iconicFramework is not null)
        {
            iconicFramework.AddToolbarIcon(
                this.Helper.ModContent.GetInternalAssetName("assets/if_icon.png").BaseName,
                new Rectangle(0, 0, 16, 16),
                () => this.Helper.Translation.Get("toolbar-icons.toggle-automations.name"),
                () => this.Helper.Translation.Get("toolbar-icons.toggle-automations.tooltip"),
                () => { AutomationEnabled = !AutomationEnabled; }
            );
        }

        var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is not null)
        {
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new(),
                save: () => this.Helper.WriteConfig(Config)
            );
            ModConfig.SetupConfigOptions(configMenu, this.ModManifest, this.Helper.Translation);
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (Config.ToggleAutomationKey.JustPressed()) AutomationEnabled = !AutomationEnabled;
    }

    // Renders the Activity Indicator and Bubble Radar
    private void OnRenderingHud(object? sender, StardewModdingAPI.Events.RenderingHudEventArgs e)
    {
        if (!Context.IsWorldReady) return;
        if (Config.EnableBubbleRadar && !(Game1.currentLocation.fishSplashPoint.X == 0 && Game1.currentLocation.fishSplashPoint.Y == 0))
        {
            var screenCenter = new Vector2(
                Game1.viewport.Width / 2,
                Game1.viewport.Height / 2
            );
            var arrowDirection = Vector2.Normalize(new Vector2(
                Game1.currentLocation.fishSplashPoint.X * 64f - (float)Game1.viewport.X + 32f,
                Game1.currentLocation.fishSplashPoint.Y * 64f - (float)Game1.viewport.Y + 32f
            ) - screenCenter);

            e.SpriteBatch.Draw(
                texture: Game1.mouseCursors,
                position: screenCenter + arrowDirection * 64f * 2f,
                sourceRectangle: new Microsoft.Xna.Framework.Rectangle(412, 495, 5, 4),
                Color.White,
                rotation: (float)(-Math.Atan2(arrowDirection.X, arrowDirection.Y) - Math.PI),
                origin: new Vector2(2f, 2f),
                scale: 4f,
                SpriteEffects.None,
                layerDepth: 1f
            );
        }

        if (!AutomationEnabled || Config.AutomationMode == "stealth") return;

        int vpadding = 10, hpadding = 12;
        var text = fishbotActiveText;
        var textSize = Game1.smallFont.MeasureString(text);
        var position = new Vector2(0, Game1.graphics.GraphicsDevice.Viewport.Height - textSize.Y - vpadding * 2);
        IClickableMenu.drawTextureBox(e.SpriteBatch,
            (int)position.X, (int)position.Y,
            (int)textSize.X + hpadding * 2, (int)textSize.Y + vpadding * 2,
            Color.White
        );
        e.SpriteBatch.DrawString(Game1.smallFont, text,
            new Vector2(position.X + hpadding, position.Y + vpadding + 2),
            Color.Black
        );
    }

    // Runs the Automations
    public void OnUpdate(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady || !(AutomationEnabled || Config.AutomationMode != "toggle") || Game1.player.CurrentTool is not FishingRod rod)
            return;

        if (rod.attachments?.Length > 0 && rod.attachments[0] is not null)
            hadBait = true;

        var state = GetFishingState(rod);

        if (state == FishingState.Waiting) wasCasting = false;
        // Auto pause on invalid cast
        if (AutomationEnabled && state == FishingState.ReadyToCast && wasCasting)
        {
            wasCasting = false;
            AutoPause("ui.hud.message.cast-blocked");
            return;
        }
        if (state == FishingState.Casting) wasCasting = true;

        Action automation = state switch
        {
            FishingState.LowStamina => HandleLowStamina,
            FishingState.ReadyToCast when restorableDirection != null => RestoreDirection,
            FishingState.ReadyToCast when PassedPauseTime => PauseAfterTime,
            FishingState.ReadyToCast when Config.DoAutoPauseOnNoBait && hadBait && (rod.attachments?.Length == 0 || rod.attachments?[0] is null) => PauseNoBait,
            FishingState.ReadyToCast when Config.DoAutoCast && AutomationEnabled => StartCasting,
            FishingState.TimingCast when Config.DoAutoCast => () => _shouldReleaseTimingCast = (rod.castingPower >= (Config.CastDistance - 0.01f)),
            FishingState.Nibbling when Config.DoAutoCast => () => rod.endUsing(Game1.currentLocation, Game1.player),
            FishingState.Playing when Config.DoAutoPlay => () => _shouldPressFishingButton = MinigameStrategyFishingAutomatonPlus((BobberBar)Game1.activeClickableMenu),
            FishingState.ShowingTreasure when Config.DoAutoLoot => () => AcquireTreasure((ItemGrabMenu)Game1.activeClickableMenu),
            FishingState.FishCaught when Config.DoAutoLoot => () => rod.doneHoldingFish(Game1.player),
            _ => () => { }
        };
        automation();
    }

    private enum FishingState { Idle, ReadyToCast, TimingCast, Casting, Waiting, Nibbling, Playing, ShowingTreasure, FishCaught, FishEscaped, LowStamina }
    private FishingState GetFishingState(FishingRod rod) => true switch
    {
        _ when Game1.activeClickableMenu is BobberBar bar => bar.distanceFromCatching > 0f ? FishingState.Playing : FishingState.FishEscaped,
        _ when Game1.activeClickableMenu is ItemGrabMenu g && g.source == ItemGrabMenu.source_fishingChest => FishingState.ShowingTreasure,
        _ when rod.fishCaught => FishingState.FishCaught,
        _ when rod.isNibbling && !rod.isReeling && !rod.showingTreasure && !rod.treasureCaught => FishingState.Nibbling,
        _ when rod.isFishing && !rod.isNibbling && !rod.hit => FishingState.Waiting,
        _ when rod.isTimingCast => FishingState.TimingCast,
        _ when rod.isCasting || rod.castedButBobberStillInAir => FishingState.Casting,
        _ when Context.CanPlayerMove && Game1.activeClickableMenu == null => Game1.player.Stamina < 10f ? FishingState.LowStamina : FishingState.ReadyToCast,
        _ => FishingState.Idle
    };

    private void StartCasting()
    {
        Game1.player.lastClick = Game1.player.GetToolLocation();
        Game1.player.BeginUsingTool();
    }

    // Keep faced direction after eating and override other mods orientation restore broken by eating without menu
    private void RestoreDirection()
    {
        if (restorableDirection == null) return;
        Game1.player.faceDirection(restorableDirection.Value);
        restorableDirection = null;
    }

    private void AutoPause(string unlocalizedMessage)
    {
        AutomationEnabled = false;
        Game1.activeClickableMenu = new GameMenu();
        if (Config.AutomationMode != "stealth") Game1.addHUDMessage(new(this.Helper.Translation.Get(unlocalizedMessage)) { messageSubject = Game1.player.CurrentItem });
    }

    private void PauseAfterTime()
    {
        pausedTimeToday = true;
        AutoPause("ui.hud.message.time-passed");
    }

    private void PauseNoBait()
    {
        hadBait = false;
        AutoPause("ui.hud.message.no-bait");
    }

    // On low energy, auto-disable fishbot or auto eat as configured
    private void HandleLowStamina()
    {
        if (Config.AutoEatMode != "disabled")
        {
            var edibles = Game1.player.Items.OfType<Object>().Where(i => i.staminaRecoveredOnConsumption() > 0);
            var underThreshold = edibles.Where(bestItem => ((float)bestItem.sellToStorePrice(Game1.player.UniqueMultiplayerID) / (float)bestItem.staminaRecoveredOnConsumption()) <= Config.MaxSGPE);
            Object? toEat = Config.AutoEatMode == "first"
                ? underThreshold.FirstOrDefault()
                : underThreshold.MinBy(i => (float)i.sellToStorePrice(Game1.player.UniqueMultiplayerID) / (float)i.staminaRecoveredOnConsumption());

            if (toEat is Item item)
            {
                restorableDirection = Game1.player.FacingDirection;
                Game1.player.Items.Reduce(item, 1);
                Game1.player.eatObject(item as Object);
                return;
            }
        }

        if (!AutomationEnabled) return;

        AutoPause("ui.hud.message.low-energy");
    }

    // Grabs all treasure from a GrabMenu and exits only if it could get out everything.
    private void AcquireTreasure(ItemGrabMenu menu)
    {
        var actualInventory = menu.ItemsToGrabMenu.actualInventory;

        for (int i = actualInventory.Count - 1; i >= 0; i--)
            if (Game1.player.addItemToInventoryBool(actualInventory[i]))
            {
                if (Config.AutomationMode != "stealth") Game1.addHUDMessage(new(actualInventory[i].DisplayName) { messageSubject = actualInventory[i], number = actualInventory[i].Stack });
                actualInventory.RemoveAt(i);
            }

        if (actualInventory.Count == 0) menu.exitThisMenu();
    }

    // FishingAutomaton+ Strategy
    internal bool FA_PrioTreasure = false;
    internal bool MinigameStrategyFishingAutomatonPlus(BobberBar bar)
    {
        // Derived values
        float barCenter = bar.bobberBarPos + (bar.bobberBarHeight / 2) - 30f; // 30f here is a correction offset stemming from the bobbers height
        float barMax = BobberBar.bobberBarTrackHeight - bar.bobberBarHeight;
        float treasureBobberDistance = Math.Abs(bar.treasurePosition - bar.bobberPosition);
        float dualTargetAllowance = bar.bobberBarHeight * Config.FA_DualTargetBarPercentage;
        float centerBiasAllowance = bar.bobberBarHeight * Config.FA_CenterBiasBarPercentage;

        // Target prioritization
        if (bar.treasure && bar.treasurePosition > 0 && !bar.treasureCaught)
        {
            if (bar.distanceFromCatching > Config.FA_PrioTreasureAbove) FA_PrioTreasure = true;
            else if (bar.distanceFromCatching < Config.FA_PrioFishBelow) FA_PrioTreasure = false;
        }
        else FA_PrioTreasure = false;

        var targetPos =
            !bar.treasureCaught && bar.treasurePosition != 0 && (treasureBobberDistance < dualTargetAllowance) ? (bar.treasurePosition + bar.bobberPosition) / 2 : // Both if within allowance
            FA_PrioTreasure ? bar.treasurePosition : // Treasure if prioritized
            Math.Clamp( // Fish, try to predict its future position without overshooting its own target...
                bar.bobberPosition + bar.bobberSpeed * Config.FA_PredictFrames,
                Math.Min(bar.bobberTargetPosition, bar.bobberPosition),
                Math.Max(bar.bobberTargetPosition, bar.bobberPosition) // ... and bias towards the center based on allowance and current position on the track
            ) + centerBiasAllowance / 2 - ((bar.bobberPosition / BobberBar.bobberTrackHeight) * centerBiasAllowance);

        // Enforce general speed limit
        if (Math.Abs(bar.bobberBarSpeed) > Config.FA_MaxBarSpeed)
            return bar.bobberBarSpeed > 0;

        // Enforce a target velocity when the fish is inside the bar
        if (IsPosInsideBar(bar.bobberBarHeight, bar.bobberBarPos, targetPos))
        {
            float targetVelocity = FA_BobberBarTargetVelocity(
                Math.Abs(barCenter - targetPos),
                Math.Max(Config.FA_MinBarTrackingSpeed, Math.Abs(bar.bobberSpeed)),
                Config.FA_MaxBarSpeed,
                0.01F,
                (bar.bobberBarHeight / 2)
            );

            if (Math.Abs(bar.bobberBarSpeed) > targetVelocity)
                return bar.bobberBarSpeed > 0;
        }

        // Enforce speed limit near bottom
        if ((bar.bobberBarPos > barMax - Config.FA_BottomThreshold) && bar.bobberBarSpeed > Config.FA_MaxBarSpeedNearBottom)
            return true;

        // Otherwise, accelerate towards target
        return targetPos < barCenter;
    }

    // Parabolic, higher speed allowed as fish reaches outer portion of bar
    private double FA_BobberBarTargetVelocityFn(double v) => v * v;
    private float FA_BobberBarTargetVelocity(float currentDelta, float minVel, float maxVel, float minDelta, float maxDelta)
    {
        if (currentDelta == 0.0F || minDelta == 0.0F || maxDelta == 0.0F)
            return minVel; // Deltas were zero, returning minVel

        // Parabolic, higher speed allowed as fish reaches outer portion of bar
        double oldMinVel = FA_BobberBarTargetVelocityFn((double)minDelta);
        double oldMaxVel = FA_BobberBarTargetVelocityFn((double)maxDelta);

        // Normalize the delta between the two values.
        float newVel = (float)((((maxVel - minVel) / (oldMaxVel - oldMinVel)) * (FA_BobberBarTargetVelocityFn((double)currentDelta) - oldMaxVel)) + maxVel);
        return newVel; // Setting new max velocity
    }

    private bool IsPosInsideBar(int barSize, float barPos, float pos) =>
        (pos >= barPos) && (pos <= barPos + barSize);
}
