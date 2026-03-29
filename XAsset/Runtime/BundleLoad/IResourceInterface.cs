using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace XAsset.Runtime.BundleLoad
{
    public interface IResourceInterface
    {

        public void Initlizate();

        public UniTask<bool> InitAssetModule(string bundleModule);
        
        /// <summary>
        /// 预热对象池资源
        /// </summary>
        public UniTask PreLoadObjAsync(string path, int count = 1);
        
        /// <summary>
        /// 加载非对象池的预制体
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public UniTask<GameObject> InstantiateAsync(string path, Transform parent = null);
        
        
        /// <summary>
        /// 加载并且实例化出一个对象
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public UniTask<GameObject> InstantiateFromPoolAsync(string path, Transform parent = null, int maxCount = 50);


        /// <summary>
        /// 加载非实例化资源接口
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public UniTask<T> LoadAssetAsync<T>(string path) where T : Object;

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path"></param>
        /// <param name="loadSceneMode"></param>
        /// <returns></returns>
        public UniTask LoadSceneAsync(string path, LoadSceneMode loadSceneMode = LoadSceneMode.Single);

        /// <summary>
        /// 释放一个实例化资源
        /// </summary>
        /// <param name="obj"></param>
        public void Release(GameObject obj);
        
        /// <summary>
        /// 清理池子
        /// </summary>
        public void ClearPoolAll();

        public void ClearResourcesAssets(bool absoluteCleaning);//是否深度清理

    }
}