using UnityEngine;
using XUIFramework;
using Cysharp.Threading.Tasks;

public class TestFramework : MonoBehaviour
{
    private void Start()
    {
        // 确保 UIManager 已经初始化 (单例自动创建)
        // 测试打开
        //TestOpen().Forget();

        UIManager.Instance.OnInit();

    }

    private async UniTaskVoid TestOpen()
    {
        Debug.Log("[Test] Opening TestPanel...");
        
        // 注意：TestPanel 的 Prefab 名字必须为 "TestPanel" (与类名一致，因为 UIManager 默认用 uiName 查找)
        // 且必须已在 UIConfig 中配置
        var panel = await UIManager.Instance.OpenWindowAsync<TestPanel>("TestPanel", "Hello UI");
        
        if (panel != null)
        {
            Debug.Log($"[Test] Panel Opened! IsVisible: {panel.IsVisible}");
        }
        else
        {
            Debug.LogError("[Test] Failed to open panel. Check UIConfig path and Prefab name.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("[Test] Closing TestPanel...");
         //   UIManager.Instance.CloseWindow("TestPanel").Forget();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("[Test] Opening TestPanel...");
            UIManager.Instance.OpenWindowAsync<TestPanel>("TestPanel").Forget();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("[Test] Destroying TestPanel...");
            UIManager.Instance.DestroyWindow<TestPanel>();
        }
    }
}
