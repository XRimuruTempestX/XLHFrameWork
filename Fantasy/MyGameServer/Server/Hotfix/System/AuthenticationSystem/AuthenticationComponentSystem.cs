using Fantasy;
using Fantasy.Async;
using Fantasy.Entitas;
using Fantasy.Entity;
using Hotfix.Component.AuthenticationComponent;

namespace Hotfix.System.AuthenticationSystem;

public static class AuthenticationComponentSystem
{
    /// <summary>
    /// 注册账号
    /// </summary>
    /// <param name="self"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    public static async FTask<uint> RegisterComponent(this AuthenticationComponent self,string username,string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Log.Error("不能为空");
            return 1;
        }

        return 0;
    }
}