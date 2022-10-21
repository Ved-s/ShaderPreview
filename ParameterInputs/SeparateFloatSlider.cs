using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.UI;
using ShaderPreview.UI.Elements;
using System;

namespace ShaderPreview.ParameterInputs
{
    public class SeparateFloatSlider : ParameterInput
    {
        public override string DisplayName => "Sliders (separate)";

        public float[] Values = null!;

        public override bool AppliesToParameter(EffectParameter parameter)
        {
            return parameter.ParameterType == EffectParameterType.Single && parameter.RowCount == 1;
        }

        public override ParameterInput NewInstance(EffectParameter parameter)
        {
            return new SeparateFloatSlider()
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
            UIList container = new UIList
            {
                AutoSize = true,
                ElementSpacing = 5,
                Height = 0
            };

            for (int i = 0; i < Values.Length; i++)
            {
                AnimationSlider slider = new(1)
                {
                    Height = 0,
                    BorderColor = new(100, 100, 100)
                };
                int index = i;

                slider.Values[0] = Values[i];
                slider.OnEvent(AnimationSlider.ValueChanged, (_, value) => { Values[index] = value.Value; });

                container.Elements.Add(slider);

                slider.Recalculate();
                slider.Recalculate();
            }

            return container;
        }
    }
}
