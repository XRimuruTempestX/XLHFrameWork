using UnityEngine;

namespace XAsset.Config
{
    public static class XAssetPath
    {
        /// <summary>
        /// 打包模块数据配置表路径
        /// </summary>
        public static string BuildBundleConfiguraPath = "Packages/com.xlh.xlhpackage/XAsset/Editor/BundleBuild/BuildBundleConfigura.asset";

        public static string BundleSettingsPath =
            "Packages/com.xlh.xlhpackage/XAsset/Config/AssetsBundleSettings.asset";
        
        public static string BundleToolsConfigPath =
            "Packages/com.xlh.xlhpackage/XAsset/Config/BundleToolsConfig.asset";

        public static string UIEventSystemPath = "Assets/XLHFrameWork/UIFrameWork/UIFrameWorkConfig/UIBasePrefabs/EventSystem.prefab";
        public static string UICamaeraPath = "Assets/XLHFrameWork/UIFrameWork/UIFrameWorkConfig/UIBasePrefabs/UICamera.prefab";

        public static string UIWindowPath = "Assets/XLHFrameWork/UIFrameWork/UIFrameWorkConfig/UIPathConfig/UWindowPath.asset";
    }
}
