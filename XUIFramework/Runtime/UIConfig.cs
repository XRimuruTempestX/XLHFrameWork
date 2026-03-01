using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Linq;
using XAsset.Config;

namespace XUIFramework
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "XUIFramework/UIConfig")]
    public class UIConfig : ScriptableObject
    {
        
        [System.Serializable]
        public class UIInfo
        {
            public GameObject uiPrefab; // UI Prefab (Editor Only or Reference)
            public string uiName;       // UI 名称 (作为 Key)
            public string assetPath;    // 资源路径 (XAsset 路径)
            public string description;  // 描述 (可选)
        }

        [FolderPath]
        public string UISavePath;
        
        public List<UIInfo> uiList = new List<UIInfo>();
        
        // 运行时缓存字典，加速查找
        private Dictionary<string, UIInfo> _infoCache;

        /// <summary>
        /// 初始化接口
        /// </summary>
        [Button("Refresh UI Config")]
        public void Initialize()
        {
#if UNITY_EDITOR
            //先判断UISavePath路径下的所有UI预制体是否有变动，如果有变动，则更新uiList，如果没有，则不变UIConfig
            if (string.IsNullOrEmpty(UISavePath))
            {
                Debug.LogWarning("[UIConfig] No UI save path configured.");
                return;
            }

            // 临时存储旧的配置，用于保留 description 等信息
            Dictionary<string, UIInfo> oldConfigMap = new Dictionary<string, UIInfo>();
            foreach (var info in uiList)
            {
                if (!string.IsNullOrEmpty(info.uiName) && !oldConfigMap.ContainsKey(info.uiName))
                {
                    oldConfigMap.Add(info.uiName, info);
                }
            }

            // 收集新列表
            List<UIInfo> newList = new List<UIInfo>();
            string[] searchFolders = new string[] { UISavePath };
            string[] guids = AssetDatabase.FindAssets("t:Prefab", searchFolders);
            bool isDirty = false;

            // 检查数量是否一致
            if (guids.Length != uiList.Count)
            {
                isDirty = true;
            }

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab == null) continue;

                string prefabName = prefab.name;
                UIInfo newInfo = new UIInfo();
                newInfo.uiPrefab = prefab;
                newInfo.uiName = prefabName;
                newInfo.assetPath = path;
                newInfo.description = "";

                // 尝试从旧配置中恢复信息
                if (oldConfigMap.TryGetValue(prefabName, out UIInfo oldInfo))
                {
                    newInfo.description = oldInfo.description;
                    
                    // 检查路径是否有变化
                    if (oldInfo.assetPath != path)
                    {
                        isDirty = true;
                    }
                }
                else
                {
                    // 发现了新 UI
                    isDirty = true;
                }

                newList.Add(newInfo);
            }
            
            // 如果检测到没有变化，且数量一致，则直接返回
            if (!isDirty)
            {
                // 还需要检查是否有被删除的 UI (即旧列表有，新列表没有)
                // 上面的数量检查已经涵盖了大部分情况，但如果是改名的情况：旧 A -> 新 B
                // oldMap 中没有 B，isDirty = true
                // 所以逻辑是完备的
                Debug.Log("[UIConfig] No changes detected.");
                return;
            }

            uiList = newList;
            
            // 标记脏数据，确保保存
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            Debug.Log($"[UIConfig] Refreshed {uiList.Count} UI items from {UISavePath}.");
#else
            Debug.LogWarning("[UIConfig] Initialize() should only be called in Editor mode or for runtime pre-cache.");
#endif
        }

        /// <summary>
        /// 根据 UI 名称获取 UIInfo
        /// </summary>
        public UIInfo GetUIInfo(string uiName)
        {
            if (_infoCache == null)
            {
                _infoCache = new Dictionary<string, UIInfo>();
                foreach (var info in uiList)
                {
                    if (!string.IsNullOrEmpty(info.uiName))
                    {
                        if (!_infoCache.ContainsKey(info.uiName))
                        {
                            _infoCache.Add(info.uiName, info);
                        }
                        else
                        {
                            Debug.LogWarning($"[UIConfig] Duplicate UI name: {info.uiName}");
                        }
                    }
                }
            }

            if (_infoCache.TryGetValue(uiName, out UIInfo infoResult))
            {
                return infoResult;
            }

            Debug.LogError($"[UIConfig] Cannot find UI info for: {uiName}");
            return null;
        }
    }
}
