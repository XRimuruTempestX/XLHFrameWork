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

                simplex.Add(newPos);

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
                            return true;
                        }
                        break;
                }

                iteration++;
            }

            return false;
        }

        private bool Line(ref Simplex simplex, ref Vector3 direction)
        {
            Vector3 A = simplex[0];
            Vector3 B = simplex[1];

            Vector3 AB = B - A;
            Vector3 AO = -A;

            //如果两个向量夹角大于90，则需要回退为一个点
            if (Vector3.Dot(AB, AO) > 0)
            {
                //找垂直于AB过原点的向量
                direction = Vector3.Cross(Vector3.Cross(AB, AO), AB);
                if (direction.sqrMagnitude < epsilon)
                {
                    // 兜底：找一个垂直 AB 的方向
                    direction = Vector3.Cross(AB, Vector3.right);
                    if (direction.sqrMagnitude < epsilon)
                        direction = Vector3.Cross(AB, Vector3.up);
                }

                if (Vector3.Dot(direction, AO) < epsilon)
                {
                    direction = -direction;
                }
                
            }
            else
            {
                simplex.Set(A);
                direction = AO;
            }

            return false;
        }

        private bool Triangle(ref Simplex simplex, ref Vector3 direction)
        {
            Vector3 A = simplex[0];
            Vector3 B = simplex[1];
            Vector3 C = simplex[2];

            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 AO = -A;
            //三角形法线
            Vector3 ABC = Vector3.Cross(AB, AC);

            if (Vector3.Dot(ABC, AO) < 0)
            {
                simplex.Set(A, C, B);

                B = simplex[1];
                C = simplex[2];

                AB = B - A;
                AC = C - A;

                ABC = Vector3.Cross(AB, AC); // 重新算
            }

            Vector3 AB_Out = Vector3.Cross(ABC, AB);

            if (Vector3.Dot(AB_Out, AO) > 0)
            {
                //同侧  则在外部 进行回退 去除C 因为C点连成的简单型没有包裹原点
                simplex.Set(A,B);
                Line(ref simplex, ref direction);
                return false;
            }

            Vector3 AC_Out = Vector3.Cross(AC, ABC);
            if (Vector3.Dot(AC_Out, AO) > 0)
            {
                //同侧  则在外部 进行回退 去除C 因为C点连成的简单型没有包裹原点
                simplex.Set(A,C);
                Line(ref simplex, ref direction);
                return false;
            }

            direction = ABC;
            return false;
        }

        private bool Tetrahedron(ref Simplex simplex, ref Vector3 direction)
        {
            Vector3 A = simplex[0];
            Vector3 B = simplex[1];
            Vector3 C = simplex[2];
            Vector3 D = simplex[3];

            Vector3 AO = -A;

            // --- 面 ABC ---
            Vector3 ABC = Vector3.Cross(B - A, C - A);
            if (Vector3.Dot(ABC, AO) > 0)
            {
                simplex.Set(A, B, C);
                return Triangle(ref simplex, ref direction);
            }

            // --- 面 ACD ---
            Vector3 ACD = Vector3.Cross(C - A, D - A);
            if (Vector3.Dot(ACD, AO) > 0)
            {
                simplex.Set(A, C, D);
                return Triangle(ref simplex, ref direction);
            }

            // --- 面 ADB ---
            Vector3 ADB = Vector3.Cross(D - A, B - A);
            if (Vector3.Dot(ADB, AO) > 0)
            {
                simplex.Set(A, D, B);
                return Triangle(ref simplex, ref direction);
            }

            return true;
        }
    }
}