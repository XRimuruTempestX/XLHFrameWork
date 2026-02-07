using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using XAsset.Config;
using XAsset.Tools;
using Object = UnityEngine.Object;

namespace XAsset.Runtime.BundleLoad
{
    
      public class PoolData
    {
        private readonly Stack<GameObject> _inactive = new Stack<GameObject>();
        private readonly HashSet<GameObject> _active = new HashSet<GameObject>();

        private readonly int _maxCount;
        private readonly GameObject _root;
        private readonly GameObject _seed; //种子实例

        public int ActiveCount => _active.Count;
        public int InactiveCount => _inactive.Count;

        public PoolData(GameObject poolRoot, string name, GameObject prefab, int maxCOunt = 50)
        {
            _maxCount = maxCOunt;
            if (XLHResourceManager.isOpenLayout)
            {
                _root = new GameObject(name + "_Pool");
                _root.transform.SetParent(poolRoot.transform);
            }

            //只创建一次seed
            _seed = GameObject.Instantiate(prefab);
            _seed.name = name + "_Seed";
            _seed.SetActive(false);
            if (XLHResourceManager.isOpenLayout)
            {
                _seed.transform.SetParent(_root.transform);
            }
        }

        public GameObject Get()
        {
            GameObject obj;

            if (_inactive.Count > 0)
            {
                obj = _inactive.Pop();
            }
            else
            {
                obj = GameObject.Instantiate(_seed);
            }

            _active.Add(obj);
            obj.SetActive(true);
            if (XLHResourceManager.isOpenLayout)
                obj.transform.SetParent(null);

            return obj;
        }

        /// <summary>
        /// 如果是false 则是直接销毁， 如果是true 则是正常放回池子
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Release(GameObject obj)
        {
            if (!_active.Remove(obj))
            {
                Debug.LogWarning($"[PoolData] Release invalid object: {obj.name}");
                return false;
            }

            if (_inactive.Count >= _maxCount)
            {
                GameObject.Destroy(obj);
                return false;
            }

            obj.SetActive(false);
            if (XLHResourceManager.isOpenLayout)
            {
                obj.transform.SetParent(_root.transform);
            }

            _inactive.Push(obj);
            return true;
        }

        public void Clear()
        {
            foreach (var obj in _active)
            {
                GameObject.Destroy(obj);
            }

            foreach (var obj in _inactive)
            {
                GameObject.Destroy(obj);
            }

            _inactive.Clear();
            _active.Clear();
            GameObject.Destroy(_root);
            if (_root != null)
                GameObject.Destroy(_root);
        }
    }
    
    public class XLHResourceManager: IResourceInterface
    {
        /// <summary>
        /// 对象池字典
        /// </summary>
        private Dictionary<uint, PoolData> mObjectPoolDic = new Dictionary<uint, PoolData>();

        private Dictionary<uint, UniTaskCompletionSource<PoolData>> _initTasks = new Dictionary<uint, UniTaskCompletionSource<PoolData>>();

        /// <summary>
        /// 已经加载在场景中的对象  用于回收
        /// </summary>
        private Dictionary<int, uint> mAlreadyLoadGameobjectDic = new Dictionary<int, uint>();

        /// <summary>
        /// 已经加载过的资源字典  key = 资源crc , value = bundleItem 资源信息
        /// </summary>
        private Dictionary<uint, BundleItem> mAlreadyLoadAssetsDic = new Dictionary<uint, BundleItem>();


        private GameObject mRoot;

        public static bool isOpenLayout = true;


        private void Log(string msg)
        {
            //    Debug.Log("[XLHResourceManager] " + msg);
        }

        private void LogWarn(string msg)
        {
            //  Debug.LogWarning("[XLHResourceManager] " + msg);
        }

        private void LogError(string msg)
        {
            //  Debug.LogError("[XLHResourceManager] " + msg);
        }

        public void Initlizate()
        {
            mRoot = new GameObject();
            mRoot.name = "PoolRoot";
            GameObject.DontDestroyOnLoad(mRoot);
            Log("Initlizate done, pool root created name=" + mRoot.name);
        }
        
        public async UniTask<bool> InitAssetModule(string bundleModule)
        {
            Log("InitAssetModule pass-through module=" + bundleModule);
            return await AssetBundleManager.Instance.InitAssetModule(bundleModule);
        }

