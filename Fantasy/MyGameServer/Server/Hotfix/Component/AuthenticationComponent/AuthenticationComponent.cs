using Fantasy.Entitas;
using Fantasy.Entity;

namespace Hotfix.Component.AuthenticationComponent;

public class AuthenticationComponent : Entity
{
    /// <summary>
    /// 登录缓存
    /// </summary>
    public Dictionary<string,Account> accountsCache = new Dictionary<string,Account>();
}