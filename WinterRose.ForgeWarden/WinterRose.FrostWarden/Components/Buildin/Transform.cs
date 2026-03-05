using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.Physics;

namespace WinterRose.ForgeWarden
{
    public class Transform : Component
    {
        [WFInclude]
        public Vector3 position
        {
            get => _position; // local
            set
            {
                _position = value;
                MarkDirty(true);
                SyncPhysics();
            }
        }

        [WFInclude]
        public Quaternion rotation
        {
            get => _rotation; // local
            set
            {
                _rotation = value;
                MarkDirty(true);
                SyncPhysics();
            }
        }

        [WFInclude]
        public Vector3 scale
        {
            get => _scale; // local
            set
            {
                _scale = value;
                MarkDirty(true);
                SyncPhysics();
            }
        }

        public Vector3 worldPosition
        {
            get => Vector3.Transform(_position, worldMatrix);
            set
            {
                if (parent != null)
                {
                    Matrix4x4.Invert(parent.worldMatrix, out var inv);
                    _position = Vector3.Transform(value, inv);
                }
                else _position = value;

                MarkDirty(true);
                SyncPhysics();
            }
        }

        public Quaternion worldRotation
        {
            get => parent != null ? _rotation * parent.worldRotation : _rotation;
            set
            {
                if (parent != null)
                {
                    _rotation = value * Quaternion.Inverse(parent.worldRotation);
                }
                else _rotation = value;
                MarkDirty(true);
                SyncPhysics();
            }
        }

        public Vector3 worldScale
        {
            get => parent != null
                ? new Vector3(_scale.X * parent.worldScale.X, _scale.Y * parent.worldScale.Y, _scale.Z * parent.worldScale.Z)
                : _scale;
            set
            {
                if (parent != null)
                {
                    _scale = new Vector3(
                        value.X / parent.worldScale.X,
                        value.Y / parent.worldScale.Y,
                        value.Z / parent.worldScale.Z
                    );
                }
                else _scale = value;

                MarkDirty(true);
                SyncPhysics();
            }
        }

        [Hide]
        public ref Vector3 positionRef
        {
            get
            {
                UpdateIfDirty();
                return ref _position;
            }
        }
        [Hide]
        public ref Quaternion rotationRef
        {
            get
            {
                UpdateIfDirty();
                return ref _rotation;
            }
        }
        [Hide]
        public ref Vector3 scaleRef
        {
            get
            {
                UpdateIfDirty();
                return ref _scale;
            }
        }
        [Hide]
        public Vector3 rotationEulerDegrees
        {
            get
            {
                Vector3 eulerRadians = QuaternionToEuler(rotation);
                return eulerRadians * (180f / MathF.PI);
            }
            set
            {
                Vector3 eulerRadians = value * (MathF.PI / 180f);
                _rotation = EulerToQuaternion(eulerRadians);
                MarkDirty(true);
            }
        }
        [WFInclude]
        public Transform? parent
        {
            get => _parent; set => SetParent(value);
        }

        [Hide]
        public IReadOnlyList<Transform> Children => children;

        [Hide]
        public Matrix4x4 localMatrix
        {
            get
            {
                if (_dirty)
                {
                    RecalculateLocalMatrix();
                    _dirty = false;
                }
                return field;
            }
            private set;
        }

        [Hide]
        public Matrix4x4 worldMatrix
        {
            get
            {
                if (_dirty)
                {
                    RecalculateLocalMatrix();
                    field = parent != null
                        ? localMatrix * parent.worldMatrix
                        : localMatrix;

                    field.M44 = 1f; // ensure proper homogeneous coordinate
                    _dirty = false;
                }
                return field;
            }
        }

        [Hide]
        public Vector3 right => new Vector3(worldMatrix.M11, worldMatrix.M21, worldMatrix.M31);

        [Hide]
        public Vector3 left => -right;

        [Hide]
        public Vector3 up => new Vector3(worldMatrix.M12, worldMatrix.M22, worldMatrix.M32);

        [Hide]
        public Vector3 down => -up;

        [Hide]
        public Vector3 forward => new Vector3(worldMatrix.M13, worldMatrix.M23, worldMatrix.M33);

        [Hide]
        public Vector3 back => -forward;

        private void UpdateIfDirty() => _ = worldMatrix;

