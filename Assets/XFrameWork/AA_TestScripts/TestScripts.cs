using UnityEngine;
using UnityEngine.AddressableAssets;
using XFrameWork.AddressableRes.Runtime;

public class TestScripts : MonoBehaviour
{

    public AssetReference AssetReference;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await AddressableLoad.Instance.InitAddressable();
        await AddressableLoad.Instance.UpdateAssets("Game");
        GameObject obj =  await AssetReference.LoadAssetAsync<GameObject>().Task;
        Instantiate(obj);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
