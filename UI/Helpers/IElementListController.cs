using ShaderPreview.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShaderPreview.UI.Helpers
{
    public interface IElementListController
    {
        public bool CanSetElement(UIElement @new, UIElement old, int index) => true;
        public bool CanAddElement(UIElement value) => true;
        public bool CanRemoveElement(UIElement value) => true;
        public bool CanInsertElement(UIElement value, int index) => true;

        public bool CanModifyElements() => true;
    }
}
