using Fantasy;
using Fantasy.Async;
using Fantasy.Network;
using Fantasy.Network.Interface;

namespace Hotfix.Handle;

public class RegisterHandle : MessageRPC<C2A_RegisterRequest,A2C_RegisterResponse>
{
    protected override async FTask Run(Session session, C2A_RegisterRequest request, A2C_RegisterResponse response, Action reply)
    {
        
        Log.Debug(request.userName);
        Log.Debug(request.password);

        await FTask.CompletedTask;
    }
}