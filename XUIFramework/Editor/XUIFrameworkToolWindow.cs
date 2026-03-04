using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

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

            CreateUICamera();
            CreateEventSystem();
            AssetDatabase.Refresh();
        }

        private static void CreateUICamera()
        {
            string path = "Assets/XLHFrameWork/XUIFramework/Resources/UICamera.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("UICamera 已存在");
                return;
            }
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            }
            GameObject go = new GameObject("UICamera");
            Camera cam = go.AddComponent<Camera>();

            cam.clearFlags = CameraClearFlags.Depth;
            cam.orthographic = true;
            cam.cullingMask = LayerMask.GetMask("UI");

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            Debug.Log("UICamera 已创建");
        }
        
        private static void CreateEventSystem()
        {
            string path = "Assets/XLHFrameWork/XUIFramework/Resources/EventSystem.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
            {
                Debug.Log("EventSystem 已存在");
                return;
            }
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            }
            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);

            Debug.Log("EventSystem 已创建");
        }
    }
}
