using System.Collections.Generic;
using UnityEngine;
using FixIntMath;
using XLHFrameWork.PhysicsFramework.GJKPhysics.Runtime;
using XLHFrameWork.PhysicsFramework.GJKPhysics.Shape;

namespace XLHFrameWork.PhysicsFramework
{
    public class PhysicsManager
    {
        private static PhysicsManager _instance;
        public static PhysicsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PhysicsManager();
                }
                return _instance;
            }
        }

        public IReadOnlyList<CollisionObject> CollisionObjects => _collisionObjects;

        private List<CollisionObject> _collisionObjects = new List<CollisionObject>();
        private Dictionary<int, CollisionObject> _objectMap = new Dictionary<int, CollisionObject>();
        
        // Key: Pair ID, Value: Collision Info from previous frame
        private Dictionary<CollisionPair, CollisionData> _activeCollisions = new Dictionary<CollisionPair, CollisionData>();
        
        private SAPBroadPhase _sap = new SAPBroadPhase();
        private GJK _gjk = new GJK();

        private struct CollisionData
        {
            public CollisionObject ObjA;
            public CollisionObject ObjB;
            public FixIntVector3 Normal; // From A to B (Separation direction for A relative to B)
            public FixInt Depth;
            public FixIntVector3 PointA;
            public FixIntVector3 PointB;
        }

        public void RegisterObject(CollisionObject obj)
        {
            if (!_collisionObjects.Contains(obj))
            {
                _collisionObjects.Add(obj);
                _objectMap[obj.ID] = obj;
            }
        }

        public void UnregisterObject(CollisionObject obj)
        {
            if (_collisionObjects.Contains(obj))
            {
                _collisionObjects.Remove(obj);
                _objectMap.Remove(obj.ID);
                
                // Remove active collisions involving this object
                List<CollisionPair> toRemove = new List<CollisionPair>();
                foreach (var pair in _activeCollisions.Keys)
                {
                    if (pair.ID1 == obj.ID || pair.ID2 == obj.ID)
                    {
                        toRemove.Add(pair);
                    }
                }
                
                foreach (var pair in toRemove)
                {
                    _activeCollisions.Remove(pair);
                }
            }
        }

        public void Update()
        {
            // 1. Update all bounds (同步 Transform 并在内部更新 Shape)
            for (int i = 0; i < _collisionObjects.Count; i++)
            {
                _collisionObjects[i].UpdateBounds();
                _collisionObjects[i].SetColliding(false); // Reset visual state
            }

            // 2. Broadphase (SAP)
            HashSet<CollisionPair> potentialPairs = _sap.Execute(_collisionObjects);
            
            // 3. Narrowphase (GJK + EPA)
            HashSet<CollisionPair> currentFrameCollisions = new HashSet<CollisionPair>();

            foreach (var pair in potentialPairs)
            {
                if (!_objectMap.TryGetValue(pair.ID1, out var objA) || !_objectMap.TryGetValue(pair.ID2, out var objB))
                    continue;

                // Narrow phase check
                if (_gjk.Intersect(objA.Shape, objB.Shape, out FixIntVector3 normal, out FixInt depth, out FixIntVector3 pA, out FixIntVector3 pB))
                {
                    // Collision detected!
                    currentFrameCollisions.Add(pair);
                    
                    // Update visual state
                    objA.SetColliding(true);
                    objB.SetColliding(true);

                    // Prepare data
                    CollisionData data = new CollisionData
                    {
                        ObjA = objA,
                        ObjB = objB,
                        Normal = normal,
                        Depth = depth,
                        PointA = pA,
                        PointB = pB
                    };

                    // Check if this is a new collision (Enter) or existing (Stay)
                    if (!_activeCollisions.ContainsKey(pair))
                    {
                        // OnCollisionEnter
                        DispatchCollisionEvent(data, true);
                    }
                    else
                    {
                        // OnCollisionStay
                        DispatchCollisionEvent(data, true, true);
                    }
                    
                    // Update the active collision record with latest data
                    _activeCollisions[pair] = data;
                }
            }

            // 4. Handle Exits
            // Find pairs that were active but are not in current frame
            List<CollisionPair> endedCollisions = new List<CollisionPair>();
            foreach (var pair in _activeCollisions.Keys)
            {
                if (!currentFrameCollisions.Contains(pair))
                {
                    endedCollisions.Add(pair);
                }
            }

            foreach (var pair in endedCollisions)
            {
                if (_activeCollisions.TryGetValue(pair, out var data))
                {
                    // OnCollisionExit
                    DispatchCollisionEvent(data, false);
                    _activeCollisions.Remove(pair);
                }
            }

            // 5. 应用修正后，必须再次更新所有物体的边界和形状
            // 这是为了确保物理层的数据与修改后的 Transform 一致，防止下一帧 GJK 使用旧数据导致误判。
            for (int i = 0; i < _collisionObjects.Count; i++)
            {
                _collisionObjects[i].UpdateBounds();
            }
        }

        private void DispatchCollisionEvent(CollisionData data, bool isEnter, bool isStay = false)
        {
            // Normal is A->B (Separation Vector).
            // For A: Incoming normal is B->A (-Normal).
            // For B: Incoming normal is A->B (Normal).

            // Event for Object A
            CollisionInfo infoA = new CollisionInfo(
                data.ObjB,
                data.PointB, // Contact point on B
                -data.Normal, // Normal pointing towards A (from B)
                data.Depth
            );

            // Event for Object B
            CollisionInfo infoB = new CollisionInfo(
                data.ObjA,
                data.PointA, // Contact point on A
                data.Normal, // Normal pointing towards B (from A)
                data.Depth
            );

            if (isEnter)
            {
                if (!isStay)
                {
                    data.ObjA.TriggerEnter(infoA);
                    data.ObjB.TriggerEnter(infoB);
                }
                else
                {
                    data.ObjA.TriggerStay(infoA);
                    data.ObjB.TriggerStay(infoB);
                }
            }
            else
            {
                data.ObjA.TriggerExit(infoA);
                data.ObjB.TriggerExit(infoB);
            }
        }
    }
}
