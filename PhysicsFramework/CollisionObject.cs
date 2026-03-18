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
        public Quaternion Rotation = Quaternion.identity;
        
        [Header("Editor View (Read Only / Debug)")]
        public Vector3 DebugSize = Vector3.one;
        public Vector3 DebugPosition;
        public bool IsColliding { get; private set; }
        
        public bool AutoSyncTransform = true; // 是否自动将Transform的变化同步给物理层（适合静态物体或完全由Transform驱动的物体）

        // Cache the shape for GJK
        private BoxShape _shape = new BoxShape();
        public BoxShape Shape => _shape;

        // Callbacks with CollisionInfo
        public event Action<CollisionInfo> OnCollisionEnter;
        public event Action<CollisionInfo> OnCollisionStay;
        public event Action<CollisionInfo> OnCollisionExit;

        private static int _nextId = 0;

        private void Awake()
        {
            ID = _nextId++;
            // 初始化时同步一次
            Position = new FixIntVector3(transform.position);
            Rotation = transform.rotation;
            
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
            PhysicsManager.Instance.RegisterObject(this);
        }

        private void OnEnable()
        {
            PhysicsManager.Instance.RegisterObject(this);
        }

        private void OnDisable()
        {
            PhysicsManager.Instance.UnregisterObject(this);
        }

        public void UpdateBounds()
        {
            // 如果开启了自动同步，将Unity Transform的位置同步给物理层
            // 这对于静态物体或在编辑器中移动物体非常有用
            if (AutoSyncTransform)
            {
                Position = new FixIntVector3(transform.position);
                Rotation = transform.rotation;
                // 也可以同步Scale
                if (transform.localScale != Vector3.one && transform.localScale != DebugSize)
                {
                    Size = new FixIntVector3(transform.localScale);
                    DebugSize = transform.localScale;
                }
            }
            
            // 在编辑器下或者运行初始化时同步数据
            DebugPosition = Position.ToVector3();
            
            // 计算 OBB 在世界坐标下的 AABB 包围盒
            // 因为 AABB 必须能包住旋转后的盒子，所以需要根据旋转计算新的 Extents
            Vector3 floatSize = Size.ToVector3();
            Vector3 right = Rotation * Vector3.right * (floatSize.x * 0.5f);
            Vector3 up = Rotation * Vector3.up * (floatSize.y * 0.5f);
            Vector3 forward = Rotation * Vector3.forward * (floatSize.z * 0.5f);

            // AABB的半边长等于OBB三个轴向在世界坐标下投影的绝对值之和
            Vector3 aabbExtents = new Vector3(
                Mathf.Abs(right.x) + Mathf.Abs(up.x) + Mathf.Abs(forward.x),
                Mathf.Abs(right.y) + Mathf.Abs(up.y) + Mathf.Abs(forward.y),
                Mathf.Abs(right.z) + Mathf.Abs(up.z) + Mathf.Abs(forward.z)
            );

            FixIntVector3 fixExtents = new FixIntVector3(aabbExtents);
            Bounds = new AABB(Position - fixExtents, Position + fixExtents);

            // Update GJK shape
            _shape.UpdateShape(Position.ToVector3(), Rotation, Size.ToVector3());
        }

        private void OnValidate()
        {
            // 当在 Inspector 中修改 DebugSize 时，自动同步给定点数 Size
            Size = new FixIntVector3(DebugSize);
            Position = new FixIntVector3(transform.position);
            Rotation = transform.rotation;
            UpdateBounds();
        }

        public void SyncView()
        {
            transform.position = Position.ToVector3();
            transform.rotation = Rotation;
        }

        public void SetColliding(bool colliding)
        {
            IsColliding = colliding;
        }

        public void TriggerEnter(CollisionInfo info)
        {
            OnCollisionEnter?.Invoke(info);
        }

        public void TriggerStay(CollisionInfo info)
        {
            OnCollisionStay?.Invoke(info);
        }

        public void TriggerExit(CollisionInfo info)
        {
            OnCollisionExit?.Invoke(info);
        }

        private void OnDrawGizmos()
        {
            // 如果未在运行状态，强制同步一下以便在编辑器中实时预览
            if (!Application.isPlaying)
            {
                UpdateBounds();
            }

            Gizmos.color = IsColliding ? Color.red : Color.green;
            
            // 1. 绘制精确的物理 OBB 形状 (紧贴物体的框)
            Gizmos.matrix = Matrix4x4.TRS(Position.ToVector3(), Rotation, Size.ToVector3());
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity; // 恢复矩阵

            // 2. 绘制粗检测阶段的 AABB 包围盒 (浅色/半透明，用于观察 SAP 的包围盒有多大)
            Gizmos.color = IsColliding ? new Color(1, 0, 0, 0.3f) : new Color(0, 1, 0, 0.3f);
            if (Bounds.Max.x != 0)
            {
                Gizmos.DrawWireCube(Bounds.Center.ToVector3(), Bounds.Size.ToVector3());
            }
        }
    }
}
