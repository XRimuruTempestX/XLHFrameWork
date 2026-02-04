using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace XAsset.Config
{
    [System.Serializable]
    public class BundleModuleData  
    {
        //AssetBundle模块id
        public long bundleid;
        //模块名称
        public string moduleName;
        //是否寻址资源
        public bool isAddressableAsset;
        //是否打包
        public bool isBuild;
#if UNITY_EDITOR
        //是否添加模块按钮
        [JsonIgnore]
        public bool isAddModule;
#endif
    
        //上一次点击按钮的时间
        public float lastClickBtnTime;
        
        [FolderPath]
        public List<string> prefabPathArr = new List<string>() ;

        [FolderPath]
        public List<string> rootFolderPathArr  = new List<string>();
        
        [FolderPath]
        public List<string> singleFolderPathArr  = new List<string>();
    }
    

}
