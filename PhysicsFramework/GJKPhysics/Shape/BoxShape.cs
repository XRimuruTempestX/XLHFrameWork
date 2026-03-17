using UnityEngine;

namespace XLHFrameWork.PhysicsFramework.GJKPhysics.Shape
{
    public class BoxShape : IConvexShape
    {
        public Vector3 Center { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Vector3 Extents { get; private set; }
        
        public void UpdateShape(Vector3 center, Quaternion rotation, Vector3 size)
        {
            Center = center;
            Rotation = rotation;
            Extents = size * 0.5f;
        }

        public Vector3 Support(Vector3 direction)
        {
            // 1. 将搜索方向转换到 Box 的本地坐标系
            Vector3 localDir = Quaternion.Inverse(Rotation) * direction;
            
            // 2. 在本地坐标系中找最远点 (利用 Extents 和符号)
            Vector3 localSupport = new Vector3(
                Mathf.Sign(localDir.x) * Extents.x,
                Mathf.Sign(localDir.y) * Extents.y,
                Mathf.Sign(localDir.z) * Extents.z
            );
            
            // 3. 将本地最远点转换回世界坐标系
            return Center + Rotation * localSupport;
        }
    }
}