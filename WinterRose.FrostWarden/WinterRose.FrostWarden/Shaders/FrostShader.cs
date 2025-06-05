using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FrostWarden.Shaders
{
    public class FrostShader
    {
        public Shader ShaderHandle { get; private set; }

        private int uniformTimeLocation = -1;
        private int uniformAcumulatedTimeLocation = -1;
        private int uniformResolutionLocation = -1;
        private int uniformMousePosLocation = -1;

        private bool hasTimeUniform => uniformTimeLocation != -1;
        private bool hasUniformAcumulatedTimeLocation => uniformAcumulatedTimeLocation != -1;
        private bool hasResolutionUniform => uniformResolutionLocation != -1;
        private bool hasMousePosUniform => uniformMousePosLocation != -1;

        public FrostShader(string vertexPath, string fragmentPath)
        {
            ShaderHandle = Raylib.LoadShader(vertexPath, fragmentPath);

            int textureLoc = Raylib.GetShaderLocation(ShaderHandle, "texture0");
            Raylib.SetShaderValue(ShaderHandle, textureLoc, new int[] { 0 }, ShaderUniformDataType.UInt);

            uniformTimeLocation = Raylib.GetShaderLocation(ShaderHandle, "deltaTime");
            uniformAcumulatedTimeLocation = Raylib.GetShaderLocation(ShaderHandle, "time");
            uniformResolutionLocation = Raylib.GetShaderLocation(ShaderHandle, "resolution");
            uniformMousePosLocation = Raylib.GetShaderLocation(ShaderHandle, "mousePos");
        }

        public unsafe bool TrySetValue<T>(string name, T value) where T : unmanaged
        {
            int location = Raylib.GetShaderLocation(ShaderHandle, name);
            if (location == -1)
                return false;

            ShaderUniformDataType type;

            void* rawValue = &value;
            lock(this)
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
                }
                else if (typeof(T) == typeof(int))
                {
                    type = ShaderUniformDataType.Int;
                }
                else if (typeof(T) == typeof(bool))
                {
                    type = ShaderUniformDataType.Int;
                    int b = (bool)(object)value! ? 1 : 0;
                    rawValue = &b;
                }
                else
                {
                    // Unsupported type
                    return false;
                }

                Raylib.SetShaderValue(ShaderHandle, location, rawValue, type);
            }
            
            return true;
        }

        // All parameters are nullable, so you only update the ones you want
        public void Apply(Vector2? resolution = null)
        {
            Raylib.BeginShaderMode(ShaderHandle);

            if (hasTimeUniform)
                Raylib.SetShaderValue(ShaderHandle, uniformTimeLocation, Time.deltaTime, ShaderUniformDataType.Float);

            if (hasResolutionUniform && resolution.HasValue)
                Raylib.SetShaderValue(ShaderHandle, uniformResolutionLocation, resolution.Value, ShaderUniformDataType.Vec2);

            if (hasMousePosUniform)
                Raylib.SetShaderValue(ShaderHandle, uniformMousePosLocation, ray.GetMousePosition(), ShaderUniformDataType.Vec2);

            if (hasUniformAcumulatedTimeLocation)
                Raylib.SetShaderValue(ShaderHandle, uniformAcumulatedTimeLocation, Time.sinceStartup, ShaderUniformDataType.Float);
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
