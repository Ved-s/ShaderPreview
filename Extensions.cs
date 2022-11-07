using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
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

        public static bool TryGet<T>(this JsonObject node, string key, [NotNullWhen(true)] out T? value, bool allowNull = false)
        {
            value = default;
            if (!node.TryGetPropertyValue(key, out var subNode))
                return false;

            if (subNode is null)
                return allowNull;

            if (subNode is T t)
            {
                value = t;
                return true;
            }
            else if (subNode is JsonValue v)
            {
                return v.TryGetValue(out value);
            }
            return false;
        }

        public static bool IsNullEmptyOrWhitespace(this string str)
        {
            return string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
        }

        public static JsonObject Save(this IState state) => IState.Save(state);
    }
}
