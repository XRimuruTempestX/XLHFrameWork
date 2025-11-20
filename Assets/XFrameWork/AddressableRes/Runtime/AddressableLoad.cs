using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace XFrameWork.AddressableRes.Runtime
{
    public class AddressableLoad
    {
        private static AddressableLoad instance;

        public static AddressableLoad Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AddressableLoad();
                }
                return instance;
            }
        }

        /// <summary>
        /// 初始化addressable
        /// </summary>
        /// <param name="autoReleaseHandle"></param>
        public async UniTask InitAddressable(bool autoReleaseHandle = true)
        {
            if (autoReleaseHandle)
            {
                await Addressables.InitializeAsync(autoReleaseHandle);
            }
            else
            {
                var handle = Addressables.InitializeAsync(false);
                var locator = await handle.Task;
                foreach (var key in locator.Keys)
                {
                    if (locator.Locate(key, typeof(object), out var locations))
                    {
                        foreach (var loc in locations)
                        {
                            Debug.Log($"Key: {key}");
                            Debug.Log($"  PrimaryKey: {loc.PrimaryKey}");
                            Debug.Log($"  InternalId: {loc.InternalId}");
                            Debug.Log($"  ProviderId: {loc.ProviderId}");

                            if (loc.Dependencies != null)
                            {
                                foreach (var dep in loc.Dependencies)
                                {
                                    Debug.Log($"    Dep: {dep.PrimaryKey}");
                                }
                            }
                        }
                    }
                }
                handle.Release();
                
            }
        }

        /// <summary>
        /// 检查目录更新
        /// </summary>
        public async UniTask UpdateAssets(string lableKey)
        {
            //更新目录
            var catalogsToUpdate = await Addressables.CheckForCatalogUpdates();
            if (catalogsToUpdate.Count > 0)
            {
                Debug.Log("需要更新目录");
                await Addressables.UpdateCatalogs(catalogsToUpdate);
            }
            
            long size = await Addressables.GetDownloadSizeAsync(lableKey);
            
            float sizeMB = size / 1024f / 1024f;
            Debug.Log($"需要更新size : {sizeMB:F2} MB");

            if (size > 0)
            {
                var handle = Addressables.DownloadDependenciesAsync(lableKey);

                while (!handle.IsDone)
                {
                    float downloadedMB  = handle.PercentComplete * sizeMB;
                    Debug.Log($"已下载: {downloadedMB:F2} / {sizeMB:F2} MB");
                    await UniTask.Yield();
                }

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("下载完成");
                }
                else
                {
                    Debug.LogError($"下载失败{handle.OperationException}");
                }
                
                handle.Release();
            }
            else
            {
                Debug.Log("资源已全部下载，无需更新");
                Debug.Log(Application.persistentDataPath);
            }
            
            Debug.Log("当前缓存路径: " + UnityEngine.Caching.currentCacheForWriting.path);
            
        }
    }
}
