using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using System;

namespace CursedTechniques
{
    public enum HollowPurpleState { Charging, Flying, Exploding, Done }

    public class HollowPurpleOrb
    {
        public HollowPurpleState State = HollowPurpleState.Charging;
        public bool IsExpired => State == HollowPurpleState.Done;
        private float chargeTime = 0f;
        private float maxChargeTime = 1.5f;
        public float ChargePercent => Math.Min(chargeTime / maxChargeTime, 1f);
        private Vector2 position;
        private Vector2 velocity;
        private float speed = 12f;
        private float maxRange = 600f;
        private float traveledDistance = 0f;
        private float explosionTimer = 0f;
        private float explosionDuration = 0.5f;
        private float explosionRadius = 150f;
        private Vector2 explosionPos;
        private float bobTimer = 0f;
        private Vector2 chargeOffset;

        public void AddCharge(float delta) { if (State != HollowPurpleState.Charging) return; chargeTime += delta; bobTimer += delta; }

        public void Launch(Farmer player)
        {
            if (State != HollowPurpleState.Charging) return;
            State = HollowPurpleState.Flying;
            Vector2 dir = GetDir(player.FacingDirection);
            velocity = dir * speed;
            position = player.Position + dir * 60f;
        }

        public void Update(Farmer player, GameLocation location)
        {
            bobTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            if (State == HollowPurpleState.Charging)
            {
                Vector2 forward = GetDir(player.FacingDirection);
                float bob = (float)Math.Sin(bobTimer * 3f) * 8f;
                chargeOffset = forward * 70f + new Vector2(0, bob);
            }
            else if (State == HollowPurpleState.Flying)
            {
                position += velocity;
                traveledDistance += speed;
                foreach (var c in location.characters) { if (c is Monster m && Vector2.Distance(position, m.Position) < 40f) { StartExplosion(position); return; } }
                if (traveledDistance >= maxRange) StartExplosion(position);
            }
            else if (State == HollowPurpleState.Exploding)
            {
                explosionTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
                if (explosionTimer <= 0.05f) foreach (var c in location.characters) { if (c is Monster m) { float d = Vector2.Distance(explosionPos, m.Position); if (d < explosionRadius) { int dmg = (int)(80f * (1f - d / explosionRadius)) + 40; m.takeDamage(dmg, 0, 0, false, 1.0, Game1.player); var dir = m.Position - explosionPos; if (dir != Vector2.Zero) { dir.Normalize(); m.Position += dir * 120f; } } } }
                if (explosionTimer >= explosionDuration) State = HollowPurpleState.Done;
            }
        }

        private void StartExplosion(Vector2 pos) { State = HollowPurpleState.Exploding; explosionPos = pos; explosionTimer = 0f; Game1.playSound("explosion"); }

        private Vector2 GetDir(int f) => f switch { 0 => new Vector2(0,-1), 1 => new Vector2(0,1), 2 => new Vector2(-1,0), 3 => new Vector2(1,0), _ => new Vector2(0,-1) };

        public void Draw(SpriteBatch sb, Farmer player)
        {
            Texture2D tex; try { tex = Game1.content.Load<Texture2D>("Mods/CursedTechniques/hollow_purple_orb"); } catch { return; }
            if (State == HollowPurpleState.Charging)
            {
                Vector2 screen = Game1.GlobalToLocal(Game1.viewport, player.Position + chargeOffset);
                float scale = 1.5f + ChargePercent * 2f;
                int size = (int)(tex.Width * scale);
                float alpha = 0.5f + ChargePercent * 0.5f;
                sb.Draw(tex, new Rectangle((int)(screen.X - size/2), (int)(screen.Y - size/2), size, size), null, new Color(0.7f, 0.2f, 1f, alpha), 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
            }
            else if (State == HollowPurpleState.Flying)
            {
                Vector2 screen = Game1.GlobalToLocal(Game1.viewport, position);
                int size = (int)(tex.Width
