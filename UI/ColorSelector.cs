using Microsoft.Xna.Framework;
using ShaderPreview.UI.Elements;
using ShaderPreview.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShaderPreview.UI
{
    public class ColorSelector : UIPanel
    {
        public static readonly ElementEvent<Color, ColorSelector> ColorChanged = new();

        public Color Color
        {
            get => new(Red.CurrentValue, Green.CurrentValue, Blue.CurrentValue, Alpha?.CurrentValue ?? 1);
            set
            {
                IgnoreChanges = true;

                Red.CurrentValue   = value.R / 255f;
                Green.CurrentValue = value.G / 255f;
                Blue.CurrentValue  = value.B / 255f;

                if (Alpha is not null)
                    Alpha.CurrentValue = value.A / 255f;

                IgnoreChanges = false;
                UpdateColors(false);
            }
        }

        private UIGradientSlider Red, Green, Blue;
        private UIGradientSlider? Alpha;
        private UIInput ColorInput;
        private bool IgnoreChanges;
        private static HashSet<char> HexDigits = new("0123456789ABCDEFabcdef");

        public ColorSelector(bool withAlpha)
        {
            Padding = new(8, 8);

            float alphaOff = 0;

            if (withAlpha)
            {
                Elements.Add(new UIGradientSlider
                {
                    Height = 10,
                    Top = 0,
                    BarPadding = -3,
                    Start = new(0, 0, 0, 0f),
                    BorderColor = new(100, 100, 100),
                    BarSize = 8,
                    BarSizeAbsolute = true
                }.Assign(out Alpha)!);
                alphaOff = 20;
            }
            Elements.Add(new UIGradientSlider
            {
                Height = 10,
                Top = alphaOff + 0,
                BarPadding = -3,
                End = new(1, 0, 0, 1f),
                BorderColor = new(100, 100, 100),
                BarSize = 8,
                BarSizeAbsolute = true
            }.Assign(out Red));
            Elements.Add(new UIGradientSlider
            {
                Height = 10,
                Top = alphaOff + 20,
                BarPadding = -3,
                End = new(0, 1, 0, 1f),
                BorderColor = new(100, 100, 100),
                BarSize = 8,
                BarSizeAbsolute = true
            }.Assign(out Green));
            Elements.Add(new UIGradientSlider
            {
                Height = 10,
                Top = alphaOff + 40,
                BarPadding = -3,
                End = new(0, 0, 1, 1f),
                BorderColor = new(100, 100, 100),
                BarSize = 8,
                BarSizeAbsolute = true
            }.Assign(out Blue));
            Elements.Add(new UIInput
            {
                Top = new(0, 1, -1),
                Height = 20,
                TextAlign = new(.5f, 0),
                Multiline = false
            }.Assign(out ColorInput));

            Func<UIScrollBar, float, bool> preCall = (_, _) => Events.PreCall(ColorChanged, Color);

            Alpha?.AddPreEventCallback(UIScrollBar.ScrollChanged, preCall);
            Red.AddPreEventCallback(UIScrollBar.ScrollChanged, preCall);
            Green.AddPreEventCallback(UIScrollBar.ScrollChanged, preCall);
            Blue.AddPreEventCallback(UIScrollBar.ScrollChanged, preCall);
            
            Alpha?.AddPostEventCallback(UIScrollBar.ScrollChanged, (_, val) =>
            {
                if (IgnoreChanges)
                    return;

                UpdateColors(true);
            });
            Red.AddPostEventCallback(UIScrollBar.ScrollChanged, (_, val) =>
            {
                if (IgnoreChanges)
                    return;

                UpdateColors(true);
            });
            Green.AddPostEventCallback(UIScrollBar.ScrollChanged, (_, val) =>
            {
                if (IgnoreChanges)
                    return;

                UpdateColors(true);
            });
            Blue.AddPostEventCallback(UIScrollBar.ScrollChanged, (_, val) =>
            {
                if (IgnoreChanges)
                    return;

                UpdateColors(true);
            });

            ColorInput.OnEvent(UIElement.ActiveChangedEvent, (input, active) =>
            {
                if (!active)
                    UpdateColors(false);
                
            });
            ColorInput.BeforeEvent(UIInput.CharacterTypedEvent, (input, args) =>
            {
                return HexDigits.Contains(args.Character)
                || args.Character == ','
                || args.Character == ' ';
            });

            ColorInput.OnEvent(UIInput.TextChangedEvent, (input, _) =>
            {
                if (IgnoreChanges)
                    return;

                string text = input.Text;
                bool allHex = text.Length > 0 && text.All(c => HexDigits.Contains(c));
                if (text.Length == 3 && allHex) // RGB
                {
                    IgnoreChanges = true;
                    Red.CurrentValue = ParseHexChar(text[0]) / 15f;
                    Green.CurrentValue = ParseHexChar(text[1]) / 15f;
                    Blue.CurrentValue = ParseHexChar(text[2]) / 15f;
                    IgnoreChanges = false;
                    IgnoreChanges = false;
                    UpdateColors(true);
                    return;
                }
                if (Alpha is not null && text.Length == 4 && allHex) //  ARGB
                {
                    IgnoreChanges = true;
                    Alpha.CurrentValue = ParseHexChar(text[0]) / 15f;
                    Red.CurrentValue = ParseHexChar(text[1]) / 15f;
                    Green.CurrentValue = ParseHexChar(text[2]) / 15f;
                    Blue.CurrentValue = ParseHexChar(text[3]) / 15f;
                    IgnoreChanges = false;
                    UpdateColors(true);
                    return;
                }
                if (text.Length == 6 && allHex) // RRGGBB
                {
                    IgnoreChanges = true;
                    Red.CurrentValue = (ParseHexChar(text[0]) * 16 + ParseHexChar(text[1])) / 255f;
                    Green.CurrentValue = (ParseHexChar(text[2]) * 16 + ParseHexChar(text[3])) / 255f;
                    Blue.CurrentValue = (ParseHexChar(text[4]) * 16 + ParseHexChar(text[5])) / 255f;
                    IgnoreChanges = false;
                    UpdateColors(true);
                    return;
                }
                if (Alpha is not null && text.Length == 8 && allHex) // AARRGGBB
                {
                    IgnoreChanges = true;
                    Alpha.CurrentValue = (ParseHexChar(text[0]) * 16 + ParseHexChar(text[1])) / 255f;
                    Red.CurrentValue = (ParseHexChar(text[2]) * 16 + ParseHexChar(text[3])) / 255f;
                    Green.CurrentValue = (ParseHexChar(text[4]) * 16 + ParseHexChar(text[5])) / 255f;
                    Blue.CurrentValue = (ParseHexChar(text[6]) * 16 + ParseHexChar(text[7])) / 255f;
                    IgnoreChanges = false;
                    UpdateColors(true);
                    return;
                }
                if (text.Count(c => c == ',') == 2)
                {
                    string[] split = text.Split(',', StringSplitOptions.TrimEntries);

                    if (byte.TryParse(split[0], out byte r) && byte.TryParse(split[1], out byte g) && byte.TryParse(split[2], out byte b))
                    {
                        IgnoreChanges = true;
                        Red.CurrentValue = r / 255f;
                        Green.CurrentValue = g / 255f;
                        Blue.CurrentValue = b / 255f;
                        IgnoreChanges = false;
                        UpdateColors(true);
                        
                        return;
                    }
                }
                if (Alpha is not null && text.Count(c => c == ',') == 3)
                {
                    string[] split = text.Split(',', StringSplitOptions.TrimEntries);

                    if (byte.TryParse(split[0], out byte a) 
                     && byte.TryParse(split[1], out byte r) 
                     && byte.TryParse(split[2], out byte g) 
                     && byte.TryParse(split[3], out byte b))
                    {
                        IgnoreChanges = true;
                        Alpha.CurrentValue = a / 255f;
                        Red.CurrentValue = r / 255f;
                        Green.CurrentValue = g / 255f;
                        Blue.CurrentValue = b / 255f;
                        IgnoreChanges = false;
                        UpdateColors(true);
                        
                    }
                }
            });

            UpdateColors(false);
        }

        void UpdateColors(bool triggerEvent)
        {
            byte r = (byte)(Red.CurrentValue * 255);
            byte g = (byte)(Green.CurrentValue * 255);
            byte b = (byte)(Blue.CurrentValue * 255);
            byte a = Alpha is null ? (byte)255 : (byte)(Alpha.CurrentValue * 255);

            Red.Start.G = g;
            Red.Start.B = b;
            Red.End.G = g;
            Red.End.B = b;

            Green.Start.R = r;
            Green.Start.B = b;
            Green.End.R = r;
            Green.End.B = b;

            Blue.Start.R = r;
            Blue.Start.G = g;
            Blue.End.R = r;
            Blue.End.G = g;

            if (Alpha is not null)
                Alpha.End = new(r, g, b, (byte)255);

            IgnoreChanges = true;
            if (!ColorInput.Active)
            {
                if (Alpha is not null)
                    ColorInput.Text = $"{a}, {r}, {g}, {b}";
                else
                    ColorInput.Text = $"{r}, {g}, {b}";
            }
            IgnoreChanges = false;

            if (triggerEvent)
                Events.PostCall(ColorChanged, Color);
        }
        int ParseHexChar(char chr)
        {
            if (chr >= '0' && chr <= '9')
                return chr - '0';

            if (chr >= 'A' && chr <= 'F')
                return chr - 'A' + 10;

            if (chr >= 'a' && chr <= 'f')
                return chr - 'a' + 10;
            return 0;
        }
    }
}