        public void SetParent(Transform? newParent)
        {
            if (_parent == newParent)
                return;

            _parent?.children.Remove(this);
            newParent?.children.Add(this);
            _parent = newParent;
            MarkDirty(recursive: true);
        }

        private void SyncPhysics()
        {
            var phcs = owner.GetAllComponents<PhysicsComponent>();
            foreach (var p in phcs)
                if (p is not RigidBodyComponent)
                    p.Sync();
        }

        [Hide]
        private Vector3 _position = Vector3.Zero;
        [Hide]
        private Quaternion _rotation = Quaternion.Identity;
        [Hide]
        private Vector3 _scale = Vector3.One;

        [Hide]
        private bool _dirty = true;

        [Hide]
        private Transform? _parent;

        [WFInclude]
        private List<Transform> children = new();

        private void MarkDirty(bool recursive = false)
        {
            _dirty = true;
            UpdateIfDirty();

            if (recursive)
            {
                foreach (var child in children)
                    child.MarkDirty(true);
            }
        }

        private void RecalculateLocalMatrix() => localMatrix = Matrix4x4.CreateScale(_scale)
             * Matrix4x4.CreateFromQuaternion(_rotation)
             * Matrix4x4.CreateTranslation(_position);

        public void Translate(BulletSharp.Math.Matrix trans)
        {
            _position = new Vector3(trans.M41, trans.M42, trans.M43);

            var bulletQuat = new BulletSharp.Math.Quaternion(trans.M11, trans.M12, trans.M13, trans.M14);
            // Bullet quaternion is (X,Y,Z,W) - convert to System.Numerics.Quaternion (X,Y,Z,W)
            _rotation = new Quaternion((float)bulletQuat.X, (float)bulletQuat.Y, (float)bulletQuat.Z, (float)bulletQuat.W);

            MarkDirty(true);

            var phcs = owner.GetAllComponents<PhysicsComponent>();
            foreach (var p in phcs)
                if (p is not RigidBodyComponent)
                    p.Sync();
        }

        private Vector3 QuaternionToEuler(Quaternion q)
        {
            // roll (x-axis rotation)
            float sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            float cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            float roll = MathF.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            float sinp = 2 * (q.W * q.Y - q.Z * q.X);
            float pitch;
            if (MathF.Abs(sinp) >= 1)
                pitch = MathF.CopySign(MathF.PI / 2, sinp);
            else
                pitch = MathF.Asin(sinp);

            // yaw (z-axis rotation)
            float siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            float yaw = MathF.Atan2(siny_cosp, cosy_cosp);

            return new Vector3(roll, pitch, yaw);
        }

        private Quaternion EulerToQuaternion(Vector3 euler)
        {
            float cy = MathF.Cos(euler.Z * 0.5f);
            float sy = MathF.Sin(euler.Z * 0.5f);
            float cp = MathF.Cos(euler.Y * 0.5f);
            float sp = MathF.Sin(euler.Y * 0.5f);
            float cr = MathF.Cos(euler.X * 0.5f);
            float sr = MathF.Sin(euler.X * 0.5f);

            Quaternion q = new Quaternion();
            q.W = cr * cp * cy + sr * sp * sy;
            q.X = sr * cp * cy - cr * sp * sy;
            q.Y = cr * sp * cy + sr * cp * sy;
            q.Z = cr * cp * sy - sr * sp * cy;
            return q;
        }
        private static Quaternion CreateLookRotation(Vector3 forward, Vector3 up)
        {
            forward = Vector3.Normalize(forward);
            up = Vector3.Normalize(up);

            Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
            up = Vector3.Cross(forward, right);

            Matrix4x4 m = new Matrix4x4(
                right.X, right.Y, right.Z, 0,
                up.X, up.Y, up.Z, 0,
                forward.X, forward.Y, forward.Z, 0,
                0, 0, 0, 1);

            Quaternion q = Quaternion.CreateFromRotationMatrix(m);
            return q;
        }

        public void LookAt(Transform target)
        {
            LookAt(target.position);
        }

        public void LookAt(Vector3 pos)
        {

            Vector3 direction = Vector3.Normalize(pos - position);

            // Handle degenerate case where target is at same position
            if (direction == Vector3.Zero)
                return;

            // Define what 'up' means for your world (usually Y+)
            Vector3 up = Vector3.UnitY;

            // Build rotation from forward direction
            Quaternion lookRotation = CreateLookRotation(direction, up);
            rotation = lookRotation;
            MarkDirty(true);
        }

    }
}
