using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WinterRose.FrostWarden.Physics;

namespace WinterRose.FrostWarden.Components
{
    public class Transform : Component
    {
        [IncludeWithSerialization]
        public Vector3 position
        {
            get => _position;
            set 
            {
                _position = value;
                MarkDirty(true);

                var phcs = owner.GetAllComponents<PhysicsComponent>();
                foreach (var p in phcs)
                    p.Sync();
            }
        }

        [IncludeWithSerialization]
        public Vector3 rotation
        {
            get => _rotation;
            set { _rotation = value; MarkDirty(true); }
        }

        [IncludeWithSerialization]
        public Vector3 scale
        {
            get => _scale;
            set { _scale = value; MarkDirty(true); }
        }

        [IncludeWithSerialization]
        public Transform? parent => _parent;

        public IReadOnlyList<Transform> Children => children;

        public Matrix4x4 localMatrix
        {
            get
            {
                if (_localDirty)
                {
                    RecalculateLocalMatrix();
                    _localDirty = false;
                }
                return _localMatrix;
            }
        }

        public Matrix4x4 worldMatrix
        {
            get
            {
                if (_worldDirty)
                {
                    _worldMatrix = parent != null
                        ? localMatrix * parent.worldMatrix
                        : localMatrix;

                    _worldDirty = false;
                }

                return _worldMatrix;
            }
        }

        public void SetParent(Transform? newParent)
        {
            if (_parent == newParent)
                return;

            _parent?.children.Remove(this);
            newParent?.children.Add(this);
            _parent = newParent;
            MarkDirty(recursive: true);
        }

        private Vector3 _position = Vector3.Zero;
        private Vector3 _rotation = Vector3.Zero;
        private Vector3 _scale = Vector3.One;

        private Matrix4x4 _localMatrix = Matrix4x4.Identity;
        private Matrix4x4 _worldMatrix = Matrix4x4.Identity;

        private bool _localDirty = true;
        private bool _worldDirty = true;

        private Transform? _parent;

        [IncludeWithSerialization]
        private List<Transform> children = new();

        private void MarkDirty(bool recursive = false)
        {
            _localDirty = true;
            _worldDirty = true;

            if (recursive)
            {
                foreach (var child in children)
                    child.MarkDirty(true);
            }
        }

        private void RecalculateLocalMatrix()
        {
            var translation = Matrix4x4.CreateTranslation(_position);
            var rotationX = Matrix4x4.CreateRotationX(_rotation.X);
            var rotationY = Matrix4x4.CreateRotationY(_rotation.Y);
            var rotationZ = Matrix4x4.CreateRotationZ(_rotation.Z);
            var rotation = rotationZ * rotationY * rotationX;
            var scale = Matrix4x4.CreateScale(_scale);

            _localMatrix = scale * rotation * translation;
        }

        public void Translate(BulletSharp.Math.Matrix trans)
        {
            // Extract position from translation elements (last row)
            _position = new Vector3(trans.M41, trans.M42, trans.M43);

            // Build a System.Numerics.Matrix4x4 rotation matrix from the bullet matrix rotation part
            var rotationMatrix = new Matrix4x4(
                (float)trans.M11, (float)trans.M12, (float)trans.M13, 0f,
                (float)trans.M21, (float)trans.M22, (float)trans.M23, 0f,
                (float)trans.M31, (float)trans.M32, (float)trans.M33, 0f,
                0f, 0f, 0f, 1f);

            // Decompose to get quaternion (ignore scale and translation)
            if (Matrix4x4.Decompose(rotationMatrix, out Vector3 scale, out Quaternion rotQuat, out Vector3 translation))
            {
                _rotation = QuaternionToEuler(rotQuat);
            }

            MarkDirty(true);

            var phcs = owner.GetAllComponents<PhysicsComponent>();
            foreach (var p in phcs)
                if(p is not RigidBodyComponent)
                    p.Sync();
        }

        // Quaternion to Euler (XYZ order) helper same as before
        private Vector3 QuaternionToEuler(Quaternion q)
        {
            float sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            float cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            float roll = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            float sinp = 2 * (q.W * q.Y - q.Z * q.X);
            float pitch;
            if (Math.Abs(sinp) >= 1)
                pitch = (float)(Math.CopySign(Math.PI / 2, sinp));
            else
                pitch = (float)Math.Asin(sinp);

            float siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            float yaw = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return new Vector3(roll, pitch, yaw);
        }
    }

}
