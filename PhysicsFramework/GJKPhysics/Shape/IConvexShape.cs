using UnityEngine;

namespace XLHFrameWork.PhysicsFramework.GJKPhysics.Shape
{
    public interface IConvexShape
    {
        Vector3 Support(Vector3 direction);
    }
}