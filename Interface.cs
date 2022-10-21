﻿using Microsoft.Xna.Framework;
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
using System.Linq;
using System.Threading;

namespace ShaderPreview
{
    public static class Interface
    {
        public static UIRoot Root = null!;

        public static UIResizeablePanel SidePanel = null!;
        public static UIList ParametersList = null!;
        public static UIFlow ParameterInputTypes = null!;

        public static List<UIShaderParameter> ParameterElements = new();
        public static string? SelectedParameter;
        public static UIElement? CurrentInputConfig;
        public static UIList ParameterInputDataList = null!;

        public static UIContainer ParamContainer = null!;
        public static UIContainer InputContainer = null!;

        public static UILabel SelectParameterNoInputsLabel = new()
        {
            Height = 0,
            Width = 0,
            Text = "Selected parameter has no registered inputs"
        };
        public static UILabel SelectParameterInputLabel = new()
        {
            Height = 0,
            Width = 0,
            Text = "Select parameter input type with buttons above"
        };
        public static UILabel ParameterInputNoConfigLabel = new() 
        {
            Height = 0,
            Width = 0,
            Text = "This input has no config"
        };

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
                        Padding = 5,

                        CanGrabTop = false,
                        CanGrabRight = false,
                        CanGrabBottom = false,
                        SizingChangesPosition = false,

