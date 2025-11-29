using Fantasy;
using Fantasy.Async;
using Hotfix.NetWorkManager;
using UnityEngine;
using XLHFramework.GCFrameWork.Base;
using XLHFramework.GCFrameWork.World;
using XLHFramework.UnityDebuger;

namespace XGC.GameWorld
{
    public class RegisterLogic : ILogicBehaviour
    {
        private RegisterMessage reqMsg;
        public void OnCreate()
        {
            reqMsg = World.GetExitsMsgMgr<RegisterMessage>();
        }

        public async FTask SendRegisterRequest(string userName, string password)
        {
            bool isSuccessConnet = await reqMsg.SendRegisterRequest(userName, password);
            if (isSuccessConnet)
            {
                A2C_RegisterResponse registerResponse = (A2C_RegisterResponse)await NetWorkManager.SendRequest(new C2A_RegisterRequest()
                {
                    userName = userName,
                    password = password,
                });
                Debuger.LogBlue(registerResponse.ToString());
            }
        }

        public void OnDestroy()
        {
        }
    }
}