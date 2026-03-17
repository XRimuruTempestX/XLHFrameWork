using System;
using FixIntMath;
using UnityEngine;
using XLHFrameWork.PhysicsFramework.GJKPhysics.Shape;

namespace XLHFrameWork.PhysicsFramework
{
    public class CollisionObject : MonoBehaviour
    {
        public int ID { get; private set; }
        public AABB Bounds;
        public FixIntVector3 Size = FixIntVector3.one;
        public FixIntVector3 Position;
        
        [Header("Editor View (Read Only / Debug)")]
        public Vector3 DebugSize = Vector3.one;
        public Vector3 DebugPosition;
        public bool IsColliding { get; private set; }
        
        public bool AutoSyncTransform = true; // 是否自动将Transform的变化同步给物理层（适合静态物体或完全由Transform驱动的物体）

        // Cache the shape for GJK
        private BoxShape _shape = new BoxShape();
        public BoxShape Shape => _shape;

        public event Action<CollisionObject> OnCollisionEnter;
        public event Action<CollisionObject> OnCollisionExit;

        private static int _nextId = 0;

        private void Awake()
        {
            ID = _nextId++;
            // 初始化时同步一次
            Position = new FixIntVector3(transform.position);
            
            // 尝试从 Transform 的 scale 或者 DebugSize 初始化 FixInt 大小
            if (transform.localScale != Vector3.one)
            {
                Size = new FixIntVector3(transform.localScale);
                DebugSize = transform.localScale;
            }
            else
            {
                Size = new FixIntVector3(DebugSize);
            }

            UpdateBounds();
        }

        private void Start()
        {
            // 双重保险：如果OnEnable时PhysicsManager还没准备好，Start再试一次
            if (PhysicsManager.Instance != null)
            {
                PhysicsManager.Instance.RegisterObject(this);
            }
        }

        private void OnEnable()
        {
            PhysicsManager.Instance?.RegisterObject(this);
        }

        private void OnDisable()
        {
            PhysicsManager.Instance?.UnregisterObject(this);
        }

        public void UpdateBounds()
        {
            // 如果开启了自动同步，将Unity Transform的位置同步给物理层
            // 这对于静态物体或在编辑器中移动物体非常有用
            if (AutoSyncTransform)
            {
                Position = new FixIntVector3(transform.position);
                // 也可以同步Scale
                if (transform.localScale != Vector3.one && transform.localScale != DebugSize)
                {
                    Size = new FixIntVector3(transform.localScale);
                    DebugSize = transform.localScale;
                }
            }
            
            // 在编辑器下或者运行初始化时同步数据
            DebugPosition = Position.ToVector3();
            
            FixIntVector3 halfSize = Size / 2;
            Bounds = new AABB(Position - halfSize, Position + halfSize);

            // Update GJK shape
            _shape.UpdateVertices(Position.ToVector3(), transform.rotation, Size.ToVector3());
        }

        private void OnValidate()
        {
            // 当在 Inspector 中修改 DebugSize 时，自动同步给定点数 Size
            Size = new FixIntVector3(DebugSize);
            Position = new FixIntVector3(transform.position);
            UpdateBounds();
        }

        public void SyncView()
        {
            transform.position = Position.ToVector3();
        }

        public void SetColliding(bool colliding)
        {
            IsColliding = colliding;
        }

        public void TriggerEnter(CollisionObject other)
        {
            OnCollisionEnter?.Invoke(other);
        }

        public void TriggerExit(CollisionObject other)
        {
            OnCollisionExit?.Invoke(other);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = IsColliding ? Color.red : Color.green;
            // Convert FixInt to float for Gizmos
            Gizmos.DrawWireCube(Bounds.Center.ToVector3(), Bounds.Size.ToVector3());
        }
    }
}
