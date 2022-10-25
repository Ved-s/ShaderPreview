extern alias mgfxc;

using mgfxc::MonoGame.Effect;
using Microsoft.Xna.Framework.Graphics;
using ShaderPreview.ParameterInputs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShaderPreview
{
    public static class ShaderCompiler
    {
        const string BasicShader = "sampler texSampler : register(S0);\n\nfloat4 MainPS(float2 pos : TEXCOORD0) : COLOR0\n{\n\treturn tex2D(texSampler, pos);\n}\n\ntechnique Main\n{\n\tpass Main\n\t{\n\t\tPixelShader = compile ps_3_0 MainPS();\n\t}\n}";
        
        static FileSystemWatcher? FileWatcher;

        public static string ShaderPath { get; private set; } = "";
        public static Effect? Shader { get; private set; }
        public static string Errors { get; private set; } = "";

        static Effect? CompiledShader;
        static object Lock = new();

        static Type EffectObjectType = typeof(ShaderResult).Assembly.GetType("MonoGame.Effect.EffectObject")!;

        delegate object CompileEffectDelegate(ShaderResult shaderResult, out string errorsAndWarnings);
        static CompileEffectDelegate CompileEffect =
            EffectObjectType.GetMethod("CompileEffect", BindingFlags.Public | BindingFlags.Static)!
            .CreateDelegate<CompileEffectDelegate>();

        static MethodInfo WriteEffect = EffectObjectType.GetMethod("Write", BindingFlags.Public | BindingFlags.Instance)!;

        static Regex ErrorRegex = new(@"^.+\((\d+(?:-\d+)?),(\d+(?:-\d+)?)\): (.+)", RegexOptions.Compiled);
        static bool WaitingForAccess = false;

        public static void SetShaderFilePath(string path)
        {
            if (UI.Pages.Settings.ShaderNameLabel is not null)
                UI.Pages.Settings.ShaderNameLabel.Text = "Selected: " + Path.GetFileName(path);

            ShaderPath = path;
            if (!File.Exists(ShaderPath))
                File.WriteAllText(ShaderPath, BasicShader);
            CompileShader();

            if (FileWatcher is not null)
            {
                FileWatcher.EnableRaisingEvents = false;
                FileWatcher.Dispose();
            }

            FileWatcher = new FileSystemWatcher(Path.GetDirectoryName(Path.GetFullPath(ShaderPath))!, "*.fx");
            FileWatcher.Changed += async (s, e) => 
            {
                if (WaitingForAccess)
                    return;

                WaitingForAccess = true;
                await Task.Delay(200);
                CompileShader();
                WaitingForAccess = false;
            };

            FileWatcher.EnableRaisingEvents = true;
        }

        private static void CompileShader()
        {
            try
            {
                Errors = "Compiling...";
                string shader = File.ReadAllText(ShaderPath);

                Options options = new()
                {
                    Profile = ShaderProfile.OpenGL,
                };

                var shaderResult = ShaderResult.FromFile(ShaderPath, options, new ConsoleCompilerOutput());
                object effect;
                string? errorsWarns = null;
                try
                {
                    effect = CompileEffect(shaderResult, out errorsWarns);
                }
                catch (Exception e)
                {
                    if (errorsWarns?.Length is null or 0)
                    {
                        Errors = e.Message;
                        return;
                    }

                    StringBuilder sb = new();
                    foreach (Match match in ErrorRegex.Matches(errorsWarns))
                    {
                        if (sb.Length > 0)
                            sb.AppendLine();

                        sb.AppendLine($"Line {match.Groups[1].Value}, column {match.Groups[2].Value}:");
                        sb.Append("    ");
                        sb.AppendLine(match.Groups[3].Value);
                    }
                    Errors = sb.ToString();
                    return;
                }
                Errors = "Compilation success";

                MemoryStream stream = new();
                BinaryWriter bw = new(stream);
                WriteEffect.Invoke(effect, new object[] { bw, options });

                stream.Position = 0;
                byte[] code = stream.ToArray();

                stream.Dispose();
                lock (Lock)
                    CompiledShader = new(ShaderPreview.Instance.GraphicsDevice, code);
            }
            catch (Exception e)
            {
                Errors = $"{e.GetType().Name}: {e.Message}";
            }
        }

        internal static void EndCompiling()
        {
            lock (Lock)
            {
                if (CompiledShader is not null)
                {
                    Shader?.Dispose();
                    Shader = CompiledShader;
                    CompiledShader = null;
                    ParameterInput.ShaderChanged();
                    UI.Pages.ShaderParameters.ShaderChanged();
                }
            }
        }

        class ConsoleCompilerOutput : IEffectCompilerOutput
        {
            public void WriteError(string file, int line, int column, string message)
            {
                Console.WriteLine($"[Error] {file}:{line}:{column}\n\t{message}");
            }

            public void WriteWarning(string file, int line, int column, string message)
            {
                Console.WriteLine($"[Warn] {file}:{line}:{column}\n\t{message}");
            }
        }
    }
}
