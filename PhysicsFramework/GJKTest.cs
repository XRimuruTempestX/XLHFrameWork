using UnityEngine;
using XLHFrameWork.PhysicsFramework.GJKPhysics.Runtime;
using XLHFrameWork.PhysicsFramework.GJKPhysics.Shape;

namespace XLHFrameWork.PhysicsFramework
{
    public class GJKTest : MonoBehaviour
    {
        [Header("Transforms (Assign two game objects)")]
        public Transform ObjectA;
        public Transform ObjectB;

        [Header("Box Sizes")]
        public Vector3 SizeA = Vector3.one;
        public Vector3 SizeB = Vector3.one;

        [Header("Result (Read Only)")]
        public bool IsColliding = false;

        private BoxShape _shapeA = new BoxShape();
        private BoxShape _shapeB = new BoxShape();
        private GJK _gjk = new GJK();

        private void Update()
        {
            if (ObjectA == null || ObjectB == null) return;

            // 1. 更新形状数据 (根据 Transform 和 Size)
            _shapeA.UpdateShape(ObjectA.position, ObjectA.rotation, SizeA);
            _shapeB.UpdateShape(ObjectB.position, ObjectB.rotation, SizeB);

            // 2. 调用 GJK 进行窄阶段精确相交测试
            IsColliding = _gjk.Intersect(_shapeA, _shapeB);
        }

        private void OnDrawGizmos()
        {
            if (ObjectA == null || ObjectB == null) return;

            // 碰撞时画红色，未碰撞时画绿色
            Gizmos.color = IsColliding ? Color.red : Color.green;

            // 绘制 Object A 的定向包围盒 (OBB)
            Gizmos.matrix = Matrix4x4.TRS(ObjectA.position, ObjectA.rotation, SizeA);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

            // 绘制 Object B 的定向包围盒 (OBB)
            Gizmos.matrix = Matrix4x4.TRS(ObjectB.position, ObjectB.rotation, SizeB);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
}
