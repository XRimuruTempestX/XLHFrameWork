using UIFrameworlk;
using UnityEngine;
using XFrameWork.AddressableRes.Runtime;
using XLHFramework.UIFrameWork.Runtime.Core;

public class Main : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await AddressableLoad.Instance.InitAddressable();
        await AddressableLoad.Instance.UpdateAssets("Game");
        await ResourceSystem.Instance.InitResource();
        UIManager.Instance.Initialize();
        await UIManager.Instance.PopUpWindow<RegisterWindow>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
