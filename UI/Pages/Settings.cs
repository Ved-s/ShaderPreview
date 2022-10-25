using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.UI.Elements;
using System;
using System.IO;

namespace ShaderPreview.UI.Pages
{
    public static class Settings
    {
        public static UILabel ShaderNameLabel = null!;
        public static UILabel TextureNameLabel = null!;

        public static UIPanel Page = null!;

        public static UIElement Init()
        {
            return Page = new UIPanel
            {
                Padding = 5,

                Elements =
                {
                    new UIList()
                    {
                        Elements =
                        {
                            new UIContainer
                            {
                                Height = 40,

                                Elements =
                                {
                                    new UIButton
                                    {
                                        Text = "Select shader",

                                        Width = 0,
                                        Height = 18
                                    }.OnEvent(UIElement.ClickEvent, (_, _) =>
                                    {
                                        Util.SelectFile("Select shader file", ShaderCompiler.SetShaderFilePath, "HLSL Shader (.fx)|*.fx|All files|*.*");
                                    }),
                                    new UILabel
                                    {
                                        Text = "Selected: " + Path.GetFileName(ShaderCompiler.ShaderPath),
                                        TextColor = new(.8f, .8f, .8f),
                                        Top = 22,
                                        Height = 0,
                                    }.Assign(out ShaderNameLabel)
                                }
                            },

                            new UIContainer
                            {
                                Height = 40,

                                Elements =
                                {
                                    new UIButton
                                    {
                                        Text = "Select base texture",

                                        Width = 0,
                                        Height = 18
                                    }.OnEvent(UIElement.ClickEvent, (_, _) =>
                                    {
                                        Util.SelectFile("Select texture file", file =>
                                        {
                                            try
                                            {
                                                Texture2D texture = Texture2D.FromFile(ShaderPreview.Instance.GraphicsDevice, file);
                                                ShaderPreview.BaseTexture = texture;
                                                ShaderPreview.BaseTexturePath = file;
                                                TextureNameLabel.Text = "Selected: " + Path.GetFileName(file) ?? "None";
                                            }
                                            catch (Exception e)
                                            {
                                                System.Windows.Forms.MessageBox.Show(e.ToString());
                                            }
                                        }, "Image (PNG, JPEG, GIF, PSD, BMP, HDR, TGA)|*.png;*.bmp;*.jpeg;*.jpg;*.gif;*.psd;*.hdr;*.tga|All files|*.*");
                                    }),
                                    new UILabel
                                    {
                                        Text = "Selected: " + Path.GetFileName(ShaderPreview.BaseTexturePath) ?? "None",
                                        TextColor = new(.8f, .8f, .8f),
                                        Top = 22,
                                        Height = 0,
                                    }.Assign(out TextureNameLabel)
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
