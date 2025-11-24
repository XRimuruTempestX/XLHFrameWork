using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace XFrameWork.AddressableRes.Editor.Config
{
    [CreateAssetMenu(fileName = "ResourcePath", menuName = "XFramework/ResourcePath")]
    public class ResourcePath : ScriptableObject
    {

        private static ResourcePath instance;

        public static ResourcePath Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = AssetDatabase.LoadAssetAtPath<ResourcePath>("Assets/XFrameWork/AddressableRes/Editor/Config/ResourcePath.asset");
                }
                return instance;
            }
        }
    
        [FolderPath,LabelText("addressable资源根目录")]
        public string findPath;

        [FolderPath,LabelText("资源数据放置目录")]
        public string resourcesDataPath;
    
        [FolderPath,LabelText("资源加载的根目录"),Tooltip("该目录下的所有文件均可被加载")]
        public List<string> resourceLoadPathList = new List<string>();
    }
}
