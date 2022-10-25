using Microsoft.Xna.Framework;
using ShaderPreview.UI.Elements;
using ShaderPreview.UI.Helpers;
using System;
using System.Linq;
using System.Reflection.PortableExecutable;

namespace ShaderPreview.UI
{
    public class AnimationSlider : UIPanel
    {
        public static readonly ElementEvent<ValueChangedArgs, AnimationSlider> ValueChanged = new();

        public readonly SliderValues Values;
        public readonly float[] ValueArray;

        UIScrollBar[] ValueScrolls;
        UIButton[] PlayButtons;
        UINumberInput MinInput, MaxInput, StepInput;
        bool IgnoreScrollEvents;
        float Step = 1;
        float Speed = 1;
        float Min = 0;
        float Max = 1;

        AnimationMode CurrentAnimation;
        bool[] AnimationActive;
        bool[] AnimationReverse;

        public AnimationSlider(int inputCount)
        {
            Padding = 5;

            int y = 0;

            ValueArray = new float[inputCount];
            ValueScrolls = new UIScrollBar[inputCount];
            PlayButtons = new UIButton[inputCount];
            AnimationActive = new bool[inputCount];
            AnimationReverse = new bool[inputCount];

            for (int i = 0; i < inputCount; i++)
            {
                UIScrollBar scroll = new UIScrollBar
                {
                    Width = new(-20, 1),
                    Height = 8,

                    Top = y + 4,
                    Left = 20,

                    BarSize = 8,
                    BarSizeAbsolute = true,
                    Horizontal = true,
                    ScrollMin = 0,
                    ScrollMax = 1,
                    BarPadding = new(-4, 0),
                    BackColor = new(0, 0, 0, 100),
                    ScrollPosition = 0
                };

                int index = i;
                scroll.BeforeEvent(UIScrollBar.ScrollChanged, (_, value) => IgnoreScrollEvents || Events.PreCall(ValueChanged, new(index, ApplyStepToValue(value))));
                scroll.OnEvent(UIScrollBar.ScrollChanged, (_, value) =>
                {
                    if (IgnoreScrollEvents)
                        return;

                    value = ApplyStepToValue(value);
                    ValueArray[index] = value;
                    Events.PostCall(ValueChanged, new(index, value));
                });
                ValueScrolls[i] = scroll;
                y += 20;
            }

            RadioButtonGroup group = new();

            group.ButtonClicked += (btn, tag) =>
            {
                if (tag is AnimationMode mode)
                    CurrentAnimation = mode;
            };

            Elements = new(this)
            {
                new UINumberInput
                {
                    Top = y,
                    Height = 20,
                    Width = new(-4, .2f),
                    TextAlign = new(.5f),
                    ValueValidator = (ref double v) =>
                    {
                        double max = MaxInput!.Value;
                        bool valid = v < max;
                        if (!valid)
                            v = max - 1;
                        return valid;
                    }
                }.Assign(out MinInput)
                .OnEvent(UINumberInput.ValueChanged, (input, _) =>
                {
                    Min = (float)input.Value;
                }),
                new UILabel
                {
                    Top = y,
                    Left = new(1, .2f),
                    Height = 20,
                    Width = new(-4, .2f),
                    TextAlign = new(.5f),
                    Text = "≤ v ≤",
                    WordWrap = false
                },
                new UINumberInput
                {
                    Top = y,
                    Left = new(2, .4f),
                    Height = 20,
                    Width = new(-4, .2f),
                    TextAlign = new(.5f),
                    Value = 1,
                    ValueValidator = (ref double v) =>
                    {
                        double min = MinInput!.Value;
                        bool valid = v > min;
                        if (!valid)
                            v = min + 1;
                        return valid;
                    }
                }.Assign(out MaxInput)
                .OnEvent(UINumberInput.ValueChanged, (input, _) =>
                {
                    Max = (float)input.Value;
                }),
                new UILabel
                {
                    Top = y,
                    Left = new(3, .6f),
                    Height = 20,
                    Width = new(0, .2f),
                    TextAlign = new(1, .5f),
                    Text = "Step:"
                },
                new UINumberInput
                {
                    Top = y,
                    Height = 20,
                    Left = new(4, .8f),
                    Width = new(-4, .2f),
                    TextAlign = new(.5f),
                    Value = 1
                }.Assign(out StepInput)
                .OnEvent(UINumberInput.ValueChanged, (input, _) =>
                {
                    Step = (float)input.Value;

                    for (int i = 0; i < ValueScrolls.Length; i++)
                    {
                        float value = ApplyStepToValue(ValueArray[i]);
                        if (value == ValueScrolls[i].ScrollPosition)
                            return;

                        ValueArray[i] = value;
                        ValueScrolls[i].ScrollPosition = value;
                    }
                }),

                new UIFlow
                {
                    Top = y + 25,
                    Height = 0,
                    ElementSpacing = 5
                }.Execute(flow =>
                {
                    for (int i = 0; i < 4; i++)
                    {
                        flow.Elements.Add(new UIButton
                        {
                            Image = Content.AnimationAssets,
                            ImageFrame = new(i * 16, 0, 16, 16),
                            ImageAlign = new(.5f),

                            SelectedBackColor = Color.White,

                            Width = 20,
                            Height = 20,
                            Selected = i == 0,
                            RadioGroup = group,
                            RadioTag = (AnimationMode)i,
                            CanDeselect = false,

                        });
                    }
                    flow.Elements.Add(new UIContainer
                    {
                        Width = 80,
                        Height = 20,

                        Elements =
                        {
                            new UILabel
                            {
                                Top = 2,
                                Width = 40,
                                Height = 18,
                                Text = "Speed:",
                                TextAlign = new(0, .5f),
                            },
                            new UINumberInput
                            {
                                Width = new(-40, 1),
                                Left = new(0, 1, -1),
                                Value = 1
                            }.OnEvent(UINumberInput.ValueChanged, (inp, _) => Speed = (float)inp.Value)
                        }
                    });
                })
            };

            foreach (UIScrollBar scroll in ValueScrolls)
                Elements.Add(scroll);

            for (int i = 0; i < AnimationActive.Length; i++)
            {
                UIButton play = new()
                {
                    RadioGroup = new(),
                    Width = 16,
                    Height = 16,

                    Top = i * 20,

                    Image = Content.AnimationAssets,
                    ImageFrame = new(64, 0, 16, 16),
                    ImageAlign = new(.5f),

                    SelectedBackColor = Color.White
                };
                int index = i;
                play.OnEvent(ClickEvent, (btn, _) => AnimationActive[index] = btn.Selected);
                Elements.Add(play);
                PlayButtons[i] = play;
            }

            Values = new(this);
        }

