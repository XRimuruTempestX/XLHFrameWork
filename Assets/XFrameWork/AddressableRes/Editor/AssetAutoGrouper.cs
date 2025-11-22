using System;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace XFrameWork.AddressableRes.Editor
{
    public class AssetAutoGrouper : OdinEditorWindow
    {
        [FolderPath]
        public string findPath;

        [LabelText("设置标签")]
        public string lable = "Game";
        
        private AddressableAssetSettings settings;


        private void Awake()
        {
            settings =  AddressableAssetSettingsDefaultObject.GetSettings(true);
        }

        [MenuItem("Tools/查找资源")]
        public static AssetAutoGrouper ShowWindow()
        {
            return GetWindow<AssetAutoGrouper>();
        }

        [Button("查找资源", ButtonSizes.Large)]
        public void FindPath()
        {
            string[] guids = AssetDatabase.FindAssets("t:Object",new string[]{findPath});
            Debug.Log(findPath);
            Debug.Log(guids.Length);
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string dirName = Path.GetDirectoryName(path);
                if (AssetDatabase.IsValidFolder(path) || path.EndsWith(".cs"))
                    continue;
                var group = CreateGroups(Path.GetFileName(dirName));
                //Debug.Log(Path.GetFileName(dirName));
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
                entry.SetAddress(Path.GetFileNameWithoutExtension(path));
                entry.SetLabel(lable,true);
            }
            
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        private AddressableAssetGroup CreateGroups(string groupName)
        {
            
            var group = settings.FindGroup(groupName);
            if(group != null)
                return group;

            group = settings.CreateGroup(groupName, false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            var bundled = group.GetSchema<BundledAssetGroupSchema>();
            if (bundled != null)
            {
                bundled.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
                bundled.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
            }
            return group;
        }
        
        
    }
}
