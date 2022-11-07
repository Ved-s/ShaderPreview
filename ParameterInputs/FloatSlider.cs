using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.UI;
using ShaderPreview.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ShaderPreview.ParameterInputs
{
    public class FloatSlider : ParameterInput
    {
        public override string DisplayName => "Sliders";

        public float[] Values = null!;

        public override bool AppliesToParameter(EffectParameter parameter)
        {
            return parameter.ParameterType == EffectParameterType.Single && parameter.RowCount == 1;
        }

        public override ParameterInput NewInstance(EffectParameter parameter)
        {
            return new FloatSlider()
            {
                Values = new float[parameter.ColumnCount]
            };
        }

        public override void UpdateSelf(EffectParameter parameter, bool selected)
        {
            object data = parameter.GetRawData();
            if (data is float[] array)
                Array.Copy(Values, array, Values.Length);
            parameter.AdvanceState();
        }

        protected override UIElement? GetConfigInterface()
        {
            AnimationSlider slider = new(Values.Length)
            {
                Height = 0,
                BorderColor = new(100, 100, 100)
            };
            slider.SetValues(Values);
            slider.OnEvent(AnimationSlider.ValueChanged, (_, value) => { Values[value.Index] = value.Value; });

            return slider;
        }

        public override JsonNode SaveState()
        {
            return JsonSerializer.SerializeToNode(Values)!;
        }

        public override void LoadState(JsonNode node)
        {
            Values = JsonSerializer.Deserialize<float[]>(node) ?? Values;
        }
    }
}
