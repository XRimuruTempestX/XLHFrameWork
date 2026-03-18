using UnityEngine;
using XLHFrameWork.PhysicsFramework.GJKPhysics.Runtime;
using XLHFrameWork.PhysicsFramework.GJKPhysics.Shape;
using FixIntMath;

namespace XLHFrameWork.PhysicsFramework
{
    public class CollisionTest : MonoBehaviour
    {
        [Header("Objects")]
        public Transform ObjectA;
        public Transform ObjectB;
        public Vector3 SizeA = Vector3.one;
        public Vector3 SizeB = Vector3.one;

        [Header("Collision Resolution")]
        public bool EnableCorrection = true;
        [Range(0, 1)] public float CorrectionPercent = 1.0f; // 1.0 means full separation

        private BoxShape _shapeA = new BoxShape();
        private BoxShape _shapeB = new BoxShape();
        private GJK _gjk = new GJK();

        // Debug info
        private Vector3 _normal;
        private float _depth;
        private Vector3 _contactA;
        private Vector3 _contactB;
        private bool _isColliding;

        private void Update()
        {
            if (ObjectA == null || ObjectB == null) return;

            // 1. Update Shapes
            _shapeA.UpdateShape(ObjectA.position, ObjectA.rotation, SizeA);
            _shapeB.UpdateShape(ObjectB.position, ObjectB.rotation, SizeB);

            // 2. Run GJK + EPA
            // Note: GJK.Intersect has been updated to return contact points
            FixIntVector3 normalFix;
            FixInt depthFix;
            FixIntVector3 pAFix;
            FixIntVector3 pBFix;
            
            _isColliding = _gjk.Intersect(_shapeA, _shapeB, out normalFix, out depthFix, out pAFix, out pBFix);
            
            _normal = normalFix.ToVector3();
            _depth = (float)depthFix;
            _contactA = pAFix.ToVector3();
            _contactB = pBFix.ToVector3();

            // 3. Resolve Collision
            if (_isColliding && EnableCorrection && _depth > 0)
            {
                // Normal points from Origin to Boundary of Minkowski Difference (A - B).
                // To separate A from B, we need to move A in direction -Normal * Depth
                // Or split the move between A and B.
                
                Vector3 separationVector = _normal * _depth * CorrectionPercent;
                
                // Move A away (opposite to normal)
                ObjectA.position -= separationVector * 0.5f;
                
                // Move B away (along normal)
                ObjectB.position += separationVector * 0.5f;
            }
        }

        private void OnDrawGizmos()
        {
            if (ObjectA == null || ObjectB == null) return;

            // Draw OBBs
            Gizmos.color = _isColliding ? Color.red : Color.green;
            
            Gizmos.matrix = Matrix4x4.TRS(ObjectA.position, ObjectA.rotation, SizeA);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            
            Gizmos.matrix = Matrix4x4.TRS(ObjectB.position, ObjectB.rotation, SizeB);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

            Gizmos.matrix = Matrix4x4.identity;

            if (_isColliding)
            {
                // Draw contact points
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(_contactA, 0.05f);
                Gizmos.DrawSphere(_contactB, 0.05f);
                
                // Draw collision normal (from B to A usually, but EPA normal is from Origin to Boundary)
                // Let's visualize the separation direction
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(_contactA, _contactA - _normal * _depth); // Direction A should move?
                
                // Draw line between contacts
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(_contactA, _contactB);
            }
        }
    }
}
