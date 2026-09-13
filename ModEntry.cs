using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;

namespace CursedTechniques
{
    public class ModEntry : Mod
    {
        private List<AkaOrb> activeAkaOrbs = new List<AkaOrb>();
        private List<AoOrb> activeAoOrbs = new List<AoOrb>();
        private HollowPurpleOrb hollowPurple = null;
        private bool isChargingHP = false;
        private MobileUI mobileUI;
        private bool isMobile => Constants.TargetPlatform == GamePlatform.Android || Constants.TargetPlatform == GamePlatform.iOS;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Display.RenderedHud += OnRenderedHud;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Display.WindowResized += OnWindowResized;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Input.ButtonReleased += OnButtonReleased;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            mobileUI = new MobileUI();
            mobileUI.OnAkaTapped += () => activeAkaOrbs.Add(new AkaOrb(Game1.player));
            mobileUI.OnAoTapped += () => activeAoOrbs.Add(new AoOrb(Game1.player));
            mobileUI.OnHPHoldStart += () => { hollowPurple = new HollowPurpleOrb(); isChargingHP = true; Game1.playSound("secret1"); };
            mobileUI.OnHPHoldRelease += () => { if (hollowPurple != null && isChargingHP) { isChargingHP = false; hollowPurple.Launch(Game1.player); Game1.playSound("thunder"); } };
        }

        private void OnWindowResized(object sender, WindowResizedEventArgs e) => mobileUI?.RecalculatePositions();

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || isMobile) return;
            var item = Game1.player.CurrentItem;
            if (item == null) return;
            if (e.Button == SButton.R)
            {
                if (item.Name == "Aka - Kırmızı") activeAkaOrbs.Add(new AkaOrb(Game1.player));
                if (item.Name == "Ao - Mavi") activeAoOrbs.Add(new AoOrb(Game1.player));
                if (item.Name == "Hollow Purple") { hollowPurple = new HollowPurpleOrb(); isChargingHP = true; Game1.playSound("secret1"); }
            }
        }

        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (!Context.IsWorldReady || isMobile) return;
            if (e.Button == SButton.R && isChargingHP && hollowPurple != null)
            { isChargingHP = false; hollowPurple.Launch(Game1.player); Game1.playSound("thunder"); }
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            var player = Game1.player;
            float delta = (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            if (isMobile) HandleTouchInput();
            mobileUI?.Update(delta, player.CurrentItem?.Name ?? "");
            for (int i = activeAkaOrbs.Count - 1; i >= 0; i--) { activeAkaOrbs[i].Update(player); if (activeAkaOrbs[i].IsExpired) activeAkaOrbs.RemoveAt(i); }
            if (activeAkaOrbs.Count > 0) PushEnemies(player);
            for (int i = activeAoOrbs.Count - 1; i >= 0; i--) { activeAoOrbs[i].Update(player); if (activeAoOrbs[i].IsExpired) activeAoOrbs.RemoveAt(i); }
            if (activeAoOrbs.Count > 0) PullEnemies(player);
            if (hollowPurple != null) { if (isChargingHP) hollowPurple.AddCharge(delta); else hollowPurple.Update(player, player.currentLocation); if (hollowPurple.IsExpired) hollowPurple = null; }
        }

        private void HandleTouchInput()
        {
            var touchState = TouchPanel.GetState();
            foreach (var touch in touchState)
            {
                int x = (int)(touch.Position.X / Game1.options.zoomLevel);
                int y = (int)(touch.Position.Y / Game1.options.zoomLevel);
                if (touch.State == TouchLocationState.Pressed) mobileUI.HandleTouchDown(x, y);
                if (touch.State == TouchLocationState.Released) mobileUI.HandleTouchUp(x, y);
            }
        }

        private void PushEnemies(Farmer player) { var loc = player.currentLocation; if (loc == null) return; foreach (var c in loc.characters) { if (c is StardewValley.Monsters.Monster m) { float d = Vector2.Distance(player.Position, m.Position); if (d < 300f && d > 20f) { var dir = m.Position - player.Position; dir.Normalize(); m.Position += dir * ((300f - d) / 300f * 5f); } } } }
        private void PullEnemies(Farmer player) { var loc = player.currentLocation; if (loc == null) return; foreach (var c in loc.characters) { if (c is StardewValley.Monsters.Monster m) { float d = Vector2.Distance(player.Position, m.Position); if (d < 300f && d > 20f) { var dir = player.Position - m.Position; dir.Normalize(); m.Position += dir * ((300f - d) / 300f * 3.5f); } } } }

        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e) { if (!Context.IsWorldReady) return; foreach (var o in activeAkaOrbs) o.Draw(e.SpriteBatch, Game1.player); foreach (var o in activeAoOrbs) o.Draw(e.SpriteBatch, Game1.player); hollowPurple?.Draw(e.SpriteBatch, Game1.player); }
        private void OnRenderedHud(object sender, RenderedHudEventArgs e) { if (!Context.IsWorldReady || !isMobile) return; mobileUI?.Draw(e.SpriteBatch); }
    }
}
