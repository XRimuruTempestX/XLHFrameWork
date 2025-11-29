using System;
using Fantasy;
using Fantasy.Async;
using Hotfix.NetWorkManager;
using UnityEngine;
using XLHFramework.GCFrameWork.Base;
using XLHFramework.UnityDebuger;

namespace XGC.GameWorld
{
    public class RegisterMessage : IMsgBehaviour
    {
        public void OnCreate()
        {
        }

        /// <summary>
        /// 连接服务器执行注册
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public async FTask<bool> SendRegisterRequest(string userName, string password)
        {
            try
            {
                await NetWorkManager.ConnectToServer("127.0.0.1", 20000);
                return true;
            }
            catch (Exception e)
            {
                Debuger.LogError(e);
                return false;
            }
        }

        public void OnDestroy()
        {
        }
    }
}