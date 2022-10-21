﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShaderPreview.Structures;

namespace ShaderPreview.ParameterInputs
{
    public class RelativePosInput : ParameterInput
    {
        public override string DisplayName => "Relative pos";

        public Vec2 Pos;
        public Vec2 PointerSize = new(10);
        bool Grabbed;

        public override bool AppliesToParameter(EffectParameter parameter)
        {
            return parameter.ParameterType == EffectParameterType.Single
                && parameter.ParameterClass == EffectParameterClass.Vector
                && parameter.ColumnCount == 2;
        }

        public override void UpdateSelf(EffectParameter parameter, bool selected)
        {
            if (!selected)
            {
                parameter.SetValue(Pos);
                return;
            }

            if (!Grabbed)
            {
                Rect pointerRect = new(ShaderPreview.TextureScreenRect.Position + ShaderPreview.TextureScreenRect.Size * Pos - PointerSize / 2, PointerSize);
                if (pointerRect.Contains(Interface.Root.MousePosition) && Interface.Root.MouseLeftKey == KeybindState.JustPressed)
                {
                    Grabbed = true;
                }
            }

            if (Grabbed && Interface.Root.MouseState.LeftButton == ButtonState.Released)
                Grabbed = false;

            if (Grabbed)
                Pos = (Interface.Root.MousePosition - ShaderPreview.TextureScreenRect.Position) / ShaderPreview.TextureScreenRect.Size;

            parameter.SetValue(Pos);
        }

        public override void DrawSelf(SpriteBatch spriteBatch, bool selected)
        {
            Vec2 size = selected ? PointerSize : PointerSize / 2;
            Vec2 pos = ShaderPreview.TextureScreenRect.Position + ShaderPreview.TextureScreenRect.Size * Pos;

            Rect pointerRect = new(pos - size / 2, size);

            spriteBatch.FillRectangle(pointerRect, Color.Yellow * (selected ? .8f : 0.3f));

            if (selected)
                spriteBatch.DrawStringShaded(ShaderPreview.Consolas10, ParameterName, pos + new Vec2(7, 2), Color.White, Color.Black);
            else
                spriteBatch.DrawString(ShaderPreview.Consolas10, ParameterName, pos + new Vec2(7, 2), Color.White * .3f);
        }
    }
}