                        Elements =
                        {
                            new UIContainer()
                            {
                                Height = new(0, 1f),

                                Elements =
                                {
                                    new UILabel()
                                    {
                                        Text = "Parameters",
                                        Height = 15,
                                        TextAlign = new(.5f, 0)
                                    },
                                    new UIList()
                                    {
                                        Top = 20,
                                        Height = new(-20, 1),
                                        Margin = 5,
                                        ElementSpacing = 2
                                    }.Assign(out ParametersList)
                                }
                            }.Assign(out ParamContainer),
                            new UIContainer()
                            {
                                Top = new(0, .5f),
                                Height = new(0, .5f),
                                Visible = false,

                                Elements =
                                {
                                    new UILabel()
                                    {
                                        Text = "Parameter input",
                                        Height = 15,
                                        TextAlign = new(.5f, 0)
                                    },
                                    new UIList()
                                    {
                                        Top = 15,

                                        Margin = 5,
                                        Height = new(-15, 1f),
                                        ElementSpacing = 5f,

                                        Elements =
                                        {
                                            new UIFlow()
                                            {
                                                Height = 0,
                                                ElementSpacing = 2
                                            }.Assign(out ParameterInputTypes)
                                        }
                                    }.Assign(out ParameterInputDataList)
                                }
                            }.Assign(out InputContainer),
                        }
                    }.Assign(out SidePanel)
                }
            };

            ShaderChanged();
        }

        public static void Update()
        {
            Root.Update();
            if (Root.GetKeyState(Keys.F12) == KeybindState.JustReleased)
            {
                Init();
                Root.Update();
            }
            if (Root.CtrlKey >= KeybindState.JustPressed && Root.GetKeyState(Keys.O) == KeybindState.JustPressed)
            {
                Thread thread = new(() =>
                {
                    System.Windows.Forms.OpenFileDialog ofd = new();
                    ofd.Title = "Select shader file";

                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        ShaderCompiler.SetShaderFilePath(ofd.FileName);
                    }
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
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

        public static void ShaderChanged()
        {
            ParameterElements.Clear();
            ParametersList.Elements.Clear();

            ParameterInputTypes.Elements.Clear();

            if (SelectedParameter is not null && !ParameterInput.CurrentShaderParams.ContainsKey(SelectedParameter))
                SelectedParameter = null;

            if (ShaderCompiler.Shader is not null)
            {
                for (int i = 0; i < ShaderCompiler.Shader.Parameters.Count; i++)
                {
                    UIShaderParameter paramui = new(i);
                    ParameterElements.Add(paramui);
                    ParametersList.Elements.Add(paramui);
                }

                RadioButtonGroup group = new();
            }
            if (SelectedParameter is not null)
                ParameterChanged();

            Root.Recalculate();
        }

        public static void ParameterChanged()
        {
            ParameterInputTypes.Elements.Clear();

            if (CurrentInputConfig is not null)
                ParameterInputDataList.Elements.Remove(CurrentInputConfig);

            if (SelectedParameter is null)
            {
                ParamContainer.Height = new(0, 1);
                InputContainer.Visible = false;
                SidePanel.Recalculate();
                return;
            }

            ParamContainer.Height = new(0, .5f);
            InputContainer.Visible = true;

            ParameterInput? selected;
            if (!ParameterInput.CurrentActiveParams.TryGetValue(SelectedParameter, out selected))
                selected = null;

            RadioButtonGroup group = new();
            bool anyInputs = false;

            foreach (ParameterInput input in ParameterInput.CurrentShaderParams[SelectedParameter])
            {
                UIButton btn = new()
                {
                    Width = 0,
                    Height = 0,
                    Text = input.DisplayName,
                    RadioTag = input,
                    Selected = input == selected,
                    RadioGroup = group,
                };
                ParameterInputTypes.Elements.Add(btn);
                anyInputs = true;
            }

            group.ButtonClicked += (btn, id) =>
            {
                if (CurrentInputConfig is not null)
                    ParameterInputDataList.Elements.Remove(CurrentInputConfig);

                if (id is not ParameterInput input)
                {
                    ParameterInput.CurrentActiveParams.Remove(SelectedParameter);
                    ParameterInputDataList.Elements.Add(CurrentInputConfig = SelectParameterInputLabel);
                }
                else
                {
                    ParameterInput.CurrentActiveParams[SelectedParameter] = input;
                    ParameterInputDataList.Elements.Add(CurrentInputConfig = input.ConfigInterface ?? ParameterInputNoConfigLabel);
                }
                ParameterInputDataList.Recalculate();
            };

            if (ParameterInput.CurrentActiveParams.TryGetValue(SelectedParameter, out ParameterInput? active))
                ParameterInputDataList.Elements.Add(CurrentInputConfig = active.ConfigInterface ?? ParameterInputNoConfigLabel);
            else
                ParameterInputDataList.Elements.Add(CurrentInputConfig = anyInputs ? SelectParameterInputLabel : SelectParameterNoInputsLabel);

            SidePanel.Recalculate();
            SidePanel.Recalculate();
        }

        public class UIShaderParameter : UIElement
        {
            int Parameter;

            public UIShaderParameter(int paramIndex)
            {
                Parameter = paramIndex;
                Height = 30;
            }

            protected override void UpdateSelf()
            {
                if (Hovered && Root.MouseLeftKey == KeybindState.JustPressed)
                {
                    string? newName = ShaderCompiler.Shader?.Parameters[Parameter].Name;
                    if (newName == SelectedParameter)
                        SelectedParameter = null;
                    else
                        SelectedParameter = newName;
                    ParameterChanged();
                }
            }

            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                EffectParameter? param = ShaderCompiler.Shader?.Parameters[Parameter];

                bool selected = param is not null && param.Name == SelectedParameter;

                spriteBatch.FillRectangle(ScreenRect, selected ? new(72, 72, 72) : Hovered ? new(64, 64, 64) : new(48, 48, 48));
                spriteBatch.DrawRectangle(ScreenRect, new(100, 100, 100));

                if (Font is not null)
                {
                    if (param is null)
                    {
                        spriteBatch.DrawString(Font, "Unknown parameter", (ScreenRect.Position + new Vec2(5, (ScreenRect.Height - (Font.LineSpacing - 4)) / 2)).Ceiling(), Color.White);
                    }
                    else
                    {
                        string data;

                        switch (param.ParameterClass)
                        {
                            case EffectParameterClass.Struct:
                                data = "struct data";
                                break;

                            case EffectParameterClass.Matrix:
                                data = "matrix data";
                                break;

                            case EffectParameterClass.Object:
                                data = "object";
                                break;

                            default:
                                data = GetParamValue(param);
                                break;
                        }

                        IEnumerable<(string, Color)> strings = Util.Enumerate(
                            (GetParamType(param), Color.DodgerBlue),
                            ($" {param.Name} = ", new Color(.9f, .9f, .9f)),
                            (data, Color.White)
                            );

                        spriteBatch.DrawStrings(Font, strings, (ScreenRect.Position + new Vec2(5, (ScreenRect.Height - (Font.LineSpacing - 3) * 2) / 2)).Floor());

                        ParameterInput? input;
                        if (!ParameterInput.CurrentActiveParams.TryGetValue(param.Name, out input))
                            input = null;

                        string boundTo = $"Bound to: {input?.DisplayName ?? "Nothing"}";

                        spriteBatch.DrawString(Font, boundTo, (ScreenRect.Position + new Vec2(5, (ScreenRect.Height - (Font.LineSpacing - 3) * 2) / 2 + (Font.LineSpacing - 3))).Floor(), Color.White);
                    }
                }
            }

            string GetParamValue(EffectParameter param)
            {
                switch (param.ParameterType)
                {
                    case EffectParameterType.Void:
                        return "void";

                    case EffectParameterType.Bool:
                        return "(" + string.Join(", ", param.GetValueBoolean()) + ")";

                    case EffectParameterType.Int32:
                        if (param.ParameterClass == EffectParameterClass.Scalar)
                            return param.GetValueInt32().ToString();
                        return "(" + string.Join(", ", param.GetValueInt32Array()) + ")";

                    case EffectParameterType.Single:
                        if (param.ParameterClass == EffectParameterClass.Scalar)
                            return param.GetValueSingle().ToString("0.##", CultureInfo.InvariantCulture);
                        return "(" + string.Join(", ", param.GetValueSingleArray().Select(f => f.ToString("0.##", CultureInfo.InvariantCulture))) + ")";

                    case EffectParameterType.String:
                        return param.GetValueString();

                    case EffectParameterType.Texture:
                        return "texture data";

                    case EffectParameterType.Texture1D:
                        return "texture1D data";

                    case EffectParameterType.Texture2D:
                        return "texture2D data";

                    case EffectParameterType.Texture3D:
                        return "texture3D data";

                    case EffectParameterType.TextureCube:
                        return "textureCube data";
                }

                return "";
            }
            string GetParamType(EffectParameter param)
            {
                switch (param.ParameterClass)
                {
                    case EffectParameterClass.Scalar:
                        return GetParamType(param.ParameterType);

                    case EffectParameterClass.Vector:
                        return GetParamType(param.ParameterType) + param.ColumnCount.ToString();

                    case EffectParameterClass.Matrix:
                        return $"{GetParamType(param.ParameterType)}{param.ColumnCount}x{param.RowCount}";

                    case EffectParameterClass.Object:
                        return "object";

                    case EffectParameterClass.Struct:
                        return "struct";
                }

                return "";
            }
            string GetParamType(EffectParameterType type)
            {
                return type switch
                {
                    EffectParameterType.Void => "void",
                    EffectParameterType.Bool => "bool",
                    EffectParameterType.Int32 => "int",
                    EffectParameterType.Single => "float",
                    EffectParameterType.String => "string",
                    EffectParameterType.Texture => "texture",
                    EffectParameterType.Texture1D => "texture1D",
                    EffectParameterType.Texture2D => "texture2D",
                    EffectParameterType.Texture3D => "texture3D",
                    EffectParameterType.TextureCube => "textureCube",
                    _ => ""
                };
            }
        }
    }
}
