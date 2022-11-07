using ShaderPreview.ParameterInputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderPreview
{
    internal static class Autoloading
    {
        public static void Autoload()
        {
            foreach (Type type in typeof(ParameterInput).Assembly.GetExportedTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                if (type.IsAssignableTo(typeof(ParameterInput)))
                    ParameterInput.Register(type);

                if (type.IsAssignableTo(typeof(IState)))
                    IState.Register(type);
            }
        }
    }
}
