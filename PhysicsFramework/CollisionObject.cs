using System;
using FixIntMath;
using UnityEngine;

namespace XLHFrameWork.PhysicsFramework
{
    public class CollisionObject : MonoBehaviour
    {
        public int ID { get; private set; }
        public AABB Bounds;
        public FixIntVector3 Size = FixIntVector3.one;
        public FixIntVector3 Position;
        public bool IsColliding { get; private set; }
        
        public event Action<CollisionObject> OnCollisionEnter;
        public event Action<CollisionObject> OnCollisionExit;

        private static int _nextId = 0;

        private void Awake()
        {
            ID = _nextId++;
            Position = new FixIntVector3(transform.position);
            UpdateBounds();
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
            // Sync Position from transform if needed, or vice versa. 
            // In a pure deterministic simulation, Position drives transform.
            // But for ease of use in editor, we can take transform.position if not running simulation logic.
            // For now, let's assume the external system updates Position.
            
            FixIntVector3 halfSize = Size / 2;
            Bounds = new AABB(Position - halfSize, Position + halfSize);
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
