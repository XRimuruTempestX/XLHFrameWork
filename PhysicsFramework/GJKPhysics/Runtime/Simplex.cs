using System;
using XLHFrameWork.PhysicsFramework.GJKPhysics.Shape;

namespace XLHFrameWork.PhysicsFramework.GJKPhysics.Runtime
{
    public struct Simplex
    {
        private SupportPoint v0, v1, v2, v3;

        public int Count { get; private set; }

        public SupportPoint this[int i]
        {
            get
            {
                if (i == 0) return v0;
                if (i == 1) return v1;
                if (i == 2) return v2;
                return v3;
            }
        }

        public void Add(SupportPoint p)
        {
            v3 = v2;
            v2 = v1;
            v1 = v0;
            v0 = p;
            Count = Math.Min(Count + 1, 4);
        }

        public void Set(SupportPoint a) { v0 = a; Count = 1; }
        public void Set(SupportPoint a, SupportPoint b) { v0 = a; v1 = b; Count = 2; }
        public void Set(SupportPoint a, SupportPoint b, SupportPoint c) { v0 = a; v1 = b; v2 = c; Count = 3; }
        public void Set(SupportPoint a, SupportPoint b, SupportPoint c, SupportPoint d) { v0 = a; v1 = b; v2 = c; v3 = d; Count = 4; }
    }
}
