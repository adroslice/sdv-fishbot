namespace Fishbot;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Tools;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

internal sealed class ModEntry : Mod
{
    public static ModConfig Config = new();
    private bool AutomationEnabled = false;
    private void ClickIf(bool cond) => OverrideButton(SButton.C, cond);
    private Action<SButton, bool> OverrideButton = (_, _) => { };

    public override void Entry(IModHelper helper)
    {
        Config = this.Helper.ReadConfig<ModConfig>() ?? new();

        OverrideButton = (Action<SButton, bool>)Delegate.CreateDelegate(typeof(Action<SButton, bool>), Game1.input, Game1.input.GetType().GetMethod("OverrideButton")
           ?? throw new InvalidOperationException("Can't find 'OverrideButton' method on SMAPI's input class. This means the mod needs to be updated to use a new input simulation method."));

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdate;
        helper.Events.Display.RenderingHud += OnRenderingHud;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;

        configMenu.Register(
            mod: this.ModManifest,
            reset: () => Config = new(),
            save: () => this.Helper.WriteConfig(Config)
        );

        ModConfig.SetupConfigOptions(configMenu, this.ModManifest);
    }

    // Toggles the Automations
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button == Config.ToggleAutomationKey) AutomationEnabled = !AutomationEnabled;
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

        if (!AutomationEnabled) return;

        int vpadding = 10, hpadding = 12;
        var text = "Fishbot active!";
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
        if (!Context.IsWorldReady || !AutomationEnabled || Game1.player.CurrentTool is not FishingRod rod)
            return;

        Action automation = GetFishingState(rod) switch
        {
            FishingState.ReadyToCast when Config.DoAutoCast => () => ClickIf(true),
            FishingState.TimingCast when Config.DoAutoCast => () => ClickIf(!(rod.castingPower >= 0.99f)),
            FishingState.Nibbling when Config.DoAutoHit => () => rod.endUsing(Game1.currentLocation, Game1.player),
            FishingState.Playing when Config.DoAutoPlay => () => ClickIf(MinigameStrategyFishingAutomatonPlus((BobberBar)Game1.activeClickableMenu)),
            FishingState.ShowingTreasure when Config.DoAutoLoot => () => AcquireTreasure((ItemGrabMenu)Game1.activeClickableMenu),
            FishingState.FishCaught when Config.DoAutoStow => () => ClickIf(true),
            _ => () => { }
        };
        automation();
    }

    private enum FishingState { Idle, ReadyToCast, TimingCast, Casting, Waiting, Nibbling, Playing, ShowingTreasure, FishCaught, FishEscaped }
    private FishingState GetFishingState(FishingRod rod) => true switch
    {
        _ when Game1.activeClickableMenu is BobberBar bar => bar.distanceFromCatching > 0f ? FishingState.Playing : FishingState.FishEscaped,
        _ when Game1.activeClickableMenu is ItemGrabMenu => FishingState.ShowingTreasure,
        _ when rod.fishCaught => FishingState.FishCaught,
        _ when rod.isNibbling && !rod.isReeling && !rod.showingTreasure && !rod.treasureCaught => FishingState.Nibbling,
        _ when rod.isFishing && !rod.isNibbling && !rod.hit => FishingState.Waiting,
        _ when rod.isTimingCast => FishingState.TimingCast,
        _ when rod.isCasting || rod.castedButBobberStillInAir => FishingState.Casting,
        _ when Context.CanPlayerMove && Game1.activeClickableMenu == null => FishingState.ReadyToCast,
        _ => FishingState.Idle
    };

    // Grabs all treasure from a GrabMenu and exits only if it could get out everything.
    private void AcquireTreasure(ItemGrabMenu menu)
    {
        var actualInventory = menu.ItemsToGrabMenu.actualInventory;

        for (int i = actualInventory.Count - 1; i >= 0; i--)
            if (Game1.player.addItemToInventoryBool(actualInventory[i]))
            {
                Game1.addHUDMessage(new(actualInventory[i].DisplayName) { messageSubject = actualInventory[i] });
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
        float dualTargetAllowance = bar.bobberBarHeight * Config.FA_DualTargetingBarPercentage;
        float centerBiasAllowance = bar.bobberBarHeight * Config.FA_RelativeOffsetBarPercentage;

        // Target prioritization
        if (bar.treasure && bar.treasurePosition > 0 && !bar.treasureCaught)
        {
            if (bar.distanceFromCatching > Config.FA_PrioTreasureAbove) FA_PrioTreasure = true;
            else if (bar.distanceFromCatching < Config.FA_PrioFishBelow) FA_PrioTreasure = false;
        }
        else FA_PrioTreasure = false;

        var targetPos =
            !bar.treasureCaught && (treasureBobberDistance < dualTargetAllowance) ? (bar.treasurePosition + bar.bobberPosition) / 2 : // Both if within allowance
            FA_PrioTreasure ? bar.treasurePosition : // Treasure if prioritized
            Math.Clamp( // Fish, try to predict its future position without overshooting its own target...
                bar.bobberPosition + bar.bobberSpeed * Config.FA_PredictLinearFrames,
                Math.Min(bar.bobberTargetPosition, bar.bobberPosition),
                Math.Max(bar.bobberTargetPosition, bar.bobberPosition) // ... and bias towards the center based on allowance and current position on the track
            ) + centerBiasAllowance / 2 - ((bar.bobberPosition / BobberBar.bobberTrackHeight) * centerBiasAllowance);

        // Enforce general speed limit
        if (Math.Abs(bar.bobberBarSpeed) > Config.FA_MaxBarVelocity)
            return bar.bobberBarSpeed > 0;

        // Enforce a target velocity when the fish is inside the bar
        if (IsPosInsideBar(bar.bobberBarHeight, bar.bobberBarPos, targetPos))
        {
            float targetVelocity = FA_BobberBarTargetVelocity(
                Math.Abs(barCenter - targetPos),
                Math.Max(Config.FA_MinBarTargetVelocity, Math.Abs(bar.bobberSpeed)),
                Config.FA_MaxBarVelocity,
                0.01F,
                (bar.bobberBarHeight / 2)
            );

            if (Math.Abs(bar.bobberBarSpeed) > targetVelocity)
                return bar.bobberBarSpeed > 0;
        }

        // Enforce speed limit near bottom
        if ((bar.bobberBarPos > barMax - Config.FA_BottomThreshold) && bar.bobberBarSpeed > Config.FA_MaxBarVelocityNearBottom)
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
