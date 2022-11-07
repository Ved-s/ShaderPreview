using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShaderPreview.ParameterInputs;
using ShaderPreview.Structures;
using ShaderPreview.UI;
using ShaderPreview.UI.Elements;
using ShaderPreview.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
namespace ShaderPreview
{
    public static class Interface
    {
        public static UIRoot Root = null!;

        public static UIResizeablePanel SidePanel = null!;

        public static bool Initialized { get; private set; } = false;

        public static void Init()
        {
            Root = new UIRoot(ShaderPreview.Instance)
            {
                Font = ShaderPreview.Consolas10,
                Elements =
                {
                    new UIResizeablePanel()
                    {
                        Left = new(0, 1, -1),

                        Width = 200,
                        Height = new(0, 1),
                        MaxWidth = new(0, .6f),
                        MinWidth = new(0, .2f),

                        Margin = 5,

                        BackColor = Color.Transparent,
                        BorderColor = Color.Transparent,

                        CanGrabTop = false,
                        CanGrabRight = false,
                        CanGrabBottom = false,
                        SizingChangesPosition = false,

                        Elements =
                        {
                            new TabContainer
                            {
                                Tabs =
                                {
                                    new()
                                    {
                                        Name = "Settings",
                                        Element = UI.Pages.Settings.Init() 
                                    },
                                    new()
                                    {
                                        Name = "Shader parameters",
                                        Element = UI.Pages.ShaderParameters.Init()
                                    }
                                }
                            }
                        }
                    }.Assign(out SidePanel),
                }
            };

            Initialized = true;
            UI.Pages.ShaderParameters.ShaderChanged();
        }

        public static void Update()
        {
            Root.Update();
            if (Root.GetKeyState(Keys.F12) == KeybindState.JustReleased)
            {
                ParameterInput.ResetUI();
                Init();
                Root.Recalculate();
                Root.Update();
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            Root.Draw(spriteBatch);
        }

        public static void SizeChanged()
        {
            Root.Recalculate();
        }

        
    }
}
