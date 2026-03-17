using UnityEngine;
using XLHFrameWork.PhysicsFramework.GJKPhysics.Shape;

namespace XLHFrameWork.PhysicsFramework.GJKPhysics.Runtime
{
    public class GJK
    {
        private const float epsilon = 1e-6f;

        public bool Intersect(IConvexShape shapeA, IConvexShape shapeB)
        {
            //1.取得第一个点
            Vector3 direction = Vector3.right;
            Vector3 A = shapeA.Support(direction) - shapeB.Support(-direction);

            //如果第一个支撑点是原点  直接返回True
            if (A.sqrMagnitude < epsilon)
            {
                return true;
            }

            direction = -A;

            Simplex simplex = new Simplex();
            simplex.Add(A);

            int maxIterations = 20;
            int iteration = 0;

            while (iteration <= maxIterations)
            {
                Vector3 newPos = shapeA.Support(direction) - shapeB.Support(-direction);

                //背离原点
                if (Vector3.Dot(newPos, direction) <= 0)
                {
                    return false;
                }

                // 添加重复点检查，防止震荡
                for (int i = 0; i < simplex.Count; i++)
                {
                    if ((simplex[i] - newPos).sqrMagnitude < epsilon)
                    {
                        Debug.LogWarning($"[GJK] Duplicate point detected at iteration {iteration}. Count: {simplex.Count}. Point: {newPos}. Escaping oscillation.");
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
                Debug.Log($"[GJK] Iteration {iteration}: Simplex {prevCount} -> {simplex.Count}, Direction: {direction}, NewPos: {newPos}");

                if (isIntersecting) return true;

                iteration++;
            }

            Debug.LogWarning("GJK max iterations reached, might be oscillating.");
            return false;
        }

        private bool Line(ref Simplex simplex, ref Vector3 direction)
        {
            Vector3 a = simplex[0];
            Vector3 b = simplex[1];

            Vector3 ab = b - a;
            Vector3 ao = -a;

            if (Vector3.Dot(ab, ao) > 0)
            {
                direction = Vector3.Cross(Vector3.Cross(ab, ao), ab);
                if (direction.sqrMagnitude < epsilon)
                {
                    // 兜底：寻找任意垂直方向
                    direction = Vector3.Cross(ab, Vector3.right);
                    if (direction.sqrMagnitude < epsilon)
                    {
                        direction = Vector3.Cross(ab, Vector3.up);
                        if (direction.sqrMagnitude < epsilon)
                        {
                            direction = Vector3.Cross(ab, Vector3.forward);
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

        private bool Triangle(ref Simplex simplex, ref Vector3 direction)
        {
            Vector3 a = simplex[0];
            Vector3 b = simplex[1];
            Vector3 c = simplex[2];

            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 ao = -a;

            Vector3 abc = Vector3.Cross(ab, ac);

            // 检查原点是否在 AC 的外侧
            if (Vector3.Dot(Vector3.Cross(abc, ac), ao) > 0)
            {
                if (Vector3.Dot(ac, ao) > 0)
                {
                    simplex.Set(a, c);
                    direction = Vector3.Cross(Vector3.Cross(ac, ao), ac);
                }
                else
                {
                    simplex.Set(a, c);
                    return Line(ref simplex, ref direction);
                }
            }
            
            // 检查原点是否在 AB 的外侧
            if (Vector3.Dot(Vector3.Cross(ab, abc), ao) > 0)
            {
                simplex.Set(a, b);
                return Line(ref simplex, ref direction);
            }

            // 原点在三角形上方或下方
            if (Vector3.Dot(abc, ao) > 0)
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
                direction = Vector3.up; 
            }

            return false;
        }

        private bool Tetrahedron(ref Simplex simplex, ref Vector3 direction)
        {
            Vector3 a = simplex[0];
            Vector3 b = simplex[1];
            Vector3 c = simplex[2];
            Vector3 d = simplex[3];

            Vector3 ao = -a;
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 ad = d - a;

            // 面的法线
            Vector3 abc = Vector3.Cross(ab, ac);
            Vector3 acd = Vector3.Cross(ac, ad);
            Vector3 adb = Vector3.Cross(ad, ab);

            // 检查原点是否在 ABC 面外
            if (Vector3.Dot(abc, ao) > 0)
            {
                simplex.Set(a, b, c);
                return Triangle(ref simplex, ref direction);
            }

            // 检查原点是否在 ACD 面外
            if (Vector3.Dot(acd, ao) > 0)
            {
                simplex.Set(a, c, d);
                return Triangle(ref simplex, ref direction);
            }

            // 检查原点是否在 ADB 面外
            if (Vector3.Dot(adb, ao) > 0)
            {
                simplex.Set(a, d, b);
                return Triangle(ref simplex, ref direction);
            }

            // 原点在四面体内部
            return true;
        }
    }
}