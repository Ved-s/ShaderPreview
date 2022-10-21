using ShaderPreview.UI.Elements;
using ShaderPreview.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderPreview.UI
{
    public class MinMaxSlider : UIPanel
    {
        public static readonly ElementEvent<float, MinMaxSlider> ScrollChanged = new();

        private float min;
        private float max;

        public float Min 
        {
            get => min;
            set
            {
                min = value;
                MinInput.Value = value;
                Scroll.ScrollMin = value;
                Scroll.BarSize = 10 / Scroll.ScreenRect.Width * (Scroll.ScrollMax - Scroll.ScrollMin);
            }
        }
        public float Max 
        {
            get => max;
            set
            {
                max = value;
                MaxInput.Value = value;
                Scroll.ScrollMax = value;
                Scroll.BarSize = 10 / Scroll.ScreenRect.Width * (Scroll.ScrollMax - Scroll.ScrollMin);
            }
        }
        public float Value
        {
            get => Scroll.ScrollPosition;
            set => Scroll.ScrollPosition = value;
        }

        private UINumberInput MinInput;
        private UINumberInput MaxInput;
        private UIScrollBar Scroll;

        public MinMaxSlider()
        {
            Padding = 5;

            Elements.Add(new UILabel
            {
                Width = new(0, .6f),
                Height = new(0, .27f),

                Text = "Min: ",
                TextAlign = new(0, .5f)
            });

            Elements.Add(new UILabel
            {
                Width = new(0, .6f),
                Height = new(0, .27f),

                Top = new(0, .5f, -.5f),

                Text = "Max: ",
                TextAlign = new(0, .5f)
            });

            Elements.Add(new UINumberInput
            {
                Width = new(0, .3f),
                Height = new(0, .27f),

                Left = new(0, 1, -1),

                Value = 0
            }.Assign(out MinInput));

            Elements.Add(new UINumberInput
            {
                Width = new(0, .3f),
                Height = new(0, .27f),

                Left = new(0, 1, -1),

                Top = new(0, .5f, -.5f),

                Value = 1
            }.Assign(out MaxInput));

            Elements.Add(new UIScrollBar()
            {
                Horizontal = true,

                Width = new(0, 1),
                Height = new(0, .135f),

                Top = new(0, 1 - .0675f, -1),

                ScrollMin = 0,
                ScrollMax = 1,

                BarPadding = new(-4, 0),

                BackColor = new(0, 0, 0, 100),
            }.Assign(out Scroll));

            MinInput.OnEvent(UINumberInput.ValueChanged, (min, _) =>
            {
                this.min = (float)min.Value;
                if (this.min <= Scroll.ScrollMax)
                {
                    Scroll.ScrollMin = this.min;
                    Scroll.BarSize = 10 / Scroll.ScreenRect.Width * (Scroll.ScrollMax - Scroll.ScrollMin);
                }
            });

            MaxInput.OnEvent(UINumberInput.ValueChanged, (max, _) =>
            {
                this.max = (float)max.Value;
                if (this.max >= Scroll.ScrollMin)
                {
                    Scroll.ScrollMax = this.max;
                    Scroll.BarSize = 10 / Scroll.ScreenRect.Width * (Scroll.ScrollMax - Scroll.ScrollMin);
                }
            });

            MinInput.OnEvent(UIElement.ActiveChangedEvent, (min, active) => 
            {
                if (active || this.min == Scroll.ScrollMin)
                    return;

                MinInput.Value = Scroll.ScrollMin;
                this.min = Scroll.ScrollMin;
            });

            MaxInput.OnEvent(UIElement.ActiveChangedEvent, (max, active) =>
            {
                if (active || this.max == Scroll.ScrollMax)
                    return;

                MaxInput.Value = Scroll.ScrollMax;
                this.max = Scroll.ScrollMax;
            });

            Scroll.AddPreEventCallback(UIScrollBar.ScrollChanged, (scroll, v) => Events.PreCall(ScrollChanged, v));
            Scroll.AddPostEventCallback(UIScrollBar.ScrollChanged, (scroll, v) => Events.PostCall(ScrollChanged, v));
        }

        public override void Recalculate()
        {
            base.Recalculate();
            Scroll.BarSize = 10 / Scroll.ScreenRect.Width * (Scroll.ScrollMax - Scroll.ScrollMin);
        }
    }
}
