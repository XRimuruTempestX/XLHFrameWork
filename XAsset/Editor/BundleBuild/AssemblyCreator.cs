using System.IO;
using UnityEditor;
using UnityEngine;

namespace XAsset.Tools
{
    public class AssemblyCreator
    {
        public static void CreateAsmdef()
        {
            string folderPath = "Assets/XLHFrameWork/XAsset/Config";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            string asmdefPath = Path.Combine(folderPath, "XLHFrameWork.Config.asmdef");

            if (File.Exists(asmdefPath))
            {
                return;
            }

            // asmdef 的内容是 JSON 格式
            string asmdefContent = @"{
    ""name"": ""XLHFrameWork.Config"",
    ""references"": [],
    ""optionalUnityReferences"": [],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";

            File.WriteAllText(asmdefPath, asmdefContent);
            AssetDatabase.Refresh();

            Debug.Log("AssemblyDefinition created at: " + asmdefPath);
        }
    }
}