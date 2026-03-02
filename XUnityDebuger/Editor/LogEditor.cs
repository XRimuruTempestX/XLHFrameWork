
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class LogEditor
{
    [MenuItem("XLHFrameWork/日志系统/开启日志")]
    public static void LoadReport()
    {
        ScriptingDefineSymbols.AddScriptingDefineSymbol("OPEN_LOG");
        GameObject reportObj = GameObject.Find("Reporter");
        if (reportObj==null)
        {
            reportObj= GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.xlh.xlhpackage/XUnityDebuger/Unity-Logs-Viewer/Reporter.prefab"));
            reportObj.name = "Reporter";
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.Refresh();
            Debug.Log("Open Log Finish!");
        }
    }
    [MenuItem("XLHFrameWork/日志系统/关闭日志")]
    public static void CloseReport()
    {
        ScriptingDefineSymbols.RemoveScriptingDefineSymbol("OPEN_LOG");
        GameObject reportObj = GameObject.Find("Reporter");
        if (reportObj!=null)
        {
            GameObject.DestroyImmediate(reportObj);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.Refresh();
            Debug.Log("Cloase Log Finish!");
        }
    }
}
