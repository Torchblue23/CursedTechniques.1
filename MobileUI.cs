using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

namespace CursedTechniques
{
    public class MobileUI
    {
        private const int ButtonSize = 80;
        private const int ButtonPadding = 12;
        private const int EdgePadding = 20;
        private Rectangle akaButton, aoButton, hollowPurpleButton;
        public bool IsHoldingHP { get; private set; } = false;
        private float hpHoldTime = 0f;
        public event Action OnAkaTapped;
        public event Action OnAoTapped;
        public event Action OnHPHoldStart;
        public event Action OnHPHoldRelease;
        private string currentWeapon = "";

        public MobileUI() { RecalculatePositions(); }

        public void RecalculatePositions()
        {
            int w = Game1.uiViewport.Width, h = Game1.uiViewport.Height;
            int x = w - ButtonSize - EdgePadding;
            hollowPurpleButton = new Rectangle(x, h - ButtonSize - EdgePadding, ButtonSize, ButtonSize);
            aoButton = new Rectangle(x, h - (ButtonSize + ButtonPadding) * 2 - EdgePadding, ButtonSize, ButtonSize);
            akaButton = new Rectangle(x, h - (ButtonSize + ButtonPadding) * 3 - EdgePadding, ButtonSize, ButtonSize);
        }

        public void Update(float delta, string weaponName)
        {
            currentWeapon = weaponName;
            if (IsHoldingHP) hpHoldTime += delta;
        }

        public void HandleTouchDown(int x, int y)
        {
            if (currentWeapon == "Aka - Kırmızı" && akaButton.Contains(x, y)) { OnAkaTapped?.Invoke(); Game1.playSound("dwop"); }
            else if (currentWeapon == "Ao - Mavi" && aoButton.Contains(x, y)) { OnAoTapped?.Invoke(); Game1.playSound("dwop"); }
            else if (currentWeapon == "Hollow Purple" && hollowPurpleButton.Contains(x, y)) { IsHoldingHP = true; hpHoldTime = 0f; OnHPHoldStart?.Invoke(); }
        }

        public void HandleTouchUp(int x, int y) { if (IsHoldingHP) { IsHoldingHP = false; OnHPHoldRelease?.Invoke(); } }

        public void Draw(SpriteBatch sb)
        {
            if (currentWeapon != "Aka - Kırmızı" && currentWeapon != "Ao - Mavi" && currentWeapon != "Hollow Purple") return;
            Texture2D px = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            px.SetData(new[] { Color.White });
            if (currentWeapon == "Aka - Kırmızı") DrawButton(sb, px, akaButton, new Color(1f, 0.2f, 0.2f, 0.85f), "赤");
            if (currentWeapon == "Ao - Mavi") DrawButton(sb, px, aoButton, new Color(0.2f, 0.5f, 1f, 0.85f), "蒼");
            if (currentWeapon == "Hollow Purple")
            {
                float charge = Math.Min(hpHoldTime / 1.5f, 1f);
                sb.Draw(px, hollowPurpleButton, new Color(0.6f, 0.1f, 1f, 0.7f));
                if (charge > 0f) { int fh = (int)(hollowPurpleButton.Height * charge); sb.Draw(px, new Rectangle(hollowPurpleButton.X, hollowPurpleButton.Bottom - fh, hollowPurpleButton.Width, fh), new Color(0.9f, 0.5f, 1f, 0.6f)); }
                DrawBorder(sb, px, hollowPurpleButton, charge >= 1f ? new Color(1f, 0.9f, 1f) : new Color(0.7f, 0.3f, 1f), charge >= 1f ? 4 : 2);
                sb.DrawString(Game1.smallFont, charge >= 1f ? "✦" : "茈", new Vector2(hollowPurpleButton.X + 25, hollowPurpleButton.Y + 25), Color.White);
            }
        }

        private void DrawButton(SpriteBatch sb, Texture2D px, Rectangle rect, Color color, string label)
        {
            sb.Draw(px, rect, color * 0.7f);
            DrawBorder(sb, px, rect, color, 3);
            sb.DrawString(Game1.smallFont, label, new Vector2(rect.X + 25, rect.Y + 25), Color.White);
        }

        private void DrawBorder(SpriteBatch sb, Texture2D px, Rectangle r, Color c, int t)
        {
            sb.Draw(px, new Rectangle(r.X, r.Y, r.Width, t), c);
            sb.Draw(px, new Rectangle(r.X, r.Bottom - t, r.Width, t), c);
            sb.Draw(px, new Rectangle(r.X, r.Y, t, r.Height), c);
            sb.Draw(px, new Rectangle(r.Right - t, r.Y, t, r.Height), c);
        }
    }
}
