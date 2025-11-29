using Fantasy.Entitas.Interface;

namespace Fantasy.Entity;

public class Account : Entitas.Entity, ISupportedDataBase
{
    public int id { get; set; }
    public string userName { get; set; }
    public string password_hash { get; set; }
    public string salt { get; set; }
    //创建时间
    public string create_time { get; set; }
    //更新时间
    public string update_time { get; set; }
    //最近登录时间
    public string last_login_time { get; set; }
    
}