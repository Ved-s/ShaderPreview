using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShaderPreview.ParameterInputs
{
    public abstract class ParameterInput
    {
        public static readonly List<ParameterInput> AllInputs = new();
        public static readonly Dictionary<string, ParameterInput[]> CurrentShaderParams = new();
        public static Dictionary<string, ParameterInput> CurrentActiveParams = new();
        private static Dictionary<string, (EffectParameterType type, EffectParameterClass @class, int rows, int cols)> CurrentShaderParamInfos = new();
        private UIElement? configInterface;

        public abstract string DisplayName { get; }
        public string ParameterName { get; internal set; } = null!;
        public UIElement? ConfigInterface
        {
            get => configInterface ??= GetConfigInterface();
            protected set => configInterface = value;
        }

        public static void ShaderChanged()
        {
            Effect? shader = ShaderCompiler.Shader;

            if (shader is null)
            {
                CurrentShaderParams.Clear();
                CurrentShaderParamInfos.Clear();
                CurrentActiveParams.Clear();
                return;
            }

            IEnumerable<string> shaderParamNames = shader.Parameters.Select(p => p.Name);

            HashSet<string> set = new(CurrentShaderParams.Keys);
            set.ExceptWith(shaderParamNames);

            foreach (string key in CurrentShaderParams.Keys)
                if (CurrentShaderParamInfos.TryGetValue(key, out var info))
                    if (!set.Contains(key))
                    {
                        EffectParameter param = shader.Parameters[key];
                        if (param.ParameterClass != info.@class
                            || param.ParameterType != info.type
                            || param.RowCount != info.rows
                            || param.ColumnCount != info.cols)
                            set.Add(key);
                    }

            foreach (string remove in set) // remove
            {
                CurrentShaderParams.Remove(remove);
                CurrentActiveParams.Remove(remove);
                CurrentShaderParamInfos.Remove(remove);
            }

            set.Clear();
            set.UnionWith(shaderParamNames);
            set.ExceptWith(CurrentShaderParams.Keys);

            foreach (string paramName in set) // add
            {
                EffectParameter param = shader.Parameters[paramName];
                ParameterInput[] inputs = AllInputs
                    .Where(i => i.AppliesToParameter(param))
                    .Select(i => i.NewInstance(param))
                    .ToArray();

                foreach (ParameterInput input in inputs)
                    input.ParameterName = paramName;

                CurrentShaderParams[paramName] = inputs;
            }

            foreach (EffectParameter param in shader.Parameters)
                CurrentShaderParamInfos[param.Name] = (param.ParameterType, param.ParameterClass, param.RowCount, param.ColumnCount);

        }
        public static void Update()
        {
            Effect? shader = ShaderCompiler.Shader;
            if (shader is null)
                return;

            foreach (var kvp in CurrentActiveParams)
            {
                EffectParameter? param = shader.Parameters[kvp.Key];
                if (param is null)
                    continue;

                kvp.Value.UpdateSelf(param, UI.Pages.ShaderParameters.SelectedParameter == kvp.Key);
            }
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (var kvp in CurrentActiveParams)
                kvp.Value.DrawSelf(spriteBatch, UI.Pages.ShaderParameters.SelectedParameter == kvp.Key);

        }
        public static void Autoload()
        {
            foreach (Type type in typeof(ParameterInput).Assembly.GetExportedTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                if (!type.IsAssignableTo(typeof(ParameterInput)))
                    continue;

                AllInputs.Add((ParameterInput)Activator.CreateInstance(type)!);
            }
        }

        public abstract bool AppliesToParameter(EffectParameter parameter);
        protected virtual UIElement? GetConfigInterface() => null;

        public abstract void UpdateSelf(EffectParameter parameter, bool selected);
        public virtual void DrawSelf(SpriteBatch spriteBatch, bool selected) { }

        public virtual ParameterInput NewInstance(EffectParameter parameter) => (ParameterInput)Activator.CreateInstance(GetType())!;
    }
}
