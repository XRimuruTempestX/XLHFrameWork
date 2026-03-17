using System.Collections.Generic;
using UnityEngine;

namespace XLHFrameWork.PhysicsFramework
{
    [DefaultExecutionOrder(-100)]
    public class PhysicsManager : MonoBehaviour
    {
        public static PhysicsManager Instance { get; private set; }

        public IReadOnlyList<CollisionObject> CollisionObjects => _collisionObjects;

        private List<CollisionObject> _collisionObjects = new List<CollisionObject>();
        private Dictionary<int, CollisionObject> _objectMap = new Dictionary<int, CollisionObject>();
        private HashSet<CollisionPair> _previousPairs = new HashSet<CollisionPair>();
        private SAPBroadPhase _sap = new SAPBroadPhase();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
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
            }
        }

        private void Update()
        {
            // Update all bounds first
            foreach (var obj in _collisionObjects)
            {
                obj.UpdateBounds();
                obj.SetColliding(false); // Reset collision state
            }

            // Execute SAP
            HashSet<CollisionPair> currentPairs = _sap.Execute(_collisionObjects);

            // Handle Enter
            foreach (var pair in currentPairs)
            {
                if (!_previousPairs.Contains(pair))
                {
                    DispatchEvent(pair, true);
                }
                
                // Set collision state for visual feedback
                if (_objectMap.TryGetValue(pair.ID1, out var obj1)) obj1.SetColliding(true);
                if (_objectMap.TryGetValue(pair.ID2, out var obj2)) obj2.SetColliding(true);
            }

            // Handle Exit
            foreach (var pair in _previousPairs)
            {
                if (!currentPairs.Contains(pair))
                {
                    DispatchEvent(pair, false);
                }
            }

            _previousPairs = currentPairs;
        }

        private void DispatchEvent(CollisionPair pair, bool enter)
        {
            if (_objectMap.TryGetValue(pair.ID1, out var obj1) && _objectMap.TryGetValue(pair.ID2, out var obj2))
            {
                if (enter)
                {
                    obj1.TriggerEnter(obj2);
                    obj2.TriggerEnter(obj1);
                    // Debug.Log($"Collision Enter: {obj1.name} <-> {obj2.name}");
                }
                else
                {
                    obj1.TriggerExit(obj2);
                    obj2.TriggerExit(obj1);
                    // Debug.Log($"Collision Exit: {obj1.name} <-> {obj2.name}");
                }
            }
        }
    }
}
