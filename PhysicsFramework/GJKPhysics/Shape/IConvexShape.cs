using FixIntMath;

namespace XLHFrameWork.PhysicsFramework.GJKPhysics.Shape
{
    public interface IConvexShape
    {
        SupportPoint Support(FixIntVector3 direction);
    }

    public struct SupportPoint
    {
        public FixIntVector3 Point; // Minkowski Difference Point (A - B)
        public FixIntVector3 SupportA; // Point on Shape A
        public FixIntVector3 SupportB; // Point on Shape B

        public static SupportPoint operator -(SupportPoint a, SupportPoint b)
        {
            return new SupportPoint
            {
                Point = a.Point - b.Point,
                SupportA = a.SupportA - b.SupportA,
                SupportB = a.SupportB - b.SupportB
            };
        }
    }
}
