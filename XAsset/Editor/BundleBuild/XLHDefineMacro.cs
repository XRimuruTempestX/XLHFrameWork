using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using XAsset.Config;
using XAsset.Editor.BundleBuild;
using XAsset.Tools;

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
                _instance = AssetDatabase.LoadAssetAtPath<XLHDefineMacro>("Packages/com.xlh.xlhpackage/XAsset/Editor/BundleBuild/XLHDefineMacro.asset");
            }

            if (_instance == null)
            {

                if (!Directory.Exists("Assets/XLHFrameWork/Editor"))
                {
                    Directory.CreateDirectory("Assets/XLHFrameWork/Editor");
                }
                _instance = CreateInstance<XLHDefineMacro>();
                _instance.isInitFrameWorlk = false;
                _instance.defineMacro = "XLHFrameWork";
                AssetDatabase.CreateAsset(_instance, "Assets/XLHFrameWork/Editor/XLHDefineMacro.asset");
                AssetDatabase.SaveAssets();
                Selection.activeObject = _instance;
            }
            AssetDatabase.Refresh();
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
            AssemblyCreator.CreateAsmdef();
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

    [Button("生成模块枚举类")]
    public void Test()
    {
        BundleTools.Instance.GenerateBundleModuleEnum();
    }
}
