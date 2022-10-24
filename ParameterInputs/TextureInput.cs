using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.UI.Elements;
using System;
using System.IO;

namespace ShaderPreview.ParameterInputs
{
	public class TextureInput : ParameterInput
    {
        public Texture2D? Texture;

        public override string DisplayName => "Texture";

        public override bool AppliesToParameter(EffectParameter parameter)
        {
            return parameter.ParameterType >= EffectParameterType.Texture && parameter.ParameterType <= EffectParameterType.Texture2D
                && parameter.ParameterClass == EffectParameterClass.Object;
        }

        public override void UpdateSelf(EffectParameter parameter, bool selected)
        {
            parameter.SetValue(Texture ?? ShaderPreview.Pixel);
        }

		protected override UIElement? GetConfigInterface()
		{
            UIButton button = new()
            {
                Height = 20,
                Text = "Select texture",
                BorderColor = new(100, 100, 100)
            };
			button.OnEvent(UIElement.ClickEvent, (_, value) =>
			{
                Util.SelectFile("Select texture", (ShaderPatch) =>
                {
                    try
                    {
                        Texture = Texture2D.FromFile(ShaderPreview.Instance.Graphics.GraphicsDevice, ShaderPatch);
                    }
                    catch
					{
                        System.Windows.Forms.MessageBox.Show("Error happened!");
					}
                }, "Image (PNG, JPEG, GIF, PSD, BMP, HDR, TGA)|*.png;*.bmp;*.jpeg;*.jpg;*.gif;*.psd;*.hdr;*.tga|All files|*.*");
			});
			return button;
		}
	}
}
