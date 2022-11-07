using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.UI.Elements;
using System;
using System.Diagnostics;
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
                        ElementSpacing = 3,

                        Elements =
                        {
                            new UIButton
                            {
                                Text = "Select shader",

                                Width = 0,
                                Height = 18
                            }.OnEvent(UIElement.ClickEvent, (_, _) =>
                            {
                                Util.SelectFileToOpen("Select shader file", ShaderCompiler.SetShaderFilePath, "HLSL Shader (.fx)|*.fx|All files|*.*");
                            }),
                            new UILabel
                            {
                                Text = "Selected: " + Path.GetFileName(ShaderCompiler.ShaderPath),
                                TextColor = new(.8f, .8f, .8f),
                                Height = 0,
                            }.Assign(out ShaderNameLabel),
                            new UIButton
                            {
                                Text = "Open in editor",

                                Width = 0,
                                Height = 18
                            }.OnEvent(UIElement.ClickEvent, (_, _) =>
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = ShaderCompiler.ShaderPath,
                                    UseShellExecute = true
                                });
                            }),

                            new UIElement() { Height = 10 },

                            new UIButton
                            {
                                Text = "Select base texture",

                                Width = 0,
                                Height = 18
                            }.OnEvent(UIElement.ClickEvent, (_, _) =>
                            {
                                Util.SelectFileToOpen("Select texture file", file =>
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
                                Text = "Selected: " + (Path.GetFileName(ShaderPreview.BaseTexturePath) ?? "None"),
                                TextColor = new(.8f, .8f, .8f),
                                Height = 0,
                            }.Assign(out TextureNameLabel)
                       
                        }
                    }
                }
            };
        }
    }
}
