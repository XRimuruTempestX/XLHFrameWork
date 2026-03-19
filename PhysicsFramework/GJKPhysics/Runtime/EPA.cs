using System.Collections.Generic;
using UnityEngine;
using XLHFrameWork.PhysicsFramework.GJKPhysics.Shape;
using FixIntMath;

namespace XLHFrameWork.PhysicsFramework.GJKPhysics.Runtime
{
    public class EPA
    {
        private static readonly FixInt epsilon = new FixInt(0.001f);
        private const int maxIterations = 64; // 增加最大迭代次数

        private struct Face
        {
            public SupportPoint a, b, c;
            public FixIntVector3 normal;
            public FixInt distance;
            public bool valid; // 标记是否为有效面

            public Face(SupportPoint a, SupportPoint b, SupportPoint c)
            {
                this.a = a;
                this.b = b;
                this.c = c;
                
                FixIntVector3 ab = b.Point - a.Point;
                FixIntVector3 ac = c.Point - a.Point;
                FixIntVector3 n = FixIntVector3.Cross(ab, ac);
                FixInt sqrLen = n.sqrMagnitude;

                // 1. 退化面检测
                if (sqrLen < epsilon)
                {
                    normal = FixIntVector3.zero;
                    distance = FixInt.Zero;
                    valid = false;
                    return;
                }

                normal = n.normalized;
                distance = FixIntVector3.Dot(normal, a.Point);
                valid = true;
                
                // 2. 强制距离为正，确保法线朝外
                if (distance < FixInt.Zero)
                {
                    normal = -normal;
                    distance = -distance;
                    SupportPoint temp = this.b;
                    this.b = this.c;
                    this.c = temp;
                }
            }
        }

        private struct Edge
        {
            public SupportPoint a, b;
            public Edge(SupportPoint a, SupportPoint b)
            {
                this.a = a;
                this.b = b;
            }
        }

        public bool ComputePenetration(Simplex simplex, IConvexShape shapeA, IConvexShape shapeB, out FixIntVector3 normal, out FixInt depth, out FixIntVector3 pA, out FixIntVector3 pB)
        {
            List<Face> faces = new List<Face>();
            List<Edge> edges = new List<Edge>();

            AddFace(faces, simplex[0], simplex[1], simplex[2]);
            AddFace(faces, simplex[0], simplex[2], simplex[3]);
            AddFace(faces, simplex[0], simplex[3], simplex[1]);
            AddFace(faces, simplex[1], simplex[3], simplex[2]);

            // 初始校验：如果没有有效面，直接退出
            if (faces.Count == 0)
            {
                normal = FixIntVector3.zero;
                depth = FixInt.Zero;
                pA = FixIntVector3.zero;
                pB = FixIntVector3.zero;
                return false;
            }

            int iteration = 0;
            while (iteration < maxIterations)
            {
                int closestIndex = -1;
                FixInt minDist = FixInt.MaxValue;

                // 寻找距离最近的面
                for (int i = 0; i < faces.Count; i++)
                {
                    if (faces[i].distance < minDist)
                    {
                        minDist = faces[i].distance;
                        closestIndex = i;
                    }
                }
                
                if (closestIndex == -1) break; // Should not happen if faces > 0
                Face closestFace = faces[closestIndex];

                FixIntVector3 searchDir = closestFace.normal;
                
                var sA = shapeA.Support(searchDir);
                var sB = shapeB.Support(-searchDir);
                
                SupportPoint p = new SupportPoint
                {
                    Point = sA.Point - sB.Point,
                    SupportA = sA.Point,
                    SupportB = sB.Point
                };

                // 3. 检查新点是否重复 (防止死循环)
                if ((p.Point - closestFace.a.Point).sqrMagnitude < epsilon || 
                    (p.Point - closestFace.b.Point).sqrMagnitude < epsilon || 
                    (p.Point - closestFace.c.Point).sqrMagnitude < epsilon)
                {
                    normal = closestFace.normal;
                    depth = closestFace.distance;
                    CalculateContactPoints(normal * depth, closestFace, out pA, out pB);
                    return true;
                }

                FixInt d = FixIntVector3.Dot(p.Point, searchDir);
                if (d - closestFace.distance <= epsilon * FixIntMath.FixIntMath.Max(FixInt.One, d))
                {
                    normal = closestFace.normal;
                    depth = closestFace.distance;
                    CalculateContactPoints(normal * depth, closestFace, out pA, out pB);
                    return true;
                }

                edges.Clear();
                for (int i = 0; i < faces.Count; i++)
                {
                    if (FixIntVector3.Dot(faces[i].normal, p.Point - faces[i].a.Point) > FixInt.Zero)
                    {
                        AddEdge(edges, faces[i].a, faces[i].b);
                        AddEdge(edges, faces[i].b, faces[i].c);
                        AddEdge(edges, faces[i].c, faces[i].a);
                        
                        faces[i] = faces[faces.Count - 1];
                        faces.RemoveAt(faces.Count - 1);
                        i--;
                    }
                }

                foreach (var edge in edges)
                {
                    AddFace(faces, edge.a, edge.b, p);
                }

                // 如果扩展后没有有效面了（极端退化），退出循环返回上一个最佳结果
                if (faces.Count == 0) break;

                iteration++;
            }

            // Fallback
            if (faces.Count > 0)
            {
                int bestIndex = 0;
                FixInt bestDist = faces[0].distance;
                for (int i = 1; i < faces.Count; i++)
                {
                    if (faces[i].distance < bestDist)
                    {
                        bestDist = faces[i].distance;
                        bestIndex = i;
                    }
                }
                normal = faces[bestIndex].normal;
                depth = bestDist;
                CalculateContactPoints(normal * depth, faces[bestIndex], out pA, out pB);
                return true;
            }
            
            normal = FixIntVector3.up;
            depth = FixInt.Zero;
            pA = FixIntVector3.zero;
            pB = FixIntVector3.zero;
            return false;
        }

