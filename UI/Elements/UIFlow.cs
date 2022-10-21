using ShaderPreview.Structures;
using ShaderPreview.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderPreview.UI.Elements
{
    public class UIFlow : UIContainer, ILayoutContainer
    {
        public float ElementSpacing = 0f;

        bool PerformingLayout;
        Vec2 CurrentLayoutElementPos;

        public override void Recalculate()
        {
            PerformingLayout = true;

            MinWidth = 0;
            MinHeight = 0;

            Vec2 pos = Vec2.Zero;
            Vec2 size = Vec2.Zero;
            float lineMaxHeight = 0;

            foreach (UIElement element in Elements)
            {
                if (pos.X + element.ScreenRect.Width > ScreenRect.Width)
                {
                    if (pos.X == 0)
                    {
                        size.X = Math.Max(size.X, element.ScreenRect.Width);
                        lineMaxHeight = element.ScreenRect.Height;
                        continue;
                    }

                    pos.X = 0;
                    pos.Y += lineMaxHeight + ElementSpacing;
                    if (size.Y > 0)
                        size.Y += ElementSpacing;
                    size.Y += lineMaxHeight;
                    lineMaxHeight = 0;
                }
                lineMaxHeight = Math.Max(lineMaxHeight, element.ScreenRect.Height);
                pos.X += element.ScreenRect.Width + ElementSpacing;
            }

            if (lineMaxHeight > 0)
            {
                if (size.Y > 0)
                    size.Y += ElementSpacing;
                size.Y += lineMaxHeight;
            }

            MinWidth = size.X;
            MinHeight = size.Y;

            base.Recalculate();
            PerformLayout();
        }

        public void LayoutChild(UIElement child, ref Rect screenRect)
        {
            if (!PerformingLayout)
                PerformLayout();
            else 
                screenRect.Position = CurrentLayoutElementPos;
        }

        void PerformLayout()
        {
            PerformingLayout = true;
            Vec2 pos = ScreenRect.Position;
            float lineMaxHeight = 0;

            foreach (UIElement element in Elements)
            {
                if (pos.X + element.ScreenRect.Width > ScreenRect.Right)
                {
                    if (pos.X == 0)
                    {
                        CurrentLayoutElementPos = pos;
                        element.Recalculate();
                        lineMaxHeight = element.ScreenRect.Height;
                        continue;
                    }

                    pos.X = ScreenRect.X;
                    pos.Y += lineMaxHeight + ElementSpacing;
                    lineMaxHeight = 0;
                }
                CurrentLayoutElementPos = pos;
                element.Recalculate();
                lineMaxHeight = Math.Max(lineMaxHeight, element.ScreenRect.Height);
                pos.X += element.ScreenRect.Width + ElementSpacing;
            }

            PerformingLayout = false;
        }
    }
}