        protected override void UpdateSelf()
        {
            for (int i = 0; i < AnimationActive.Length; i++)
            {
                UIScrollBar scroll = ValueScrolls[i];

                scroll.ScrollMax = Max;
                scroll.ScrollMin = Min;

                if (!AnimationActive[i])
                    continue;

                float step = Step * Speed;

                switch (CurrentAnimation)
                {
                    case AnimationMode.ForwardBackward:
                        bool negStep = step < 0;
                        step *= AnimationReverse[i] ? -1 : 1;

                        float value = scroll.ScrollPosition + step;

                        if (value > scroll.ScrollMax)
                            AnimationReverse[i] = !negStep;
                        
                        else if (value < scroll.ScrollMin)
                            AnimationReverse[i] = negStep;
                        
                        scroll.ScrollPosition = value;
                        
                        break;

                    case AnimationMode.ForwardOnly:
                        value = scroll.ScrollPosition + step;

                        if (value > scroll.ScrollMax)
                            value -= scroll.ScrollMax - scroll.ScrollMin;

                        else if (value < scroll.ScrollMin)
                            value += scroll.ScrollMax - scroll.ScrollMin;

                        scroll.ScrollPosition = value;

                        break;

                    case AnimationMode.Once:
                        value = scroll.ScrollPosition + step;

                        if (value > scroll.ScrollMax)
                        {
                            value = scroll.ScrollMin;
                            AnimationActive[i] = false;
                            PlayButtons[i].Selected = false;
                        }

                        else if (value < scroll.ScrollMin)
                        {
                            value = scroll.ScrollMax;
                            AnimationActive[i] = false;
                            PlayButtons[i].Selected = false;
                        }

                        scroll.ScrollPosition = value;
                        break;

                    case AnimationMode.Incement:

                        scroll.ScrollPosition += step;

                        float a = 0;
                        float b = scroll.ScrollPosition * 2;
                        scroll.ScrollMin = Math.Min(a, b);
                        scroll.ScrollMax = Math.Max(a, b);
                        break;
                }
            }

            base.UpdateSelf();
        }

        public override void Recalculate()
        {
            base.Recalculate();

            MinHeight = Elements.Select(e => e.ScreenRect.Bottom - ScreenRect.Top - Padding.Top).Max() + Padding.Vertical;
            base.Recalculate();
        }

        public void SetValues(float[] values)
        {
            IgnoreScrollEvents = true;
            int end = Math.Min(values.Length, ValueScrolls.Length);
            for (int i = 0; i < end; i++)
            {
                float value = values[i];
                ValueScrolls[i].ScrollPosition = value;
                ValueArray[i] = ApplyStepToValue(value);
            }
            IgnoreScrollEvents = false;
        }

        float ApplyStepToValue(float value)
        {
            float absStep = MathF.Abs(Step);

            float rem = value % absStep;
            value = MathF.Floor(value / absStep) * absStep;
            value += MathF.Round(rem / absStep) * absStep;
            return value;
        }

        public record struct ValueChangedArgs(int Index, float Value);
        public class SliderValues
        {
            AnimationSlider Parent;

            public SliderValues(AnimationSlider parent)
            {
                Parent = parent;
            }

            public float this[int index]
            {
                get => Parent.ValueArray[index];
                set => Parent.ValueScrolls[index].ScrollPosition = value;
            }
        }
        enum AnimationMode
        {
            ForwardBackward,
            ForwardOnly,
            Once,
            Incement
        }
    }
}