        private void CalculateContactPoints(FixIntVector3 p, Face face, out FixIntVector3 pA, out FixIntVector3 pB)
        {
            // 计算重心坐标
            FixIntVector3 v0 = face.b.Point - face.a.Point;
            FixIntVector3 v1 = face.c.Point - face.a.Point;
            FixIntVector3 v2 = p - face.a.Point;

            FixInt d00 = FixIntVector3.Dot(v0, v0);
            FixInt d01 = FixIntVector3.Dot(v0, v1);
            FixInt d11 = FixIntVector3.Dot(v1, v1);
            FixInt d20 = FixIntVector3.Dot(v2, v0);
            FixInt d21 = FixIntVector3.Dot(v2, v1);

            FixInt denom = d00 * d11 - d01 * d01;
            
            // 防止除零
            if (FixIntMath.FixIntMath.Abs(denom) < epsilon)
            {
                // 退化情况，直接取 A 点
                pA = face.a.SupportA;
                pB = face.a.SupportB;
                return;
            }

            FixInt v = (d11 * d20 - d01 * d21) / denom;
            FixInt w = (d00 * d21 - d01 * d20) / denom;
            FixInt u = FixInt.One - v - w;

            pA = u * face.a.SupportA + v * face.b.SupportA + w * face.c.SupportA;
            pB = u * face.a.SupportB + v * face.b.SupportB + w * face.c.SupportB;
        }

        private void AddFace(List<Face> faces, SupportPoint a, SupportPoint b, SupportPoint c)
        {
            Face f = new Face(a, b, c);
            if (f.valid)
            {
                faces.Add(f);
            }
        }

        private void AddEdge(List<Edge> edges, SupportPoint a, SupportPoint b)
        {
            // 维护地平线边列表
            // 如果找到反向边 (b, a)，说明是内部共享边，移除它
            // 优化：使用略微宽松的 epsilon 防止断裂
            // const float edgeEpsilon = 1e-5f; 
            
            for (int i = 0; i < edges.Count; i++)
            {
                // 检查反向边
                if ((edges[i].a.Point - b.Point).sqrMagnitude < epsilon && (edges[i].b.Point - a.Point).sqrMagnitude < epsilon)
                {
                    // 优化：Swap Remove
                    edges[i] = edges[edges.Count - 1];
                    edges.RemoveAt(edges.Count - 1);
                    return;
                }
            }
            edges.Add(new Edge(a, b));
        }
    }
}
