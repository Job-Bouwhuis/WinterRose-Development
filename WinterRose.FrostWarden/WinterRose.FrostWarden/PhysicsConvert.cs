using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FrostWarden
{
    public static class PhysicsConvert
    {
        public static BulletSharp.Math.Matrix ToBullet(this Matrix4x4 m)
        {
            return new BulletSharp.Math.Matrix(
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44
            );
        }

        public static Matrix4x4 ToNumerics(this BulletSharp.Math.Matrix m)
        {
            return new Matrix4x4(
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44
            );
        }

        public static BulletSharp.Math.Vector3 ToBullet(this Vector3 v)
            => new BulletSharp.Math.Vector3(v.X, v.Y, v.Z);

        public static Vector3 ToNumerics(this BulletSharp.Math.Vector3 v)
            => new Vector3(v.X, v.Y, v.Z);
    }
}
