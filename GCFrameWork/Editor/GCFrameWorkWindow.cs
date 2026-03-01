using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using XLHFramework.GCFrameWorlk.Editor;

namespace XLHFrameWork.GCFrameWork.Editor
{
    public class GCFrameWorkWindow : OdinEditorWindow
    {
        [SerializeField][InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
        public WorldConfig worldConfig;
        
        [MenuItem("XLHFrameWork/GCFrameWork初始化")]
        public static void ShowWindow()
        {
            GCFrameWorkWindow window = GetWindow<GCFrameWorkWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            string path = "Assets/XLHFrameWork/GCFrameWork/Editor/WorldConfig.asset";
            worldConfig = AssetDatabase.LoadAssetAtPath<WorldConfig>(path);
            if (worldConfig == null)
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
                }
                    
                worldConfig = CreateInstance<WorldConfig>();
                AssetDatabase.CreateAsset(worldConfig, path);
                AssetDatabase.SaveAssets();
                Selection.activeObject = worldConfig;
            }
            AssetDatabase.Refresh();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EditorUtility.SetDirty(worldConfig);
            AssetDatabase.SaveAssets();
        }
    }
}
