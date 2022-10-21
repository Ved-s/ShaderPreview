using ShaderPreview.Structures;
using ShaderPreview.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderPreview.UI.Helpers
{
    public interface ILayoutContainer
    {
        public void LayoutChild(UIElement child, ref Rect screenRect);
    }
}