        public async UniTask PreLoadObjAsync(string path, int count = 1)
        {
            path = path.EndsWith(".prefab") ? path : path + ".prefab";
            uint crc = Crc32.GetCrc32(path);

            // 已有池子，直接预热
            if (mObjectPoolDic.TryGetValue(crc, out var pool))
            {
                Prewarm(pool, count);
                return;
            }

            UniTaskCompletionSource<PoolData> tcs;

            //  正在初始化，等待
            if (_initTasks.TryGetValue(crc, out tcs))
            {
                pool = await tcs.Task;
                Prewarm(pool, count);
                return;
            }

            //  第一个进入，创建初始化
            tcs = new UniTaskCompletionSource<PoolData>();
            _initTasks.Add(crc, tcs);

            try
            {
                pool = await CreatePoolAsync(crc, path, count);
                mObjectPoolDic.Add(crc, pool);

                tcs.TrySetResult(pool);

                Prewarm(pool, count);
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
                throw;
            }
            finally
            {
                _initTasks.Remove(crc);
            }
        }
        
        private void Prewarm(PoolData pool, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var obj = pool.Get();
                pool.Release(obj);
            }
        }
        
        /// <summary>
        /// 加载非对象池的预制体
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public async UniTask<GameObject> InstantiateAsync(string path, Transform parent = null)
        {
            GameObject obj = await LoadAssetAsync<GameObject>(path);
            obj = GameObject.Instantiate(obj,parent);
            return obj;
        }
        
        /// <summary>
        /// 加载并且实例化出一个对象
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public async UniTask<GameObject> InstantiateFromPoolAsync(string path, Transform parent = null, int maxCount = 50)
        {
            path = path.EndsWith(".prefab") ? path : path + ".prefab";
            uint crc = Crc32.GetCrc32(path);

            PoolData pool;

            //池子中存在
            if (mObjectPoolDic.TryGetValue(crc, out pool))
            {
                return Spawn(pool, crc, parent);
            }
            UniTaskCompletionSource<PoolData> tcs;
            //初始化中.. 等待种子实例创建
            if (_initTasks.TryGetValue(crc, out tcs))
            {
                pool = await tcs.Task;
                return Spawn(pool, crc, parent);
            }

            //初始化
            tcs = new UniTaskCompletionSource<PoolData>();
            _initTasks.TryAdd(crc, tcs);

            try
            {
                pool = await CreatePoolAsync(crc, path, maxCount);
                mObjectPoolDic.TryAdd(crc, pool);

                tcs.TrySetResult(pool);
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
                throw;
            }
            finally
            {
                _initTasks.Remove(crc);
            }

            return Spawn(pool, crc, parent);
        }
        
        private GameObject Spawn(PoolData pool, uint crc, Transform parent)
        {
            var obj = pool.Get();
            obj.transform.SetParent(parent, false); // UI / 非 UI 都更安全
            mAlreadyLoadGameobjectDic.TryAdd(obj.GetInstanceID(), crc);
            return obj;
        }
        
        /// <summary>
        /// 初始化对象池子，  每个预制体只会初始化一次
        /// </summary>
        /// <param name="crc"></param>
        /// <param name="path"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        private async UniTask<PoolData> CreatePoolAsync(uint crc, string path, int maxCount = 50)
        {
            var prefab = await LoadAssetAsync<GameObject>(path);

            var pool = new PoolData(mRoot, path, prefab, maxCount);

            mObjectPoolDic.Add(crc, pool);
            return pool;
        }
        
        /// <summary>
        /// 加载非实例化资源接口
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async UniTask<T> LoadAssetAsync<T>(string path) where T : Object
        {
            Log("LoadAssetAsync start path=" + path + ", type=" + typeof(T).Name);
            uint crc = Crc32.GetCrc32(path);
            BundleItem item = GetAlreadyLoadAssets(crc);
            if (item?.obj != null)
            {
                return item.obj as T;
            }

            T loadObj = await LoadResourceAsync<T>(path);
            return loadObj;
        }

