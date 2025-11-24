using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace XFrameWork.AddressableRes.Runtime
{
    public class ObjectPool
    {
        private Transform _root;

        private readonly Queue<GameObject> _pool = new Queue<GameObject>();

        private readonly List<GameObject> _usedQueue = new List<GameObject>();

        private int Count => _pool.Count;

        private int maxCount;

        private bool autoExpansion = false;

        public ObjectPool(int initMaxCount = 30, Transform root = null, bool autoExpansion = false)
        {
            _root = root;
            maxCount = initMaxCount;
            this.autoExpansion = autoExpansion;
        }

        private async UniTask<GameObject> Create(string key)
        {
            GameObject obj = await ResourceSystem.Instance.LoadAssetAsync<GameObject>(key);
            obj = GameObject.Instantiate(obj);
            obj.name = key;
            
            return obj;
        }

        public async UniTask<GameObject> Get(string key)
        {
            GameObject obj = null;
            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                if (_usedQueue.Count < maxCount || (_usedQueue.Count >= maxCount && autoExpansion))
                {
                    obj = await Create(key);
                }
                else if (_usedQueue.Count >= maxCount)
                {
                    obj = _usedQueue[0];
                    _usedQueue.RemoveAt(0);
                }
            }
            _usedQueue.Add(obj);
            obj?.transform.SetParent(null);
            obj?.SetActive(true);
            return obj;
        }

        public void Release(GameObject obj)
        {
            _pool.Enqueue(obj);
            _usedQueue.Remove(obj);
            obj.SetActive(false);
            obj.transform.SetParent(_root);
        }
    }

    public class PoolManager
    {
        private static PoolManager instance;

        public static PoolManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PoolManager();
                }

                return instance;
            }
        }


        /// <summary>
        /// 池子根节点
        /// </summary>
        private GameObject root;
    }
}