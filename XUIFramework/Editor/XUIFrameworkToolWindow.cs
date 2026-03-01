using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace XUIFramework.Editor
{
    public class XUIFrameworkToolWindow : OdinEditorWindow
    {

        [MenuItem("XLHFrameWork/UI初始化配置")]
        public static void ShowWindow()
        {
            var window = GetWindow<XUIFrameworkToolWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        }


        [Button("创建UI配置文件")]
        private static void CreateUIConfig()
        {
            string path = "Assets/XLHFrameWork/XUIFramework/Config/UIConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<UIConfig>(path);
            if (config == null)
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
                }
                    
                config = CreateInstance<UIConfig>();
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                Selection.activeObject = config;
            }
            AssetDatabase.Refresh();
        }
    }
}
