using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ShaderPreview.UI.Helpers
{
    public static class Content
    {
        public static Effect Gradient = null!;
        public static Effect Ellipse = null!;

        internal static void Load(ContentManager content)
        {
            Gradient = content.Load<Effect>("Gradient");
            Ellipse = content.Load<Effect>("Ellipse");
        }
    }
}
