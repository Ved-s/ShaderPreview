using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderPreview.ParameterInputs
{
    public class Projection : ParameterInput
    {
        public override string DisplayName => "Projection matrix";

        public override bool AppliesToParameter(EffectParameter parameter)
        {
            return parameter.ParameterClass == EffectParameterClass.Matrix
                && parameter.ParameterType == EffectParameterType.Single
                && parameter.ColumnCount == 4
                && parameter.RowCount == 4;
        }

        public override void UpdateSelf(EffectParameter parameter, bool selected)
        {
            RenderTarget2D rt = ShaderPreview.RenderTarget;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, rt.Width, rt.Height, 0, 0, 1);
            parameter.SetValue(projection);
        }
    }
}
