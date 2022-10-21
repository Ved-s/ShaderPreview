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
    public class UIList : UIContainer, ILayoutContainer
    {
        static RasterizerState Scissors = new() { ScissorTestEnable = true };

        public UIScrollBar ScrollBar = new()
        {
            Width = 15,
            Height = new(0, 1),

            Left = new(0, 1, -1),
            BarPadding = new(3),

            BackColor = Color.White * 0.1f,
            ScrollDistance = 20

        };

        public float ElementSpacing = 0f;

        bool LayoutInProgress = false;
        float OldScroll;
        float CurrentLayoutElementY;

        protected override void PreUpdateChildren()
        {
            if (Elements.Count == 0 || Elements[0] != ScrollBar)
                Elements.Insert(0, ScrollBar);

            if (OldScroll != ScrollBar.ScrollPosition)
                Recalculate();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            PreDrawChildren(spriteBatch);

            if (ScrollBar.Visible)
                ScrollBar.Draw(spriteBatch);

            Rect rect = ScreenRect;
            rect.X += Padding.Left;
            rect.Y += Padding.Top;
            rect.Width -= Padding.Horizontal;
            rect.Height -= Padding.Vertical;

            if (ScrollBar.Visible)
                rect.Width -= ScrollBar.ScreenRect.Width;

            spriteBatch.PushAndChangeState(rasterizerState: Scissors);

            Rectangle oldScissors = spriteBatch.GraphicsDevice.ScissorRectangle;
            spriteBatch.GraphicsDevice.ScissorRectangle = (Rectangle)rect;

            foreach (UIElement element in Elements)
                if (element != ScrollBar && element.Visible)
                    element.Draw(spriteBatch);

            spriteBatch.RestoreState();
            spriteBatch.GraphicsDevice.ScissorRectangle = oldScissors;

            PostDrawChildren(spriteBatch);
        }

        public override void Recalculate()
        {
            if (Elements.Count == 0 || Elements[0] != ScrollBar)
                Elements.Insert(0, ScrollBar);

            LayoutInProgress = true;
            SkipRecalculatingChildren = true;
            base.Recalculate();
            ScrollBar.Recalculate();

            float contentHeight = Elements.Where(e => e != ScrollBar).Select(e => e.ScreenRect.Height).Sum() + ElementSpacing * Math.Max(0, Elements.Count - 2);
            ScrollBar.ScrollMax = Math.Max(0, contentHeight - ScreenRect.Height - Padding.Vertical);
            ScrollBar.ScrollPosition = Math.Min(ScrollBar.ScrollPosition, ScrollBar.ScrollMax);
            ScrollBar.BarSize = ScreenRect.Height - Padding.Vertical;

            if (ScrollBar.Visible != ScrollBar.ScrollMax > 0)
            {
                ScrollBar.Visible = ScrollBar.ScrollMax > 0;
                Recalculate();
                return;
            }

            PerformLayout();
            LayoutInProgress = false;
        }

        public void LayoutChild(UIElement child, ref Rect screenRect)
        {
            if (child == ScrollBar)
                return;

            if (!LayoutInProgress)
            {
                LayoutInProgress = true;
                PerformLayout();
                LayoutInProgress = false;
            }
            else
            {
                screenRect.Top = CurrentLayoutElementY;
                screenRect.Left = ScreenRect.X + Padding.Left;
                screenRect.Width = ScreenRect.Width - Padding.Horizontal - (ScrollBar.Visible ? ScrollBar.ScreenRect.Width : 0);
            }
        }

        void PerformLayout()
        {
            float startPos = ScreenRect.Y + Padding.Top - ScrollBar.ScrollPosition;

            foreach (UIElement element in Elements)
            {
                if (element == ScrollBar)
                    continue;

                CurrentLayoutElementY = startPos;
                element.Recalculate();
                startPos += element.ScreenRect.Height + ElementSpacing;
            }
            OldScroll = ScrollBar.ScrollPosition;
        }
    }
}