        public async UniTask LoadSceneAsync(string path, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            path = path.EndsWith(".unity") ? path : path + ".unity";
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
#if UNITY_EDITOR
            if (BundleSettings.Instance.loadAssetType == LoadAssetEnum.Editor)
            {
                bool isContain = false;
                foreach (UnityEditor.EditorBuildSettingsScene sceneItem in UnityEditor.EditorBuildSettings.scenes)
                {
                    if (sceneItem.path.Contains(sceneName))
                    {
                        isContain = true;
                        break;
                    }
                }

                if (isContain == false)
                {
                    Debug.Log(
                        $"BuildSetting In Scene list not Find {sceneName} Scence,Plase Add {sceneName} to scene list!");
                }
                else
                {
                    await SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                    return;
                }
            }
#endif
            uint crc = Crc32.GetCrc32(path);

            BundleItem item = GetAlreadyLoadAssets(crc);
            if (item == null || item.assetBundle == null)
            {
                item = await AssetBundleManager.Instance.LoadAssetBundle(crc);
                if (item != null)
                {
                    mAlreadyLoadAssetsDic.TryAdd(crc,item);
                }
            }
            await SceneManager.LoadSceneAsync(sceneName,loadSceneMode);
        }
        
         /// <summary>
        /// 用于接在非实例化资源接口
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private async UniTask<T> LoadResourceAsync<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                LogError("path is null or empty");
                return null;
            }

            uint crc = Crc32.GetCrc32(path);
            Log("LoadResourceAsync start path=" + path + ", crc=" + crc + ", type=" + typeof(T).Name);

            BundleItem item = GetAlreadyLoadAssets(crc);
            if (item?.obj != null)
            {
                Log("BundleItem already has obj, returning cached for path=" + path);
                return (T)item.obj;
            }

            T obj = null;
#if UNITY_EDITOR
            if (BundleSettings.Instance.loadAssetType == LoadAssetEnum.Editor)
            {
                obj = LoadAssetsFromEditor<T>(path);
                if (obj == null)
                {
                    LogError("Load object is null path=" + path);
                    return null;
                }

            //    item.obj = obj;
                Log("Editor mode load success path=" + path);
                return obj;
            }
#endif
            item = await AssetBundleManager.Instance.LoadAssetBundle(crc);
            if (item.obj != null)
            {
                Log("Item.obj present after bundle load path=" + path);
                item.refCount++;
                return item.obj as T;
            }

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            T loadObj = await item.assetBundle.LoadAssetAsync<T>(item.path) as T;
            item.obj = loadObj;
            mAlreadyLoadAssetsDic.TryAdd(crc, item); //记录已经加载过这个资源进缓存
            sw.Stop();
            Log("AssetBundle LoadAssetAsync done assetPath=" + item.path + ", elapsed=" + sw.ElapsedMilliseconds +
                " ms");
            return loadObj;
        }
         
         /// <summary>
        /// 获取已经加载过的资源
        /// </summary>
        /// <param name="crc"></param>
        /// <returns></returns>
        private BundleItem GetAlreadyLoadAssets(uint crc)
        {
            return mAlreadyLoadAssetsDic.TryGetValue(crc, out BundleItem bundleItem) ? bundleItem : null;
        }


#if UNITY_EDITOR
        private T LoadAssetsFromEditor<T>(string path) where T : Object
        {
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif

        /// <summary>
        /// 释放一个实例化资源
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="destroyCache"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Release(GameObject obj)
        {
            int insId = obj.GetInstanceID();
            if (mAlreadyLoadGameobjectDic.TryGetValue(insId, out var crc))
            {
                if (mObjectPoolDic.TryGetValue(crc, out var pool))
                {
                    var result = pool.Release(obj);
                    if (!result)
                    {
                        mAlreadyLoadGameobjectDic.Remove(insId);
                    }
                }
            }
            else
            {
                Log("销毁一个非池子预制体");
                GameObject.Destroy(obj);
            }
        }

        /// <summary>
        /// 清理池子
        /// </summary>
        public void ClearPoolAll()
        {
            foreach (var pool in mObjectPoolDic.Values)
            {
                pool.Clear();
            }

            mObjectPoolDic.Clear();
            _initTasks.Clear();
            mAlreadyLoadGameobjectDic.Clear();
        }

        public void ClearResourcesAssets(bool absoluteCleaning)
        {
            ClearPoolAll();
            foreach (var item in mAlreadyLoadAssetsDic.Values)
            {
                AssetBundleManager.Instance.ReleaseAssets(item, absoluteCleaning);
            }

            //清理列表
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            Log("ClearResourcesAssets finished, absoluteCleaning=" + absoluteCleaning);
        }
    }
}