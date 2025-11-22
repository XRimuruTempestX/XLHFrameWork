using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using XFrameWork.AddressableRes.Config;

namespace XFrameWork.AddressableRes.Runtime
{
    public class ResourceSystem
    {
        
        private static ResourceSystem _instance;

        public static ResourceSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ResourceSystem();
                }
                return _instance;
            }
        }
        
        private ResourceDatabase _db;
        
        private Dictionary<string, AsyncOperationHandle> _cache = new Dictionary<string, AsyncOperationHandle>();

        private async UniTask InitResource()
        {
            _db = await Addressables.LoadAssetAsync<ResourceDatabase>("ResourceDatabase");
        }

        public AsyncOperationHandle<T> LoadAsync<T>(string key) where T : Object
        {
            if (_db == null)
                return default;

            var ar = _db.Get(key);
            
            if (ar == null)
            {
                Debug.LogError($"ResourceSystem: 未在 ResourceDatabase 中找到 key={key}");
                return default;
            }

            if (_cache.TryGetValue(key, out var cachedHandle))
            {
                return cachedHandle.Convert<T>();
            }

            var handle =  ar.LoadAssetAsync<T>();
            _cache.Add(key, handle);
            return handle;
        }
        
        public void Release(string key)
        {
            if (_cache.TryGetValue(key, out var handle))
            {
                Addressables.Release(handle);
                _cache.Remove(key);
            }
        }

        public void ReleaseAll()
        {
            foreach (var kv in _cache)
                Addressables.Release(kv.Value);
            _cache.Clear();
        }
    }
}
