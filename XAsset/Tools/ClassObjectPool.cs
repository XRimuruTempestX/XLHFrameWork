using System.Collections.Generic;
using UnityEngine;

namespace XAsset.Tools
{
    public class ClassObjectPool<T> where T : class,new()
    {
        protected Stack<T> mPool = new Stack<T>();

        /// <summary>
        /// 最大缓存个数
        /// </summary>
        protected int mMaxCount = 0;
        
        public int PoolCount {get { return mPool.Count; }}

        public ClassObjectPool(int maxCount = 30)
        {
            mMaxCount = maxCount;
        }

        /// <summary>
        /// 取出对象
        /// </summary>
        /// <returns></returns>
        public T Spawn()
        {
            if (mPool.Count > 0)
            {
                return mPool.Pop();
            }
            else
            {
                return new T();
            }
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        /// <param name="obj"></param>
        public void Recycle(T obj)
        {
            if (obj == null)
            {
                Debug.LogError("Recycle object is null");
                return;
            }
            mPool.Push(obj);
        }
    }
}