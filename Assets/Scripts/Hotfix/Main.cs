using UIFrameworlk;
using UnityEngine;
using XFrameWork.AddressableRes.Runtime;
using XGC.GameWorld;
using XLHFramework.GCFrameWork.World;
using XLHFramework.UIFrameWork.Runtime.Core;
using XLHFramework.UnityDebuger;

public class Main : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await AddressableLoad.Instance.InitAddressable();
        await AddressableLoad.Instance.UpdateAssets("Game");
        await ResourceSystem.Instance.InitResource();
        UIManager.Instance.Initialize();
        Debuger.InitLog();
        WorldManager.CreateWorld<GameWorld>();
        await UIManager.Instance.PopUpWindow<RegisterWindow>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
