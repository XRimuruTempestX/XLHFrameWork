using UnityEngine;

namespace XLHFrameWork.PhysicsFramework.Test
{
    [RequireComponent(typeof(CollisionObject))]
    public class SimpleColliderBehavior : MonoBehaviour
    {
        private CollisionObject _collider;

        private void Awake()
        {
            _collider = GetComponent<CollisionObject>();
        }

        private void OnEnable()
        {
            _collider.OnCollisionEnter += HandleCollisionEnter;
            _collider.OnCollisionStay += HandleCollisionStay;
            _collider.OnCollisionExit += HandleCollisionExit;
        }

        private void OnDisable()
        {
            _collider.OnCollisionEnter -= HandleCollisionEnter;
            _collider.OnCollisionStay -= HandleCollisionStay;
            _collider.OnCollisionExit -= HandleCollisionExit;
        }

        private void HandleCollisionEnter(CollisionInfo info)
        {
            Debug.Log($"<color=orange>[Collision Enter]</color> {gameObject.name} hit {info.Collider.gameObject.name}. " +
                      $"ContactPoint: {info.ContactPoint}, Depth: {(float)info.PenetrationDepth}");
            
            ResolveCollision(info);
        }

        private void HandleCollisionStay(CollisionInfo info)
        {
            // 在 Stay 阶段持续修正穿透
            ResolveCollision(info);
        }

        private void ResolveCollision(CollisionInfo info)
        {
            // info.Normal 是指向自己内部的法线（对方推我的方向）
            var testManager = FindObjectOfType<PhysicsManagerTest>();
            if (testManager != null && testManager.EnableCorrection && info.PenetrationDepth > FixIntMath.FixInt.Zero)
            {
                // 将自己沿法线方向推开 (这里简单处理，各退一半)
                Vector3 moveDir = info.Normal.ToVector3() * (float)info.PenetrationDepth * testManager.CorrectionPercent;
                transform.position += moveDir;
                _collider.Position += new FixIntMath.FixIntVector3(moveDir);
                
                // 立即同步内部的包围盒和形状，防止同一帧内其他碰撞判断出错
                _collider.UpdateBounds();
            }
        }

        private void HandleCollisionExit(CollisionInfo info)
        {
            Debug.Log($"<color=cyan>[Collision Exit]</color> {gameObject.name} separated from {info.Collider.gameObject.name}.");
        }

        private void OnDrawGizmos()
        {
            // 如果处于碰撞状态，绘制法线和接触点，方便调试
            if (_collider != null && _collider.IsColliding && Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(transform.position, 0.1f);
            }
        }
    }
}
