using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.UI;
using ShaderPreview.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ShaderPreview.ParameterInputs
{
    public class Color : ParameterInput
    {
        public override string DisplayName => "Color";

        Microsoft.Xna.Framework.Color CurrentColor = Microsoft.Xna.Framework.Color.Black;

        public override bool AppliesToParameter(EffectParameter parameter)
        {
            return parameter.ParameterType == EffectParameterType.Single 
                && parameter.ParameterClass == EffectParameterClass.Vector 
                && ( parameter.ColumnCount == 3 || parameter.ColumnCount == 4);
        }

        public override void UpdateSelf(EffectParameter parameter, bool selected)
        {
            if (parameter.ColumnCount == 4)
                parameter.SetValue(CurrentColor.ToVector4());
            else
                parameter.SetValue(CurrentColor.ToVector3());
        }

        protected override UIElement? GetConfigInterface()
        {
            bool alpha = ShaderCompiler.Shader?.Parameters[ParameterName]?.ColumnCount == 4;

            return new ColorSelector(alpha)
            {
                Height = alpha ? 120 : 100,
                Color = CurrentColor,
            }.OnEvent(ColorSelector.ColorChanged, (_, color) => CurrentColor = color);
        }

        public override JsonNode SaveState()
        {
            return new JsonObject
            {
                ["color"] = CurrentColor.PackedValue
            };
        }

        public override void LoadState(JsonNode node)
        {
            if (node is not JsonObject obj)
                return;

            if (obj.TryGet("color", out uint color))
                CurrentColor.PackedValue = color;
        }
    }
}
