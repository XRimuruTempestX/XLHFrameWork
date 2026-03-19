using FixIntMath;

namespace XLHFrameWork.PhysicsFramework
{
    public struct CollisionInfo
    {
        public CollisionObject Collider; // The other object involved in the collision
        public FixIntVector3 ContactPoint; // The contact point on 'Collider'
        public FixIntVector3 Normal; // The collision normal pointing towards 'Collider'
        public FixInt PenetrationDepth; // Depth of penetration

        public CollisionInfo(CollisionObject collider, FixIntVector3 contactPoint, FixIntVector3 normal, FixInt depth)
        {
            Collider = collider;
            ContactPoint = contactPoint;
            Normal = normal;
            PenetrationDepth = depth;
        }
    }
}
