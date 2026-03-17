using System.Collections.Generic;
using UnityEngine;

namespace XLHFrameWork.PhysicsFramework.GJKPhysics.Runtime
{
    public class Simplex
    {
        private List<Vector3> points = new List<Vector3>();

        public int Count => points.Count;

        public Vector3 this[int i]
        {
            get => points[i];
            set => points[i] = value;
        }

        public void Add(Vector3 p)
        {
            points.Insert(0, p); 
            // 新点放最前
        }

        public void RemoveAt(int index)
        {
            points.RemoveAt(index);
        }

        public void Set(params Vector3[] pts)
        {
            points.Clear();
            points.AddRange(pts);
        }
    }
}