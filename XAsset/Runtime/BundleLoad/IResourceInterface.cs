using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace XAsset.Runtime.BundleLoad
{
    public interface IResourceInterface
    {
        void Initlizate();

        UniTask<bool> InitAssetModule(string bundleModule);

        UniTask PreLoadObjAsync(string path, int count = 1);

        UniTask<GameObject> InstantiateFromPoolAsync(string path, Transform parent = null, int maxCount = 50);

        UniTask<GameObject> InstantiateAsync(string path, Transform parent = null);
        
        UniTask<T> LoadAssetAsync<T>(string path) where T : Object;
        
        UniTask LoadSceneAsync(string path, LoadSceneMode loadSceneMode = LoadSceneMode.Single);

        void Release(GameObject obj);
        

        void ClearResourcesAssets(bool absoluteCleaning);//是否深度清理
    }
}