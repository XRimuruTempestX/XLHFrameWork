using FixIntMath;
using UnityEngine;

namespace XLHFrameWork.PhysicsFramework
{
    [System.Serializable]
    public struct AABB
    {
        public FixIntVector3 Min;
        public FixIntVector3 Max;

        public FixIntVector3 Center => (Min + Max) / 2;
        public FixIntVector3 Size => Max - Min;

        public AABB(FixIntVector3 min, FixIntVector3 max)
        {
            Min = min;
            Max = max;
        }

        public bool Intersects(AABB other)
        {
            return (Min.x <= other.Max.x && Max.x >= other.Min.x) &&
                   (Min.y <= other.Max.y && Max.y >= other.Min.y) &&
                   (Min.z <= other.Max.z && Max.z >= other.Min.z);
        }
    }
}
