using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ShaderPreview
{
    public interface IState
    {
        static List<Type> StateTypes = new();

        internal static void Register(Type type)
        {
            if (!type.IsAssignableTo(typeof(IState)))
                return;

            StateTypes.Add(type);
        }

        public static JsonObject Save(IState state)
        {
            JsonObject obj = new();
            obj["name"] = state.GetType().Name;
            obj["data"] = state.SaveState();
            return obj;
        }

        public static T? Load<T>(JsonObject json, Func<string, T?>? instancePicker = null) where T : IState
        {
            if (!json.TryGet("name", out string? name))
                return default;

            T? instance;
            if (instancePicker is null)
            {
                Type? type = StateTypes.FirstOrDefault(t => t.Name == name && t.IsAssignableTo(typeof(T)));
                if (type is null)
                    return default;

                instance = (T)Activator.CreateInstance(type)!;
            }
            else
            {
                instance = instancePicker(name);
            }
            if (instance is not null && json.TryGet("data", out JsonNode? data))
                instance.LoadState(data);

            return instance;
        }

        public JsonNode SaveState();
        public void LoadState(JsonNode node);
    }
}
