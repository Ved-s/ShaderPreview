using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.Structures;
using ShaderPreview.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderPreview.UI.Elements
{
    public class UIGradientSlider : UIScrollBar
    {
        public Color Start = Color.Black;
        public Color End = Color.White;

        public Color CurrentColor => Color.Lerp(Start, End, CurrentValue);
        public float CurrentValue
        {
            get => (ScrollPosition - ScrollMin) / (ScrollMax - ScrollMin);
            set => ScrollPosition = value * (ScrollMax - ScrollMin) + ScrollMin;
        }

        public UIGradientSlider()
        {
            Horizontal = true;
            ScrollMin = 0;
            ScrollMax = 1;
            ScrollPosition = 0;
            BarSize = .05f;
        }

        protected override void DrawBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.PushAndChangeState(sortMode: SpriteSortMode.Immediate);

            Content.Gradient.Parameters["start"].SetValue(Start.ToVector4());
            Content.Gradient.Parameters["end"].SetValue(End.ToVector4());
            Content.Gradient.CurrentTechnique.Passes[0].Apply();

            spriteBatch.FillRectangle(ScreenRect, Color.White);

            spriteBatch.RestoreState();

            if (BorderColor != Color.Transparent)
                spriteBatch.DrawRectangle(ScreenRect, BorderColor);
        }

        protected override void DrawBar(SpriteBatch spriteBatch)
        {
            spriteBatch.FillRectangle(BarRect, CurrentColor);

            Color border = BorderColor;
            if (border.A == 0)
                border = Color.White;

            spriteBatch.DrawRectangle(BarRect, border);
        }
    }
}
