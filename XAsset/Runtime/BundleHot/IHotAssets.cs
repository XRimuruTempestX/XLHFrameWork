using System;
using Cysharp.Threading.Tasks;

namespace XAsset.Runtime.BundleHot
{
    public interface IHotAssets
    {
        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="bundleModule"></param>
        /// <param name="startHotCallBack"></param>
        /// <param name="hotFinishCallBack"></param>
        /// <param name="waiteDownloadCallBack"></param>
        /// <param name="isCheckAssetsVersion"></param>
        /// <returns></returns>
        UniTask StartHotAsset(string bundleModule, Action<string> startHotCallBack,
            Action<string> waiteDownloadCallBack,Action<HotFileInfo> onDownLoadSuccess,
            Action<HotFileInfo> onDownLoadFailed,
            Action<HotAssetsModule> onDownLoadFinish,bool isCheckAssetsVersion = true);
        
        /// <summary>
        /// 检测版本是否需要热更  return 是否热更结果，热更大小
        /// </summary>
        /// <param name="bundleModule"></param>
        /// <returns></returns>
        UniTask<(bool,float)> CheckAssetsVersion(string bundleModule);
        
    }
}