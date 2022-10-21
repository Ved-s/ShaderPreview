extern alias mgfxc;

using mgfxc::MonoGame.Effect;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ShaderPreview.ParameterInputs;
using ShaderPreview.Structures;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ShaderPreview
{
    public class ShaderPreview : Game
    {
        public GraphicsDeviceManager Graphics;
        public static SpriteBatch SpriteBatch = null!;

        public static Texture2D Pixel = null!;
        public static SpriteFont Consolas10 = null!;

        public static ShaderPreview Instance = null!;

        public static Rect TextureScreenRect;
        public static GameTime GameTime = new();

        public ShaderPreview()
        {
            Instance = this;
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += (s, e) =>
            {
                Interface.SizeChanged();
            };
            ParameterInput.Autoload();
            ShaderCompiler.SetShaderFilePath("Shader.fx");
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Pixel = new(GraphicsDevice, 1, 1);
            Pixel.SetData(new Color[] { Color.White });
            Consolas10 = Content.Load<SpriteFont>("Consolas10");
            
            UI.Helpers.Content.Load(Content);
            Interface.Init();
            Interface.Root.Font = Consolas10;
        }

        protected override void Update(GameTime gameTime)
        {
            GameTime = gameTime;
            Viewport vp = GraphicsDevice.Viewport;
            Vec2 screenSize = new(vp.Width, vp.Height);
            screenSize.X -= (vp.Width - Interface.SidePanel?.ScreenRect.Left) ?? 0;

            Vec2 size = new(MathF.Min(screenSize.X, screenSize.Y) * .8f);
            TextureScreenRect = new Rect(((screenSize - size) / 2).Round(), size.Round());

            Interface.Update();
            ParameterInput.Update();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(.1f, .1f, .1f));
            GraphicsDevice.ScissorRectangle = new(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            SpriteBatch.Begin(SpriteSortMode.Immediate);

            SpriteBatch.DrawRectangle(TextureScreenRect - new Offset4(1), Color.White * .3f);

            if (ShaderCompiler.Shader is not null)
            {
                ShaderCompiler.Shader.CurrentTechnique.Passes[0].Apply();
            }

            SpriteBatch.Draw(Pixel, TextureScreenRect, Color.White);

            SpriteBatch.End();
            SpriteBatch.Begin();
            ParameterInput.Draw(SpriteBatch);
            Interface.Draw(SpriteBatch);
            
            SpriteBatch.DrawStringShaded(Consolas10, ShaderCompiler.Errors, new(10), Color.White, Color.Black);

            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}