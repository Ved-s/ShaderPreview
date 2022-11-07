using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShaderPreview;
using ShaderPreview.Structures;
using ShaderPreview.UI.Elements;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ShaderPreview.ParameterInputs
{
    public class AbsolutePos : ParameterInput
    {
        public override string DisplayName => "Absolute pos";

        public Vec2 Pos = new(.5f);
        public Vec2 PointerSize = new(10);

        public bool TexturePosAsOrigin = false;
        public bool ReverseX = false;
        public bool ReverseY = false;

        bool Grabbed;

        public override bool AppliesToParameter(EffectParameter parameter)
        {
            return parameter.ParameterType == EffectParameterType.Single
                && parameter.ParameterClass == EffectParameterClass.Vector
                && parameter.ColumnCount == 2;
        }

        public override void UpdateSelf(EffectParameter parameter, bool selected)
        {
            Vec2 origin = TexturePosAsOrigin ? ShaderPreview.TextureScreenRect.Position : Vec2.Zero;
            Vec2 originSize = TexturePosAsOrigin ? ShaderPreview.TextureScreenRect.Size : ShaderPreview.TextureMaxScreenRect.Size;

            if (selected)
            {
                if (!Grabbed)
                {
                    Rect pointerRect = new(origin + Pos * originSize - PointerSize / 2, PointerSize);
                    if (pointerRect.Contains(Interface.Root.MousePosition) && Interface.Root.MouseLeftKey == KeybindState.JustPressed)
                    {
                        Grabbed = true;
                    }
                }
                if (Grabbed)
                {
                    Pos = (Interface.Root.MousePosition - origin) / originSize;

                    if (Interface.Root.MouseState.LeftButton == ButtonState.Released)
                    {
                        Grabbed = false;
                        Pos = Pos.ClampedTo(new(0, 0, 1, 1));
                    }
                }
            }

            Vec2 resultPos = Pos * originSize + origin;
            if (ReverseX) resultPos.X = originSize.X - resultPos.X;
            if (ReverseY) resultPos.Y = originSize.Y - resultPos.Y;

            parameter.SetValue(resultPos);
        }

        public override void DrawSelf(SpriteBatch spriteBatch, bool selected)
        {
            Vec2 size = selected ? PointerSize : PointerSize / 2;

            Vec2 origin = TexturePosAsOrigin ? ShaderPreview.TextureScreenRect.Position : Vec2.Zero;
            Vec2 originSize = TexturePosAsOrigin ? ShaderPreview.TextureScreenRect.Size : ShaderPreview.TextureMaxScreenRect.Size;

            Vec2 pos = origin + Pos * originSize;

            Rect pointerRect = new(pos - size / 2, size);

            spriteBatch.FillRectangle(pointerRect, Color.Yellow * (selected ? .8f : 0.3f));

            if (selected)
                spriteBatch.DrawStringShaded(ShaderPreview.Consolas10, ParameterName, pos + new Vec2(7, 2), Color.White, Color.Black);
            else
                spriteBatch.DrawString(ShaderPreview.Consolas10, ParameterName, pos + new Vec2(7, 2), Color.White * .3f);
        }

        protected override UIElement? GetConfigInterface()
        {
            return new UIList
            {
                AutoSize = true,
                ElementSpacing = 5,
                Height = 0,
                Elements =
                {
                    new UIFlow
                    {
                        Height = 0,
                        ElementSpacing = 5,
                        Elements =
                        {
                            new UILabel
                            {
                                Text = "Origin:",
                                TextAlign = new(0, .5f),
                                Margin = { Top = 3 },
                                Width = 45,
                                Height = 15
                            },
                            new UIButton
                            {
                                Text = TexturePosAsOrigin ? "Texture" : "Screen",
                                Width = 80,
                                Height = 18
                            }.OnEvent(UIElement.ClickEvent, (btn, _) =>
                            {
                                TexturePosAsOrigin = !TexturePosAsOrigin;
                                btn.Text = TexturePosAsOrigin ? "Texture" : "Screen";
                            })
                        }
                    },
                    new UIFlow
                    {
                        Height = 0,
                        ElementSpacing = 5,
                        Elements =
                        {
                            new UIButton
                            {
                                Text = "Reverse X",
                                Selectable = true,
                                Selected = ReverseX,
                                Width = 0,
                                Height = 0,
                                SelectedBackColor = Color.White,
                                SelectedTextColor = Color.Black
                            }.OnEvent(UIElement.ClickEvent, (btn, _) =>
                            {
                                ReverseX = btn.Selected;
                            }),
                            new UIButton
                            {
                                Text = "Reverse Y",
                                Selectable = true,
                                Selected = ReverseY,
                                Width = 0,
                                Height = 0,
                                SelectedBackColor = Color.White,
                                SelectedTextColor = Color.Black
                            }.OnEvent(UIElement.ClickEvent, (btn, _) =>
                            {
                                ReverseY = btn.Selected;
                            })
                        }
                    }
                }
            };
        }

        public override JsonNode SaveState()
        {
            return new JsonObject
            {
                ["pos"] = JsonSerializer.SerializeToNode(Pos),
                ["texture"] = TexturePosAsOrigin,
                ["revx"] = ReverseX,
                ["revy"] = ReverseY
            };
        }

        public override void LoadState(JsonNode node)
        {
            if (node is not JsonObject obj)
                return;

            if (obj.TryGet("pos", out JsonNode? pos))
                Pos = JsonSerializer.Deserialize<Vec2>(pos);

            if (obj.TryGet("texture", out bool texture))
                TexturePosAsOrigin = texture;

            if (obj.TryGet("revx", out bool revx))
                ReverseX = revx;

            if (obj.TryGet("revy", out bool revy))
                ReverseY = revy;
        }
    }
}
