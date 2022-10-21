using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ShaderPreview
{
    public static class Extensions
    {
        static Func<EffectParameter, object> EffectParameterDataGetter =
            typeof(EffectParameter).GetProperty("Data", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetGetMethod(true)!.CreateDelegate<Func<EffectParameter, object>>();

        static Action<EffectParameter, ulong> EffectParameterStateKeySetter =
            typeof(EffectParameter).GetProperty("StateKey", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetSetMethod(true)!.CreateDelegate<Action<EffectParameter, ulong>>();

        static Func<ulong> EffectParameterNextStateKeyGetter =
            typeof(EffectParameter).GetProperty("NextStateKey", BindingFlags.Static | BindingFlags.NonPublic)!
            .GetGetMethod(true)!.CreateDelegate<Func<ulong>>();

        static Action<ulong> EffectParameterNextStateKeySetter =
            typeof(EffectParameter).GetProperty("NextStateKey", BindingFlags.Static | BindingFlags.NonPublic)!
            .GetSetMethod(true)!.CreateDelegate<Action<ulong>>();

        public static object GetRawData(this EffectParameter param)
        {
            return EffectParameterDataGetter(param);
        }

        public static void AdvanceState(this EffectParameter param)
        {
            ulong state = EffectParameterNextStateKeyGetter() + 1;
            EffectParameterNextStateKeySetter(state);
            EffectParameterStateKeySetter(param, state);
        }

    }
}
