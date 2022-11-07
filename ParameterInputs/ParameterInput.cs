using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace ShaderPreview.ParameterInputs
{
    public abstract class ParameterInput : IState
    {
        internal static JsonObject State
        {
            get
            {
                if (StateData is not null)
                    return StateData;

                JsonObject state = new();

                foreach (var kvp in CurrentActiveParams)
                {
                    JsonObject input = kvp.Value.Save();

                    if (kvp.Value.configInterface is not null)
                    {
                        JsonNode? uistate = kvp.Value.SaveUIState();

                        if (uistate is not null)
                            input["config"] = uistate;
                    }


                    state[kvp.Key] = input;
                }
                return state;
            }

            set => StateData = value;
        }

        public static readonly List<ParameterInput> AllInputs = new();
        public static readonly Dictionary<string, ParameterInput[]> CurrentShaderParams = new();
        public static Dictionary<string, ParameterInput> CurrentActiveParams = new();

        private static JsonObject? StateData;
        private static Dictionary<string, (EffectParameterType type, EffectParameterClass @class, int rows, int cols)> CurrentShaderParamInfos = new();

        public abstract string DisplayName { get; }
        public string ParameterName { get; internal set; } = null!;
        public UIElement? ConfigInterface
        {
            get
            {
                if (configInterface is not null)
                    return configInterface;

                configInterface = GetConfigInterface();
                if (uiState is not null)
                    LoadUIState(uiState);

                return configInterface;
            }
            protected set => configInterface = value;
        }
        private UIElement? configInterface;
        private JsonNode? uiState;

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

            if (StateData is not null)
            {
                foreach (var kvp in CurrentShaderParams)
                {
                    if (StateData[kvp.Key] is not JsonObject obj)
                        continue;

                    ParameterInput? input = IState.Load(obj, name => kvp.Value.FirstOrDefault(i => i.GetType().Name == name));
                    if (input is null)
                        continue;

                    if (obj.TryGet("config", out JsonNode? config))
                        input.uiState = config;

                    CurrentActiveParams[kvp.Key] = input;
                }
                StateData = null;
            }
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

        internal static void Register(Type type)
        {
            if (type.IsAssignableTo(typeof(ParameterInput)))
                AllInputs.Add((ParameterInput)Activator.CreateInstance(type)!);
        }

        public static void ResetUI()
        {
            foreach (ParameterInput[] arr in CurrentShaderParams.Values)
                foreach (ParameterInput param in arr)
                    param.ConfigInterface = null;
        }

        public abstract bool AppliesToParameter(EffectParameter parameter);
        protected virtual UIElement? GetConfigInterface() => null;

        public abstract void UpdateSelf(EffectParameter parameter, bool selected);
        public virtual void DrawSelf(SpriteBatch spriteBatch, bool selected) { }

        public virtual ParameterInput NewInstance(EffectParameter parameter) => (ParameterInput)Activator.CreateInstance(GetType())!;

        public virtual JsonNode? SaveState() { return null!; }
        public virtual void LoadState(JsonNode node) { }

        public virtual JsonNode? SaveUIState()
        {
            if (configInterface is not IState uistate)
                return null;
            return uistate.SaveState();
        }
        public virtual void LoadUIState(JsonNode node)
        {
            if (configInterface is not IState uistate)
                return;

            uistate.LoadState(node);
        }
    }
}
