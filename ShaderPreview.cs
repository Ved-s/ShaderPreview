extern alias mgfxc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.ParameterInputs;
using ShaderPreview.Structures;
using System;
using System.IO;
using System.Text.Json.Nodes;

namespace ShaderPreview
{
    public class ShaderPreview : Game
    {
        public GraphicsDeviceManager Graphics;
        public static SpriteBatch SpriteBatch = null!;

        public static Texture2D Pixel = null!;
        public static SpriteFont Consolas10 = null!;

        public static Texture2D? BaseTexture;
        public static string? BaseTexturePath;

        public static ShaderPreview Instance = null!;

        public static Rect TextureScreenRect;
        public static Rect TextureMaxScreenRect;
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
            Autoloading.Autoload();

            if (File.Exists("state.json"))
            {
                try
                {
                    JsonNode? node = JsonNode.Parse(File.ReadAllText("state.json"));
                    if (node is JsonObject state)
                        LoadState(state);
                }
                catch { }
            }

            if (ShaderCompiler.ShaderPath.IsNullEmptyOrWhitespace())
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

            if (BaseTexturePath is not null)
                BaseTexture = Texture2D.FromFile(GraphicsDevice, BaseTexturePath);
        }

        protected override void Update(GameTime gameTime)
        {
            GameTime = gameTime;
            Viewport vp = GraphicsDevice.Viewport;
            Vec2 screenSize = new(vp.Width, vp.Height);
            screenSize.X -= (vp.Width - Interface.SidePanel?.ScreenRect.Left) ?? 0;

            TextureMaxScreenRect = new(Vec2.Zero, screenSize);

            Vec2 size = screenSize;
            size *= .8f;

            Vec2 textureSize = BaseTexture is null ? new(100) : new(BaseTexture.Width, BaseTexture.Height);

            float scale = Math.Min(size.X / textureSize.X, size.Y / textureSize.Y);
            size = textureSize * scale;

            TextureScreenRect = new Rect(((screenSize - size) / 2).Rounded(), size.Rounded());

            ShaderCompiler.EndCompiling();
            Interface.Update();
            ParameterInput.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(.1f, .1f, .1f));
            GraphicsDevice.ScissorRectangle = new(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            // TODO: make this configurable from UI
            SpriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp, rasterizerState: RasterizerState.CullNone);

            SpriteBatch.DrawRectangle(TextureScreenRect - new Offset4(1), Color.White * .3f);

            if (ShaderCompiler.Shader is not null)
            {
                ShaderCompiler.Shader.CurrentTechnique.Passes[0].Apply();
            }

            SpriteBatch.Draw(BaseTexture ?? Pixel, TextureScreenRect, Color.White);

            SpriteBatch.End();
            SpriteBatch.Begin();
            ParameterInput.Draw(SpriteBatch);
            Interface.Draw(SpriteBatch);

            SpriteBatch.DrawStringShaded(Consolas10, ShaderCompiler.Errors, new(10), Color.White, Color.Black);

            SpriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void EndRun()
        {
            JsonObject state = new();
            SaveState(state);
            File.WriteAllText("state.json", state.ToJsonString(new() { WriteIndented = true }));
        }

        private void SaveState(JsonObject state)
        {
            state["shader"] = ShaderCompiler.ShaderPath;
            state["texture"] = BaseTexturePath;
            state["inputs"] = ParameterInput.State;
        }

        private void LoadState(JsonObject state)
        {
            if (state.TryGet("inputs", out JsonObject? inputs))
                ParameterInput.State = inputs;
            if (state.TryGet("shader", out string? shader))
                ShaderCompiler.SetShaderFilePath(shader);
            if (state.TryGet("texture", out string? texture))
                BaseTexturePath = texture;
        }
    }
}