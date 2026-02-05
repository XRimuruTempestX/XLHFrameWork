using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using XAsset.Config;
using XAsset.Editor.BundleBuild;

public class XAssetConfigWindow : OdinMenuEditorWindow
{

    
    [MenuItem("XLHFrameWork/框架配置面板")]
    private static void OpenWindow()
    {
        var window = GetWindow<XAssetConfigWindow>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
    }
        
        
    /*[SerializeField][InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    private BundleSettings bundleSettings;*/

    protected override void OnEnable()
    {
        /*bundleSettings = AssetDatabase.LoadAssetAtPath<BundleSettings>(
            "Assets/XLHFrameWork/XAsset/Resources/AssetsBundleSettings.asset");*/
        
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
        {
            {"XAsset", XLHDefineMacro.Instance, EditorIcons.House},
            {"XAsset/AssetBundle", BuildBundleConfigura.Instance, EditorIcons.SettingsCog},
            {"XAsset/BundleSetting",BundleSettings.Instance, EditorIcons.SettingsCog},
            {"XAsset/GeneratorModuleEnum",BundleTools.Instance, EditorIcons.SettingsCog}
        };
            
        return tree;
    }
       
}