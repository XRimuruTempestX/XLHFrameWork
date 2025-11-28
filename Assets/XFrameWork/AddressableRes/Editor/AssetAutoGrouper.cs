using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using XFrameWork.AddressableRes.Config;
using XFrameWork.AddressableRes.Editor.Config;

namespace XFrameWork.AddressableRes.Editor
{
    public class AssetAutoGrouper : OdinEditorWindow
    {
        [FolderPath] public string findPath;

        [LabelText("设置标签")] public string lable = "Game";

        [FolderPath] public string resourcesDataPath = "Assets/XFrameWork/ResourceData";

        [LabelText("需要手动加载资源的路径"), FolderPath] public List<string> loadAssetsPathList = new List<string>();

        private AddressableAssetSettings settings;


        private void Awake()
        {
            settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            findPath = ResourcePath.Instance.findPath;
            resourcesDataPath = ResourcePath.Instance.resourcesDataPath;
            loadAssetsPathList = ResourcePath.Instance.resourceLoadPathList;
        }

        [MenuItem("Tools/查找资源")]
        public static AssetAutoGrouper ShowWindow()
        {
            return GetWindow<AssetAutoGrouper>();
        }

        [Button("查找资源", ButtonSizes.Large)]
        public void FindPath()
        {
            string[] guids = AssetDatabase.FindAssets("t:Object", new string[] { findPath });
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
                entry.SetLabel(lable, true);
            }

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        [Button("生成资源SO", ButtonSizes.Large)]
        public void GenerateResourcesData()
        {
            if (loadAssetsPathList.Count == 0)
                return;

            Dictionary<string, string> assetDic = new Dictionary<string, string>();

            Dictionary<string, List<string>> scriptDic = new Dictionary<string, List<string>>();

            foreach (var assetpath in loadAssetsPathList)
            {
                string[] guids = AssetDatabase.FindAssets("t:Object", new string[] { assetpath });
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    string dirName = Path.GetDirectoryName(path);
                    if (AssetDatabase.IsValidFolder(path) || path.EndsWith(".cs"))
                        continue;
                    string key = "";
                    if (Path.GetFileNameWithoutExtension(path).Contains("Window"))
                    {
                        key = Path.GetFileNameWithoutExtension(path);
                    }
                    else
                    {
                        key = Path.GetFileName(dirName) + "_" + Path.GetFileNameWithoutExtension(path);
                    }

                    assetDic.TryAdd(key, guid);

                    if (!scriptDic.ContainsKey(Path.GetFileName(dirName) ?? string.Empty))
                    {
                        scriptDic.Add(Path.GetFileName(dirName) ?? string.Empty,
                            new List<string>() { Path.GetFileNameWithoutExtension(path) });
                    }
                    else
                    {
                        scriptDic[Path.GetFileName(dirName) ?? string.Empty]
                            .Add(Path.GetFileNameWithoutExtension(path));
                    }
                }
            }

            if (!Directory.Exists(resourcesDataPath))
            {
                Directory.CreateDirectory(resourcesDataPath);
            }

            GenerateScriptData(scriptDic);

            var so = AssetDatabase.LoadAssetAtPath<ResourceDatabase>(resourcesDataPath + "/ResourceDatabase.asset");
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<ResourceDatabase>();
                AssetDatabase.CreateAsset(so, resourcesDataPath + "/ResourceDatabase.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            so.entries.Clear();
            foreach (var item in assetDic)
            {
                ResourceDatabase.Entry entry = new ResourceDatabase.Entry();
                entry.key = item.Key;
                Type assetType = AssetDatabase.GetMainAssetTypeAtPath(item.Value);
                if (assetType == typeof(Sprite))
                {
                    entry.reference = new AssetReferenceSprite(item.Value);
                }
                else if (assetType == typeof(Texture2D))
                {
                    entry.reference = new AssetReferenceTexture2D(item.Value);
                }
                else
                {
                    entry.reference = new AssetReference(item.Value);
                }

                so.entries.Add(entry);
            }

            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();
        }

        public void GenerateScriptData(Dictionary<string, List<string>> scriptDic)
        {
            if (scriptDic == null || scriptDic.Count == 0)
                return;
            foreach (var pair in scriptDic)
            {
                string className = pair.Key;
                List<string> names = pair.Value;

                if (names == null || names.Count == 0)
                    continue;

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("public static class " + className);
                sb.AppendLine("{");

                foreach (var n in names)
                {
                    // 确保是有效的 C# 字段名
                    string fieldName = MakeValidIdentifier(n);
                    if (fieldName.Contains("Window"))
                    {
                        sb.AppendLine($"    public const string {fieldName} = \"{n}\";");
                    }
                    else
                    {
                        sb.AppendLine($"    public const string {fieldName} = \"{className}_{n}\";");
                    }
                }

                sb.AppendLine("}");

                string filePath = Path.Combine(resourcesDataPath, className + ".cs");
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                Debug.Log("生成脚本: " + filePath);
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 将名字转换为合法的 C# 标识符（避免数字开头、空格、特殊字符）
        /// </summary>
        private string MakeValidIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "_Empty";

            // 替换非法字符
            var sb = new StringBuilder();
            foreach (char c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
                else
                    sb.Append('_');
            }

            // 如果第一个字符不是字母或 _，就补个 _
            if (!char.IsLetter(sb[0]) && sb[0] != '_')
                sb.Insert(0, '_');

            return sb.ToString();
        }

        private AddressableAssetGroup CreateGroups(string groupName)
        {
            var group = settings.FindGroup(groupName);
            if (group != null)
                return group;

            group = settings.CreateGroup(groupName, false, false, false, null, typeof(BundledAssetGroupSchema),
                typeof(ContentUpdateGroupSchema));
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