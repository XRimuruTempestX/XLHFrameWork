using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace XFrameWork.AddressableRes.Config
{
    [CreateAssetMenu(fileName = "ResourceDatabase", menuName = "XFramework/Resource Database")]
    public class ResourceDatabase : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string key;
            public AssetReference reference;
        }

        [Searchable]
        public List<Entry> entries = new List<Entry>();
        
        private Dictionary<string, AssetReference> _map;

        private void EnsureMap()
        {
            if(_map != null)
                return;
            _map = new Dictionary<string, AssetReference>();
            foreach (var entry in entries)
            {
                if(string.IsNullOrEmpty(entry.key) ||  entry.reference == null)
                    continue;
                _map.Add(entry.key, entry.reference);
            }
        }

        public AssetReference Get(string key)
        {
            EnsureMap();
            _map.TryGetValue(key, out var r);
            return r;
        }

        public AssetReferenceT<T> GetTyped<T>(string key) where T : UnityEngine.Object
        {
            var ar = Get(key);
            if (ar == null)
                return null;
            return ar as  AssetReferenceT<T>;
        }
    }
}
