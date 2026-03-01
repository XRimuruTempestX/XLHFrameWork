using System;

namespace XLHFrameWork.PhysicsFramework
{
    public struct CollisionPair : IEquatable<CollisionPair>
    {
        public readonly int ID1;
        public readonly int ID2;

        public CollisionPair(int id1, int id2)
        {
            if (id1 < id2)
            {
                ID1 = id1;
                ID2 = id2;
            }
            else
            {
                ID1 = id2;
                ID2 = id1;
            }
        }

        public bool Equals(CollisionPair other)
        {
            return ID1 == other.ID1 && ID2 == other.ID2;
        }

        public override bool Equals(object obj)
        {
            return obj is CollisionPair other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ID1 * 397) ^ ID2;
            }
        }
    }
}
