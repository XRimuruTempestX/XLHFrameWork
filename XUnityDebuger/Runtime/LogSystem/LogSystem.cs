
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogSystem : MonoBehaviour
{
    void Awake()
    {
        InitLogSystem();
    }
    public static void InitLogSystem()
    {
#if OPEN_LOG  
        Debuger.InitLog(new LogConfig
        {
            openLog = true,
            openTime = true,
            showThreadID = true,
            showColorName = true,
            logSave = true,
            showFPS = true,
        });
        Debuger.Log("Log");
        Debuger.LogWarning("LogWarning");
        Debuger.LogError("LogError");
        Debuger.ColorLog(LogColor.Red, "ColorLog");
        Debuger.LogGreen("LogGreen");
        Debuger.LogYellow("LogYellow");
#else
        Debug.unityLogger.logEnabled = false;
#endif
    }

}
