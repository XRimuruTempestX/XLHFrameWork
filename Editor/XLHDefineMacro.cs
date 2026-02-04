using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using XAsset.Config;
using XAsset.Editor.BundleBuild;

[CreateAssetMenu(fileName = "XLHDefineMacro", menuName = "XLHFrameWork/宏配置")]
public class XLHDefineMacro : ScriptableObject
{
    private static XLHDefineMacro _instance;

    public static XLHDefineMacro Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = AssetDatabase.LoadAssetAtPath<XLHDefineMacro>("Packages/com.xlh.xlhpackage/Editor/XLHDefineMacro.asset");
            }

            return _instance;
        }
    }

    [ReadOnly]
    public bool isInitFrameWorlk = false;
    
    public string defineMacro = "XLHFrameWork";

    [Button("添加框架宏")]
    public void AddMacro()
    {
        var targetGroup =  EditorUserBuildSettings.selectedBuildTargetGroup; // 目标平台
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        if (!defines.Contains(defineMacro))
        {
            defines += $";{defineMacro}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
            UnityEngine.Debug.Log($"{defineMacro} Macro added!");
            isInitFrameWorlk =  true;
            BundleTools.Instance.GenerateBundleModuleEnum();
        }
    }

    [Button("删除框架宏")]
    public void RemoveMacro()
    {
        var targetGroup =  EditorUserBuildSettings.selectedBuildTargetGroup; // 目标平台
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        // 分号分割
        var defineList = new System.Collections.Generic.List<string>(defines.Split(';'));

        if (defineList.Contains(defineMacro))
        {
            defineList.Remove(defineMacro);
            string newDefines = string.Join(";", defineList);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newDefines);
            UnityEngine.Debug.Log($"{defineMacro} Macro removed!");
            isInitFrameWorlk =  false;
        }
    }

#if XLHFrameWork

    [Button("启用")]
    public void Test()
    {
        Debug.Log("启用宏定义");
    }
    
#endif
}
