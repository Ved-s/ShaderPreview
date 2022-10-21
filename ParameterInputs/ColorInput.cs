using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.UI;
using ShaderPreview.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ShaderPreview.ParameterInputs
{
    public class ColorInput : ParameterInput
    {
        public override string DisplayName => "Color";

        Color Color = Color.Black;

        public override bool AppliesToParameter(EffectParameter parameter)
        {
            return parameter.ParameterType == EffectParameterType.Single 
                && parameter.ParameterClass == EffectParameterClass.Vector 
                && ( parameter.ColumnCount == 3 || parameter.ColumnCount == 4);
        }

        public override void UpdateSelf(EffectParameter parameter, bool selected)
        {
            if (parameter.ColumnCount == 4)
                parameter.SetValue(Color.ToVector4());
            else
                parameter.SetValue(Color.ToVector3());
        }

        protected override UIElement? GetConfigInterface()
        {
            bool alpha = ShaderCompiler.Shader?.Parameters[ParameterName]?.ColumnCount == 4;

            return new ColorSelector(alpha)
            {
                Height = alpha ? 120 : 100,
                Color = Color,
            }.OnEvent(ColorSelector.ColorChanged, (_, color) => Color = color);
        }
    }
}
