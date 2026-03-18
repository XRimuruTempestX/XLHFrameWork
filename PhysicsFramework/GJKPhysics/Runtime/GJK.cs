using UnityEngine;
using XLHFrameWork.PhysicsFramework.GJKPhysics.Shape;
using FixIntMath;

namespace XLHFrameWork.PhysicsFramework.GJKPhysics.Runtime
{
    public class GJK
    {
        private static readonly FixInt epsilon = new FixInt(0.001f);
        private EPA _epa = new EPA();

        public bool Intersect(IConvexShape shapeA, IConvexShape shapeB, out FixIntVector3 normal, out FixInt depth, out FixIntVector3 pA, out FixIntVector3 pB)
        {
            normal = FixIntVector3.zero;
            depth = FixInt.Zero;
            pA = FixIntVector3.zero;
            pB = FixIntVector3.zero;

            //1.取得第一个点
            FixIntVector3 direction = FixIntVector3.right;
            var supportA = shapeA.Support(direction);
            var supportB = shapeB.Support(-direction);

            SupportPoint A = new SupportPoint
            {
                Point = supportA.Point - supportB.Point,
                SupportA = supportA.Point,
                SupportB = supportB.Point
            };

            //如果第一个支撑点是原点  直接返回True
            if (A.Point.sqrMagnitude < epsilon)
            {
                // 此时还没法算 EPA，因为没有单纯形。给一个默认穿透?
                // 或者认为刚好接触，深度为0
                return true;
            }

            direction = -A.Point;

            Simplex simplex = new Simplex();
            simplex.Add(A);

            int maxIterations = 20;
            int iteration = 0;

            while (iteration <= maxIterations)
            {
                var nextA = shapeA.Support(direction);
                var nextB = shapeB.Support(-direction);
                
                SupportPoint newPos = new SupportPoint
                {
                    Point = nextA.Point - nextB.Point,
                    SupportA = nextA.Point,
                    SupportB = nextB.Point
                };

                //背离原点
                if (FixIntVector3.Dot(newPos.Point, direction) <= FixInt.Zero)
                {
                    return false;
                }

                // 添加重复点检查，防止震荡
                for (int i = 0; i < simplex.Count; i++)
                {
                    if ((simplex[i].Point - newPos.Point).sqrMagnitude < epsilon)
                    {
                        Debug.LogWarning($"[GJK] Duplicate point detected at iteration {iteration}. Count: {simplex.Count}. Point: {newPos.Point}. Escaping oscillation.");
                        return false;
                    }
                }

                simplex.Add(newPos);

                int prevCount = simplex.Count;
                bool isIntersecting = false;

                switch (simplex.Count)
                {
                    case 2:
                        Line(ref simplex, ref direction);
                        break;
                    case 3:
                        Triangle(ref simplex, ref direction);
                        break;
                    case 4:
                        if (Tetrahedron(ref simplex, ref direction))
                        {
                            isIntersecting = true;
                        }
                        break;
                }

                // 打印每一帧的推进状态，用于排查死循环
              //  Debug.Log($"[GJK] Iteration {iteration}: Simplex {prevCount} -> {simplex.Count}, Direction: {direction}, NewPos: {newPos.Point}");

                if (isIntersecting)
                {
                    // GJK 确认碰撞，进入 EPA 计算穿透深度和法线
                    return _epa.ComputePenetration(simplex, shapeA, shapeB, out normal, out depth, out pA, out pB);
                }

                iteration++;
            }

            Debug.LogWarning("GJK max iterations reached, might be oscillating.");
            return false;
        }

        public bool Intersect(IConvexShape shapeA, IConvexShape shapeB)
        {
            return Intersect(shapeA, shapeB, out _, out _, out _, out _);
        }

