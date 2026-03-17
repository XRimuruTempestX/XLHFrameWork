using System.Collections.Generic;
using FixIntMath;

namespace XLHFrameWork.PhysicsFramework
{
    public class SAPBroadPhase
    {
        private List<CollisionObject> _sortedObjects = new List<CollisionObject>();

        public HashSet<CollisionPair> Execute(List<CollisionObject> objects)
        {
            _sortedObjects.Clear();
            _sortedObjects.AddRange(objects);
            
            // Sort by min x using FixInt comparison
            _sortedObjects.Sort((a, b) => a.Bounds.Min.x.CompareTo(b.Bounds.Min.x));

            HashSet<CollisionPair> pairs = new HashSet<CollisionPair>();

            for (int i = 0; i < _sortedObjects.Count; i++)
            {
                CollisionObject a = _sortedObjects[i];
                for (int j = i + 1; j < _sortedObjects.Count; j++)
                {
                    CollisionObject b = _sortedObjects[j];

                    // SAP Pruning on X axis
                    if (b.Bounds.Min.x > a.Bounds.Max.x)
                    {
                        break;
                    }

                    // Check full AABB overlap (Y and Z axes included)
                    if (a.Bounds.Intersects(b.Bounds))
                    {
                        pairs.Add(new CollisionPair(a.ID, b.ID));
                    }
                }
            }

            return pairs;
        }
    }
}
