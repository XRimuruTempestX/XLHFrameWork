using UnityEngine;
using XFrameWork.AddressableRes.Runtime;

public class TestScripts : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await AddressableLoad.Instance.InitAddressable();
        await AddressableLoad.Instance.UpdateAssets("Game");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