        private bool Line(ref Simplex simplex, ref FixIntVector3 direction)
        {
            SupportPoint a = simplex[0];
            SupportPoint b = simplex[1];

            FixIntVector3 ab = b.Point - a.Point;
            FixIntVector3 ao = -a.Point;

            if (FixIntVector3.Dot(ab, ao) > FixInt.Zero)
            {
                direction = FixIntVector3.Cross(FixIntVector3.Cross(ab, ao), ab);
                if (direction.sqrMagnitude < epsilon)
                {
                    // 兜底：寻找任意垂直方向
                    direction = FixIntVector3.Cross(ab, FixIntVector3.right);
                    if (direction.sqrMagnitude < epsilon)
                    {
                        direction = FixIntVector3.Cross(ab, FixIntVector3.up);
                        if (direction.sqrMagnitude < epsilon)
                        {
                            direction = FixIntVector3.Cross(ab, FixIntVector3.forward);
                        }
                    }
                }
            }
            else
            {
                simplex.Set(a);
                direction = ao;
            }
            return false;
        }

        private bool Triangle(ref Simplex simplex, ref FixIntVector3 direction)
        {
            SupportPoint a = simplex[0];
            SupportPoint b = simplex[1];
            SupportPoint c = simplex[2];

            FixIntVector3 ab = b.Point - a.Point;
            FixIntVector3 ac = c.Point - a.Point;
            FixIntVector3 ao = -a.Point;

            FixIntVector3 abc = FixIntVector3.Cross(ab, ac);

            // 检查原点是否在 AC 的外侧
            if (FixIntVector3.Dot(FixIntVector3.Cross(abc, ac), ao) > FixInt.Zero)
            {
                if (FixIntVector3.Dot(ac, ao) > FixInt.Zero)
                {
                    simplex.Set(a, c);
                    direction = FixIntVector3.Cross(FixIntVector3.Cross(ac, ao), ac);
                }
                else
                {
                    simplex.Set(a, c);
                    return Line(ref simplex, ref direction);
                }
            }
            
            // 检查原点是否在 AB 的外侧
            if (FixIntVector3.Dot(FixIntVector3.Cross(ab, abc), ao) > FixInt.Zero)
            {
                simplex.Set(a, b);
                return Line(ref simplex, ref direction);
            }

            // 原点在三角形上方或下方
            if (FixIntVector3.Dot(abc, ao) > FixInt.Zero)
            {
                direction = abc;
            }
            else
            {
                simplex.Set(a, c, b); // 保持法线朝向原点
                direction = -abc;
            }

            // 处理方向为零的极特殊情况
            if (direction.sqrMagnitude < epsilon)
            {
                direction = FixIntVector3.up; 
            }

            return false;
        }

        private bool Tetrahedron(ref Simplex simplex, ref FixIntVector3 direction)
        {
            SupportPoint a = simplex[0];
            SupportPoint b = simplex[1];
            SupportPoint c = simplex[2];
            SupportPoint d = simplex[3];

            FixIntVector3 ao = -a.Point;
            FixIntVector3 ab = b.Point - a.Point;
            FixIntVector3 ac = c.Point - a.Point;
            FixIntVector3 ad = d.Point - a.Point;

            // 面的法线
            FixIntVector3 abc = FixIntVector3.Cross(ab, ac);
            FixIntVector3 acd = FixIntVector3.Cross(ac, ad);
            FixIntVector3 adb = FixIntVector3.Cross(ad, ab);

            // 检查原点是否在 ABC 面外
            if (FixIntVector3.Dot(abc, ao) > FixInt.Zero)
            {
                simplex.Set(a, b, c);
                return Triangle(ref simplex, ref direction);
            }

            // 检查原点是否在 ACD 面外
            if (FixIntVector3.Dot(acd, ao) > FixInt.Zero)
            {
                simplex.Set(a, c, d);
                return Triangle(ref simplex, ref direction);
            }

            // 检查原点是否在 ADB 面外
            if (FixIntVector3.Dot(adb, ao) > FixInt.Zero)
            {
                simplex.Set(a, d, b);
                return Triangle(ref simplex, ref direction);
            }

            // 原点在四面体内部
            return true;
        }
    }
}
