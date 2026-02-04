using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using XAsset.Config;

namespace XAsset.Editor.BundleBuild
{
    [CreateAssetMenu(fileName = "BundleToolsConfig", menuName = "XAsset/生成枚举配置路径")]
    public class BundleTools : ScriptableObject
    {
        private static BundleTools _instance;

        public static BundleTools Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<BundleTools>(XAssetPath.BundleToolsConfigPath);
                }

                return _instance; 
            }
        }

        [FolderPath] public string mBundleModuleEnumFilePath =
            "Assets/XLHFrameWork/XAsset/Config" + "/BundleModuleEnum.cs";

        // [MenuItem("XLHFrameWork/XAsset/GeneratorModuleEnum",false,1)]
        [Button("生成模块枚举类", ButtonSizes.Large)]
        public void GenerateBundleModuleEnum()
        {
            List<BundleModuleData> moduleList = BuildBundleConfigura.Instance.AssetBundleConfig;

            string namespaceName = "XLHFrameWork.XAsset.Config";
            string classname = "BundleModuleEnum";

            if (!Directory.Exists("Assets/XLHFrameWork/XAsset/Config"))
            {
                Directory.CreateDirectory("Assets/XLHFrameWork/XAsset/Config");
            }

            if (File.Exists(mBundleModuleEnumFilePath))
            {
                File.Delete(mBundleModuleEnumFilePath);
                AssetDatabase.Refresh();
            }

            var writer = File.CreateText(mBundleModuleEnumFilePath);
            writer.WriteLine("/* ----------------------------------------------");
            writer.WriteLine("/* Title:AssetBundle模块类");
            writer.WriteLine("/* Author:XLHFrameWork");
            writer.WriteLine("/* Data:" + System.DateTime.Now);
            writer.WriteLine("/* Description:  Represents each module which is used to download an load");
            writer.WriteLine("/* Modify:");
            writer.WriteLine("----------------------------------------------*/");

            writer.WriteLine($"namespace {namespaceName}");
            writer.WriteLine("{");


            writer.WriteLine("\t" + $"public enum {classname}");
            writer.WriteLine("\t" + "{");
            writer.WriteLine("\t\tNone,");

            if (moduleList != null)
            {
                for (int i = 0; i < moduleList.Count; i++)
                {
                    if (string.IsNullOrEmpty(moduleList[i].moduleName))
                    {
                        writer.WriteLine("\t\t" + moduleList[i].moduleName + ",");
                    }
                }
            }

            writer.WriteLine("\t" + "}");

            writer.WriteLine("}");

            writer.Close();

            AssetDatabase.Refresh();
        }
    }
}