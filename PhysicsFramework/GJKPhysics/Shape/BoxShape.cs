using UnityEngine;
using FixIntMath;

namespace XLHFrameWork.PhysicsFramework.GJKPhysics.Shape
{
    public class BoxShape : IConvexShape
    {
        public FixIntVector3 Center { get; private set; }
        public Quaternion Rotation { get; private set; }
        public FixIntVector3 Extents { get; private set; }
        
        public void UpdateShape(Vector3 center, Quaternion rotation, Vector3 size)
        {
            Center = new FixIntVector3(center);
            Rotation = rotation;
            Extents = new FixIntVector3(size) * (FixInt)0.5f;
        }

        public SupportPoint Support(FixIntVector3 direction)
        {
            // 1. 将搜索方向转换到 Box 的本地坐标系
            Vector3 dirVec = direction.ToVector3();
            // 修复：求逆矩阵。因为我们要把世界坐标下的方向转到本地坐标系
            Vector3 localDirVec = Quaternion.Inverse(Rotation) * dirVec;
            FixIntVector3 localDir = new FixIntVector3(localDirVec);
            
            // 2. 在本地坐标系中找最远点
            // FixIntMath.Sign 当值为0时返回0，在Support函数中，当方向为0时，我们希望返回正边界，所以不能简单的用Sign，需要处理0的情况。
            // 为了安全起见，这里手写符号判断，确保落在顶点上
            FixInt x = localDir.x >= FixInt.Zero ? Extents.x : -Extents.x;
            FixInt y = localDir.y >= FixInt.Zero ? Extents.y : -Extents.y;
            FixInt z = localDir.z >= FixInt.Zero ? Extents.z : -Extents.z;
            FixIntVector3 localSupport = new FixIntVector3(x, y, z);
            
            // 3. 将本地最远点转换回世界坐标系
            Vector3 localSupportVec = localSupport.ToVector3();
            Vector3 worldPointVec = Rotation * localSupportVec;
            FixIntVector3 worldPoint = Center + new FixIntVector3(worldPointVec);
            
            return new SupportPoint
            {
                Point = worldPoint,
                SupportA = worldPoint, 
                SupportB = FixIntVector3.zero
            };
        }
    }
}
