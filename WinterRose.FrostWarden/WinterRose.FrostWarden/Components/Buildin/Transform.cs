using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FrostWarden.Components
{
    public class Transform : Component
    {
        [IncludeWithSerialization]
        public Vector3 position
        {
            get => _position;
            set { _position = value; MarkDirty(true); }
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
    }

}
