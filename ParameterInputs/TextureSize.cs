using Microsoft.Xna.Framework.Graphics;
using System;

namespace ShaderPreview.ParameterInputs
{
    public class TextureSize : ParameterInput
    {
        public override string DisplayName => "Texture size";

        public override bool AppliesToParameter(EffectParameter parameter)
        {
            return parameter.ParameterType == EffectParameterType.Single 
                && parameter.ParameterClass == EffectParameterClass.Vector
                && parameter.ColumnCount == 2;
        }

        public override void UpdateSelf(EffectParameter parameter, bool selected)
        {
            parameter.SetValue(ShaderPreview.TextureScreenRect.Size);
        }
    }
}
