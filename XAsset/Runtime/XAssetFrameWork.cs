using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using XAsset.Runtime.BundleHot;
using XAsset.Runtime.BundleLoad;
using XAsset.Tools;
using Object = UnityEngine.Object;

namespace XAsset.Runtime
{
    public class XAssetFrameWork  : Singleton<XAssetFrameWork>
    {
        private IHotAssets mHotAssets = null;

        private IResourceInterface mResourceMgr = null;
        
        private IDecompressAssets mDecompressAssets = null;


        private void Initialize()
        {
            mHotAssets = new HotAssetsManager();
            mResourceMgr = new XLHResourceManager();
            mDecompressAssets = new AssetsDecompressManager();
            mResourceMgr.Initlizate();
        }

        public async UniTask InitlizateResAsync(string bundleModule)
        {
            if (mHotAssets == null || mResourceMgr == null || mDecompressAssets == null)
            {
                Initialize();
            }
            await mResourceMgr.InitAssetModule(bundleModule);
        }

        /// <summary>
        /// 热更模块  ----->>>>>>>  游戏初始化调用
        /// </summary>
        /// <param name="bundleModule"></param>
        /// <param name="startHotCallBack"></param>
        /// <param name="waiteDownloadCallBack"></param>
        /// <param name="onDownLoadSuccess"></param>
        /// <param name="onDownLoadFailed"></param>
        /// <param name="onDownLoadFinish"></param>
        /// <param name="isCheckAssetsVersion"></param>
        public async UniTask StartHotAsset(string bundleModule, Action<string> startHotCallBack,
            Action<string> waiteDownloadCallBack,
            Action<HotFileInfo> onDownLoadSuccess, Action<HotFileInfo> onDownLoadFailed,
            Action<HotAssetsModule> onDownLoadFinish, bool isCheckAssetsVersion = true)
        {
            if (mHotAssets == null || mResourceMgr == null || mDecompressAssets == null)
            {
                Initialize();
            }

            mHotAssets?.StartHotAsset(bundleModule, startHotCallBack, waiteDownloadCallBack,onDownLoadSuccess,onDownLoadFailed,onDownLoadFinish,isCheckAssetsVersion);
           // await InitlizateResAsync(bundleModule);
        }
        
        /// <summary>
        /// 解压游戏资源
        /// </summary>
        /// <param name="bundleModule"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        
        public IDecompressAssets StartDeCompressBuiltinFile(string bundleModule, Action callBack)
        {
            
            if (mHotAssets == null || mResourceMgr == null || mDecompressAssets == null)
            {
                Initialize();
            }
            
            return Instance.mDecompressAssets.StartDeCompressBuiltinFile(bundleModule, callBack);
        }

        /// <summary>
        /// 加载并实例化一个GameObject 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public async UniTask<GameObject> InstantiateAsync(string path,Transform parent = null)
        {
            return await mResourceMgr.InstantiateAsync(path,parent);
        }

        /// <summary>
        /// 从缓存池中加载
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        public async UniTask<GameObject> InstantiateFromPoolAsync(string path, Transform parent = null,int maxCount = 50)
        {
            return await mResourceMgr.InstantiateFromPoolAsync(path, parent,maxCount);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path"></param>
        /// <param name="loadSceneMode"></param>
        public async UniTask LoadSceneAsync(string path, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            await mResourceMgr.LoadSceneAsync(path, loadSceneMode);
        }

        /// <summary>
        /// 加载非实例化资源
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async UniTask<T> LoadAssetAsync<T>(string path)  where T : Object
        {
            return await mResourceMgr.LoadAssetAsync<T>(path);
        }

        /// <summary>
        /// 实例化资源统一释放接口
        /// </summary>
        /// <param name="obj"></param>
        public void ReleaseGameObject(GameObject obj )
        {
            mResourceMgr.Release(obj);
        }

        /// <summary>
        /// 清空所有加载的资源   
        /// </summary>
        /// <param name="isClearAll">false 不会释放已经加载的资源，  true 释放所有由框架加载的资源</param>
        public void ReleaseAllAssets(bool isClearAll = false)
        {
            mResourceMgr.ClearResourcesAssets(isClearAll);
        }
    }
}