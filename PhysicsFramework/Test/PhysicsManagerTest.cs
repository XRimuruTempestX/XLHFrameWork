using UnityEngine;
using FixIntMath;

namespace XLHFrameWork.PhysicsFramework.Test
{
    public class PhysicsManagerTest : MonoBehaviour
    {
        [Header("Settings")]
        public bool EnableCorrection = true; // 是否开启碰撞分离
        [Range(0f, 1f)]
        public float CorrectionPercent = 0.5f; // 分离力度分配，0.5代表两人各退一半

        private void Update()
        {
            // 在每一帧调用 PhysicsManager 的 Update 来推进物理模拟
            // 正常项目中，这里应该在负责整体逻辑循环的脚本中调用，或者放在 FixedUpdate 中
            PhysicsManager.Instance.Update();
        }

        private void OnDrawGizmos()
        {
            // 可视化所有的碰撞对连线
            if (!Application.isPlaying) return;

            foreach (var obj in PhysicsManager.Instance.CollisionObjects)
            {
                if (obj.IsColliding)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.green;
                }
                
                // 不再在这里绘制 OBB，因为 CollisionObject 里有更详细的绘制
                // 或者如果你想在这里画精确的 OBB：
                // Gizmos.matrix = Matrix4x4.TRS(obj.Position.ToVector3(), obj.Rotation, obj.Size.ToVector3());
                // Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                // Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }
}
