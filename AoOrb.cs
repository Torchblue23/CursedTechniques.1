using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace CursedTechniques
{
    public class AoOrb
    {
        private Vector2 offset;
        private float bobTimer = 0f;
        private float bobAmount = 12f;
        private float bobSpeed = 2.5f;
        private float lifetime = 0f;
        private float maxLifetime = 999f;
        private float forwardDistance = 80f;
        public bool IsExpired => lifetime >= maxLifetime;

        public AoOrb(Farmer player) { }

        public void Update(Farmer player)
        {
            lifetime += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            bobTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
            if (player.CurrentItem == null || player.CurrentItem.Name != "Ao - Mavi") { lifetime = maxLifetime; return; }
            Vector2 forward = GetDir(player.FacingDirection);
            float bob = (float)Math.Sin(bobTimer * bobSpeed) * bobAmount;
            offset = forward * forwardDistance + new Vector2(0, bob);
        }

        private Vector2 GetDir(int f) => f switch { 0 => new Vector2(0,-1), 1 => new Vector2(0,1), 2 => new Vector2(-1,0), 3 => new Vector2(1,0), _ => new Vector2(0,-1) };

        public void Draw(SpriteBatch sb, Farmer player)
        {
            Vector2 screen = Game1.GlobalToLocal(Game1.viewport, player.Position + offset);
            Texture2D tex; try { tex = Game1.content.Load<Texture2D>("Mods/CursedTechniques/ao_orb"); } catch { return; }
            float alpha = 0.85f + (float)Math.Sin(bobTimer * 3f) * 0.15f;
            int size = (int)(tex.Width * 2.5f);
            sb.Draw(tex, new Rectangle((int)(screen.X - size/2), (int)(screen.Y - size/2), size, size), null, new Color(0.3f, 0.5f, 1f, alpha), 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
        }
    }
}
