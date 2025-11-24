using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
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
        
        private Dictionary<string, AsyncOperationHandle> _sceneCache = new Dictionary<string, AsyncOperationHandle>();

        public async UniTask InitResource()
        {
            _db = await Addressables.LoadAssetAsync<ResourceDatabase>("ResourceDatabase");
        }

        /// <summary>
        /// 加载无需实例化资源
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async UniTask<T> LoadAssetAsync<T>(string key) where T : Object
        {
            if (_db == null)
            {
                await InitResource();
            }

            var ar = _db.Get(key);
            if (ar == null)
            {
                Debug.LogError($"ResourceSystem: 未在 ResourceDatabase 中找到 key={key}");
                return null;
            }

            AsyncOperationHandle handle;

            if (_cache.TryGetValue(key, out handle))
            {
                var typed = handle.Convert<T>();

                if (typed.IsDone)
                    return typed.Result;

                return await typed.ToUniTask();
            }

            var loadHandle = ar.LoadAssetAsync<T>();

            _cache[key] = loadHandle;

            return await loadHandle.ToUniTask();
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="key"></param>
        public async UniTask LoadSceneAsync(string key , LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (_db == null)
                await InitResource();

            var ar = _db.Get(key);
            if (ar == null)
            {
                Debug.LogError($"ResourceSystem: 未在 ResourceDatabase 中找到 key={key}");
                return ;
            }

            AsyncOperationHandle handle;
            SceneInstance scene;

            if (_sceneCache.TryGetValue(key, out handle))
            {
                var typed = handle.Convert<SceneInstance>();
                
                scene = await typed.ToUniTask();
                
                Debug.Log($"{scene.Scene.name} 加载完成");
                
                return;
            }

            var loadHandle = ar.LoadSceneAsync();

            _sceneCache[key] = loadHandle;

            scene = await loadHandle.ToUniTask();
            
            Debug.Log($"{scene.Scene.name} 加载完成");
        }

        /// <summary>
        /// 释放单个资源
        /// </summary>
        /// <param name="key"></param>
        public void Release(string key)
        {
            if (_cache.TryGetValue(key, out var handle))
            {
                Addressables.Release(handle);
                _cache.Remove(key);
            }
        }
        
        /// <summary>
        /// 释放场景
        /// </summary>
        /// <param name="key"></param>
        public async UniTask ReleaseScene(string key)
        {
            if (_sceneCache.TryGetValue(key, out var handle))
            {
                await Addressables.UnloadSceneAsync(handle);
                _sceneCache.Remove(key);
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
