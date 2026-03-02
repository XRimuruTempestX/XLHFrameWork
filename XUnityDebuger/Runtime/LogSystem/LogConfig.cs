
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogConfig  
{

    public bool openLog = true;

    public string logHeadFix = "###";

    public bool openTime = true;

    public bool showThreadID = true;

    public bool logSave = true;

    public bool showFPS = true;

    public bool showColorName = true;

    public string logFileSavePath { get { return Application.persistentDataPath + "/"; } }

    public string logFileName { get { return Application.productName + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm")+".log"; } }
}
