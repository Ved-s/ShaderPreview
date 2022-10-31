using Microsoft.Xna.Framework.Graphics;
using System;

namespace ShaderPreview.ParameterInputs
{
    public class Time : ParameterInput
    {
        public override string DisplayName => "Time";

        public override bool AppliesToParameter(EffectParameter parameter)
        {
            return parameter.ParameterType == EffectParameterType.Single && parameter.ParameterClass == EffectParameterClass.Scalar;
        }

        public override void UpdateSelf(EffectParameter parameter, bool selected)
        {
            parameter.SetValue((float)ShaderPreview.GameTime.TotalGameTime.TotalSeconds);
        }
    }
}
