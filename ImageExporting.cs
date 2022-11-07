using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ShaderPreview.Structures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace ShaderPreview
{
    public static class ImageExporting
    {
        public static Rgba32[] RenderData = null!;

        public static Image<Rgba32>? Image;
        public static float GifDelay = 0;
        public static float GifFps = 40;

        public static void Update()
        {
            int renderDataSize = ShaderPreview.RenderTarget.Width * ShaderPreview.RenderTarget.Height;
            if (RenderData is null || RenderData.Length != renderDataSize)
                RenderData = new Rgba32[renderDataSize];

            if (Interface.Root.GetKeyState(Keys.F7) == KeybindState.JustPressed)
            {
                Image = new(ShaderPreview.RenderTarget.Width, ShaderPreview.RenderTarget.Height);
                ShaderPreview.RenderTarget.GetData(RenderData);
                Image.Frames.AddFrame(RenderData);
                Image.Frames.RemoveFrame(0);
                Image.Mutate(f => f.Crop(new((int)ShaderPreview.TextureScreenRect.X, (int)ShaderPreview.TextureScreenRect.Y, (int)ShaderPreview.TextureScreenRect.Width, (int)ShaderPreview.TextureScreenRect.Height)));
                Util.SelectFileToSave("Save screenshot", (success, file) =>
                {
                    if (success)
                        Image?.SaveAsPng(file);
                    Image?.Dispose();
                    Image = null;
                }, "PNG file|*.png");
            }

            switch (Interface.Root.GetKeyState(Keys.F8))
            {
                case KeybindState.JustPressed:
                    Image = new(ShaderPreview.RenderTarget.Width, ShaderPreview.RenderTarget.Height);
                    
                    Image.Metadata.GetGifMetadata().RepeatCount = 0;
                    GifDelay = 0;
                    break;

                case KeybindState.Pressed:
                    GifDelay += (float)ShaderPreview.GameTime.ElapsedGameTime.TotalMilliseconds;

                    if (GifDelay > 1000 / GifFps)
                    {
                        int delay = (int)(100 / GifFps);
                        GifDelay -= delay * 10;

                        ShaderPreview.RenderTarget.GetData(RenderData);
                        ImageFrame frame = Image!.Frames.AddFrame(RenderData);
                        GifFrameMetadata gifData = frame.Metadata.GetGifMetadata();
                        gifData.FrameDelay = delay;
                        gifData.DisposalMethod = GifDisposalMethod.RestoreToPrevious;
                    }

                    break;

                case KeybindState.JustReleased:
                    Image!.Frames.RemoveFrame(0);
                    Image.Mutate(f => f.Crop(new((int)ShaderPreview.TextureScreenRect.X, (int)ShaderPreview.TextureScreenRect.Y, (int)ShaderPreview.TextureScreenRect.Width, (int)ShaderPreview.TextureScreenRect.Height)));

                    Util.SelectFileToSave("Save recorded gif", (success, file) =>
                    {
                        if (success)
                            Image?.SaveAsGif(file, new() { ColorTableMode = GifColorTableMode.Global });
                        Image?.Dispose();
                        Image = null;
                    }, "GIF file|*.gif");

                    break;
            }
        }
    }
}
