
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class LogData
{
    public string log;
    public string trace;
    public LogType type;
}

public class UnityLogHelper : MonoBehaviour
{

    private StreamWriter mStreamWriter;

    private readonly ConcurrentQueue<LogData> mConCurrentQueue = new ConcurrentQueue<LogData>();

    private readonly ManualResetEvent mManualRestEvent = new ManualResetEvent(false);
    private bool mThreadRuning = false;
    private string mNowTime { get { return DateTime.Now.ToString("yyyy:MM:dd HH:mm:ss"); } }
    public void InitLogFileModule(string savePath,string logfineName)
    {
        string logFilePath = Path.Combine(savePath,logfineName);
        Debug.Log("logFilePath:"+ logFilePath);
        mStreamWriter = new StreamWriter(logFilePath);
        Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
        mThreadRuning = true;
        Thread fileThread = new Thread(FileLogThread);
        fileThread.Start();
    }

    public void FileLogThread()
    {
        while (mThreadRuning)
        {
            mManualRestEvent.WaitOne();
            if (mStreamWriter==null)
            {
                break;
            }
            LogData data;
            while (mConCurrentQueue.Count>0&&mConCurrentQueue.TryDequeue(out data))
            {
                if (data.type==LogType.Log)
                {
                    mStreamWriter.Write("Log >>> ");
                    mStreamWriter.WriteLine(data.log);
                    mStreamWriter.WriteLine(data.trace);
                }
                else if(data.type == LogType.Warning)
                {
                    mStreamWriter.Write("Warning >>> ");
                    mStreamWriter.WriteLine(data.log);
                    mStreamWriter.WriteLine(data.trace);
                }
                else if (data.type == LogType.Error)
                {
                    mStreamWriter.Write("Error >>> ");
                    mStreamWriter.WriteLine(data.log);
                    mStreamWriter.Write('\n');
                    mStreamWriter.WriteLine(data.trace);
                }
                mStreamWriter.Write("\r\n");
            }
            mStreamWriter.Flush();
            mManualRestEvent.Reset();
            Thread.Sleep(1);
        }
    }
    public void OnApplicationQuit()
    {
        Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
        mThreadRuning = false;
        mManualRestEvent.Reset();
        mStreamWriter.Close();
        mStreamWriter = null;
    }
    private void OnLogMessageReceivedThreaded(string condition, string stackTrace, LogType type)
    {
        mConCurrentQueue.Enqueue(new LogData { log= mNowTime +" "+ condition ,trace= stackTrace ,type=type});
        mManualRestEvent.Set();

    }
}
