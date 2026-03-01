using System;
using FixIntMath;
using UnityEngine;

namespace XLHFrameWork.PhysicsFramework
{
    public class CollisionTest : MonoBehaviour
    {
        public int ObjectCount = 10;
        public FixInt MoveSpeed = (FixInt)5;
        public FixIntVector3 AreaSize = new FixIntVector3(10, 10, 10);

        private struct Mover
        {
            public CollisionObject collisionObject;
            public FixIntVector3 velocity;
        }

        private Mover[] _movers;
        private System.Random _random;

        private void Start()
        {
            if (PhysicsManager.Instance == null)
            {
                GameObject pm = new GameObject("PhysicsManager");
                pm.AddComponent<PhysicsManager>();
            }

            _movers = new Mover[ObjectCount];
            _random = new System.Random(12345); // Deterministic seed

            for (int i = 0; i < ObjectCount; i++)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Object_{i}";
                
                // Random position
                FixInt x = NextFixInt(-AreaSize.x, AreaSize.x);
                FixInt y = NextFixInt(-AreaSize.y, AreaSize.y);
                FixInt z = NextFixInt(-AreaSize.z, AreaSize.z);
                
                FixIntVector3 pos = new FixIntVector3(x, y, z);
                go.transform.position = pos.ToVector3();
                
                // Add CollisionObject
                var co = go.AddComponent<CollisionObject>();
                co.Size = FixIntVector3.one; 
                co.Position = pos;
                co.UpdateBounds(); // Initial bounds update
                
                // Random velocity
                FixInt vx = NextFixInt(-1, 1);
                FixInt vy = NextFixInt(-1, 1);
                FixInt vz = NextFixInt(-1, 1);
                FixIntVector3 dir = new FixIntVector3(vx, vy, vz).normalized;

                _movers[i] = new Mover
                {
                    collisionObject = co,
                    velocity = dir * MoveSpeed
                };
            }
        }

        private FixInt NextFixInt(FixInt min, FixInt max)
        {
            long minVal = min.Value;
            long maxVal = max.Value;
            long range = maxVal - minVal;
            if (range <= 0) return min;
            
            // Generate a random long in [0, range)
            // Note: System.Random.Next() returns int, not long coverage.
            // For better distribution we can use NextDouble, though it involves float math.
            // Since this is just a test script, NextDouble is acceptable.
            double rnd = _random.NextDouble();
            long offset = (long)(rnd * range);
            
            return new FixInt(minVal + offset);
        }

        private void Update()
        {
            if (_movers == null) return;

            FixInt deltaTime = (FixInt)Time.deltaTime;

            for (int i = 0; i < _movers.Length; i++)
            {
                var m = _movers[i];
                
                // Update Logical Position
                m.collisionObject.Position += m.velocity * deltaTime;

                // Bounce off walls
                // Using full qualification for FixIntMath class to avoid namespace conflict
                if (FixIntMath.FixIntMath.Abs(m.collisionObject.Position.x) > AreaSize.x) m.velocity.x *= -1;
                if (FixIntMath.FixIntMath.Abs(m.collisionObject.Position.y) > AreaSize.y) m.velocity.y *= -1;
                if (FixIntMath.FixIntMath.Abs(m.collisionObject.Position.z) > AreaSize.z) m.velocity.z *= -1;

                // Sync View
                m.collisionObject.SyncView();
                
                _movers[i] = m; // Update struct back to array
            }
        }
    }
}
