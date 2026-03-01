using UnityEngine;
using XUIFramework;
using Cysharp.Threading.Tasks;

public class TestPanel : XUIBase
{
    protected override void OnInit()
    {
        base.OnInit();
        Debug.Log($"[TestPanel] OnInit: {Name}");
    }

    public override async UniTask OnOpen(params object[] args)
    {
        await base.OnOpen(args);
        Debug.Log($"[TestPanel] OnOpen. Args: {(args != null && args.Length > 0 ? args[0] : "null")}");
    }

    public override async UniTask OnClose()
    {
        Debug.Log("[TestPanel] OnClose");
        await base.OnClose();
    }
}
