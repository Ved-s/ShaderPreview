using ShaderPreview.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderPreview.UI
{
    public abstract class Tab
    {
        public string Name = "";
        public abstract bool Selected { get; set; }
        public object? Tag;
    }
}
