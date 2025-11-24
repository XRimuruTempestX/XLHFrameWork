using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using XFrameWork.AddressableRes.Runtime;

public class TestScripts : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Image image;
    async void Start()
    {
        await AddressableLoad.Instance.InitAddressable();
        await AddressableLoad.Instance.UpdateAssets("Game");
        await ResourceSystem.Instance.InitResource();

        GameObject obj = await ResourceSystem.Instance.LoadAssetAsync<GameObject>(Test2.Cube__1_);
        Instantiate(obj);

        image.sprite = await ResourceSystem.Instance.LoadAssetAsync<Sprite>(TestArt.icon_bg3);

        await ResPool.Instance.InitializePool(Test2.Cube__1_);

        GameObject result =  ResPool.Instance.Get(Test2.Cube__1_);
        result.transform.position = Vector3.up;
    }


    // Update is called once per frame
    void Update()
    {
    }
}