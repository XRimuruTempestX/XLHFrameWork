using UnityEngine;

namespace XLHFrameWork.PhysicsFramework.GJKPhysics.Shape
{
    public class BoxShape : IConvexShape
    {
        public Vector3[] vertices;
        
        public void UpdateVertices(Vector3 center, Quaternion rotation, Vector3 size)
        {
            Vector3 extents = size * 0.5f;
            if (vertices == null || vertices.Length != 8)
            {
                vertices = new Vector3[8];
            }
            
            // Generate 8 corners
            vertices[0] = center + rotation * new Vector3( extents.x,  extents.y,  extents.z);
            vertices[1] = center + rotation * new Vector3( extents.x,  extents.y, -extents.z);
            vertices[2] = center + rotation * new Vector3( extents.x, -extents.y,  extents.z);
            vertices[3] = center + rotation * new Vector3( extents.x, -extents.y, -extents.z);
            vertices[4] = center + rotation * new Vector3(-extents.x,  extents.y,  extents.z);
            vertices[5] = center + rotation * new Vector3(-extents.x,  extents.y, -extents.z);
            vertices[6] = center + rotation * new Vector3(-extents.x, -extents.y,  extents.z);
            vertices[7] = center + rotation * new Vector3(-extents.x, -extents.y, -extents.z);
        }

        public Vector3 Support(Vector3 direction)
        {
            float maxDot = float.MinValue;
            Vector3 best = Vector3.zero;

            foreach (var v in vertices)
            {
                float dot = Vector3.Dot(v, direction);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    best = v;
                }
            }

            return best;
        }
    }
}