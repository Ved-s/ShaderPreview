using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShaderPreview.Structures;
using ShaderPreview.UI.Helpers;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace ShaderPreview.UI.Elements
{
    public class UIButton : UIElement
    {
        public string? Text
        {
            get => text;
            set
            {
                text = value;
                Lines = text?.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                Recalculate();
            }
        }

        public Offset4 Padding = new(3, 5);
        public Vec2 TextAlign;

        public Color TextColor = Color.White;

        public Color BackColor = new(48, 48, 48);
        public Color HoverBackColor = new(64, 64, 64);
        public Color SelectedBackColor = new(72, 72, 72);
        public Color ClickedBackColor = new(96, 96, 96);

        public Color BorderColor = new(100, 100, 100);

        public bool CanDeselect = true;
        public bool Selected
        {
            get => SelectedInternal;
            set { SelectedInternal = value; RadioGroup?.SelectButton(value ? this : null); }
        }
        public RadioButtonGroup? RadioGroup
        {
            get => radioGroup;
            set
            {
                if (radioGroup == value)
                    return;

                radioGroup?.RemoveButton(this);
                radioGroup = value;
                radioGroup?.AddButton(this);
            }
        }
        public object? RadioTag;

        private string[]? Lines;
        private string? text;
        internal bool SelectedInternal;
        private RadioButtonGroup? radioGroup;

        public override void Recalculate()
        {
            MinWidth = 0;
            MinHeight = 0;
            if (Font is not null && Lines?.Length is not null and > 0)
            {
                MinWidth = Lines.Select(line => Font.MeasureString(line).X).Max() + Padding.Horizontal;
                MinHeight = Lines.Length * (Font.LineSpacing - 3) + Padding.Vertical;
            }

            base.Recalculate();
        }

        protected override void UpdateSelf()
        {
            if (RadioGroup is not null && Hovered && Root.MouseLeftKey == KeybindState.JustPressed)
            {
                if (!Selected || CanDeselect)
                    Selected = !Selected;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (Hovered && Root.MouseState.LeftButton == ButtonState.Pressed)
                spriteBatch.FillRectangle(ScreenRect, ClickedBackColor);
            else if (Selected)
                spriteBatch.FillRectangle(ScreenRect, SelectedBackColor);
            else if (Hovered)
                spriteBatch.FillRectangle(ScreenRect, HoverBackColor);
            else
                spriteBatch.FillRectangle(ScreenRect, BackColor);

            spriteBatch.DrawRectangle(ScreenRect, BorderColor);

            if (Lines is null || Font is null)
                return;

            float y = Padding.Top + ScreenRect.Y + (ScreenRect.Height - Padding.Vertical - Lines.Length * (Font.LineSpacing - 4)) * TextAlign.Y;

            foreach (string line in Lines)
            {
                float lineWidth = Font.MeasureString(line).X;

                float x = Padding.Left + ScreenRect.X + (ScreenRect.Width - Padding.Horizontal - lineWidth) * TextAlign.X;

                spriteBatch.DrawString(Font, line, new(x, y), TextColor);

                y += Font.LineSpacing - 4;
            }
        }
    }
}
