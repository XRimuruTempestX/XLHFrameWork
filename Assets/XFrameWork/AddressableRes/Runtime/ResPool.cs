using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace XFrameWork.AddressableRes.Runtime
{

    public class ResPool
    {
        private static ResPool instance;
        public static ResPool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ResPool();
                }

                return instance;
            }
        }
        
        private Dictionary<string, IObjectPool<GameObject>> _poolDic = new  Dictionary<string, IObjectPool<GameObject>>();
        
        /// <summary>
        /// 缓存
        /// </summary>
        private Dictionary<string, GameObject> _cacheDic = new  Dictionary<string, GameObject>();

        /// <summary>
        /// 初始化gameobject池子
        /// </summary>
        /// <param name="key"></param>
        public async UniTask InitializePool(string key)
        {
            if(_cacheDic.ContainsKey(key))
                return;
            GameObject obj = await ResourceSystem.Instance.LoadAssetAsync<GameObject>(key);
            _cacheDic.Add(key, obj);

            var pool = new ObjectPool<GameObject>(() => GameObject.Instantiate(obj), (go) => go.SetActive(true), go => go.SetActive(false), (go) => GameObject.Destroy(go)
                ,true, 30,100);

            _poolDic.TryAdd(key, pool);
        }


        public GameObject Get(string key)
        {
            if (_poolDic.ContainsKey(key))
            {
                return _poolDic[key].Get();
            }
            Debug.LogError($"没有{key}在缓存在池中。。");
            return null;
        }

        public void Release(string key, GameObject obj)
        {
            if (_poolDic.ContainsKey(key))
            {
                _poolDic[key].Release(obj);
            }
            else
            {
                Debug.LogError($"没有{key}在缓存在池中。。");
            }
        }

        public void Clear(string key)
        {
            if (_poolDic.ContainsKey(key))
            {
                _poolDic[key].Clear();
                _poolDic.Remove(key);
                if (_cacheDic.ContainsKey(key))
                {
                    ResourceSystem.Instance.Release(key);
                    _cacheDic.Remove(key);
                }
            }
            else
            {
                Debug.LogError($"没有{key}在缓存在池中。。");

            }
        }

        public void ClearAll()
        {
            foreach (var pool in _poolDic.Values)
            {
                pool.Clear();
            }
            _poolDic.Clear();

            foreach (var key in _cacheDic.Keys)
            {
                ResourceSystem.Instance.Release(key);
            }
            _cacheDic.Clear();
        }
  
    }
}