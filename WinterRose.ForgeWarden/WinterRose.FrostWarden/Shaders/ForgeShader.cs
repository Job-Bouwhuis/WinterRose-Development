using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.Worlds;

namespace WinterRose.ForgeWarden.Shaders
{
    public class MaterialShader : ForgeShader
    {
        public MaterialShader(ref Material mat, string vertexPath, string fragmentPath) : base(vertexPath, fragmentPath)
        {
            Material = mat;
            Material.Shader = ShaderHandle;
            ShaderHandle = mat.Shader;
        }

        public Material Material;

        public override void Apply(Vector2? resolution = null)
        {
            ray.BeginShaderMode(Material.Shader);
            SetDefaults();
        }
    }


    public class ForgeShader
    {
        public Shader ShaderHandle { get; protected set; }

        public string? VertexPath { get; }
        public string? FragmentPath { get; }

        public ForgeShader(string vertexPath, string fragmentPath) : this(Raylib.LoadShader(vertexPath, fragmentPath))
        {
            VertexPath = vertexPath;
            FragmentPath = fragmentPath;
        }

        public ForgeShader(Shader handle)
        {
            ShaderHandle = handle;

            VertexPath = null;
            FragmentPath = null;
        }

        public unsafe bool TrySetValue<T>(string name, T value) where T : unmanaged
        {
            int location = Raylib.GetShaderLocation(ShaderHandle, name);
            if (location == -1)
                return false;

            ShaderUniformDataType type;

            void* rawValue = &value;
            lock (this)
            {
                if (typeof(T) == typeof(float))
                {
                    type = ShaderUniformDataType.Float;
                }
                else if (typeof(T) == typeof(Vector2))
                {
                    type = ShaderUniformDataType.Vec2;
                }
                else if (typeof(T) == typeof(Vector3))
                {
                    type = ShaderUniformDataType.Vec3;
                }
                else if (typeof(T) == typeof(Vector4))
                {
                    type = ShaderUniformDataType.Vec4;
                }
                else if (typeof(T) == typeof(Color))
                {
                    type = ShaderUniformDataType.Vec4;
                    Vector4 c = Raylib.ColorNormalize((Color)(object)value!);
                    rawValue = &c;
                    SetValue(location, rawValue, type);
                    return true;
                }
                else if (typeof(T) == typeof(int))
                {
                    type = ShaderUniformDataType.Int;
                }
                else if (typeof(T) == typeof(bool))
                {
                    type = ShaderUniformDataType.Int;
                    int b = (bool)(object)value! ? 1 : 0;
                    SetValue(location, &b, type);
                    return true;
                }
                else
                {
                    // Unsupported type
                    return false;
                }

                SetValue(location, rawValue, type);
            }

            return true;
        }

        protected virtual unsafe void SetValue(int location, void* rawValue, ShaderUniformDataType type)
        {
            Raylib.SetShaderValue(ShaderHandle, location, rawValue, type);
        }

        public void SetDefaults(Vector2? resolution = null)
        {
            TrySetValue("deltaTime", Time.deltaTime);

            if (resolution.HasValue)
                TrySetValue("resolution", resolution.Value);

            TrySetValue("mousePos", Universe.Input.MousePosition);

            TrySetValue("time", Time.sinceStartup);

            TrySetValue("lightDir", Universe.SunDirection);
        }

        public virtual void Apply(Vector2? resolution = null)
        {
            Raylib.BeginShaderMode(ShaderHandle);
            SetDefaults(resolution);
        }

        public void End()
        {
            Raylib.EndShaderMode();
        }

        public void Unload()
        {
            Raylib.UnloadShader(ShaderHandle);
        }
    }

}
