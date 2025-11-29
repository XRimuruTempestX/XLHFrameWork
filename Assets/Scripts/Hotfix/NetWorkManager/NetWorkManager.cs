using System;
using Fantasy;
using Fantasy.Async;
using Fantasy.Network.Interface;
using JetBrains.Annotations;

namespace Hotfix.NetWorkManager
{
    public class NetWorkManager
    {
        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="remoteIP">服务器 IP 地址</param>
        /// <param name="remotePort">服务器端口号</param>
        /// <param name="protocol">网络协议类型</param>
        /// <param name="isHttps">是否启用 HTTPS (仅 WebSocket 有效)</param>
        /// <param name="connectTimeout">连接超时时间 (单位: 毫秒)</param>
        /// <param name="enableHeartbeat">是否启用心跳组件</param>
        /// <param name="heartbeatInterval">心跳请求发送间隔 (单位: 毫秒)</param>
        /// <param name="heartbeatTimeOut">通信超时时间,超过此时间将断开连接 (单位: 毫秒)</param>
        /// <param name="heartbeatTimeOutInterval">检测连接超时的频率 (单位: 毫秒)</param>
        /// <param name="maxPingSamples">Ping 包的采样数量,用于计算平均延迟</param>
        /// <param name="onConnectComplete">连接成功时触发</param>
        /// <param name="onConnectFail">连接失败时触发</param>
        /// <param name="onConnectDisconnect">连接断开时触发</param>
        public static async FTask ConnectToServer(string remoteIP,
            int remotePort,
            FantasyRuntime.NetworkProtocolType protocol = FantasyRuntime.NetworkProtocolType.KCP,
            bool isHttps = false,
            int connectTimeout = 5000,
            bool enableHeartbeat = true,
            int heartbeatInterval = 2000,
            int heartbeatTimeOut = 30000,
            int heartbeatTimeOutInterval = 5000,
            int maxPingSamples = 4,
            [CanBeNull] Action onConnectComplete = null,
            [CanBeNull] Action onConnectFail = null,
            [CanBeNull] Action onConnectDisconnect = null)
        {
            await Runtime.Connect(remoteIP, remotePort, protocol, isHttps, connectTimeout, enableHeartbeat,heartbeatInterval
                , heartbeatTimeOut, heartbeatTimeOutInterval
                ,maxPingSamples, () =>
                {
                    OnConnectComplete();
                    onConnectComplete?.Invoke();
                } , () =>
                {
                    OnConnectFail();
                    onConnectFail?.Invoke();
                }, () =>
                {
                    OnConnectDisconnect();
                    onConnectDisconnect?.Invoke();
                });
        }

        public static async FTask<IResponse> SendRequest(IRequest request)
        {
            IResponse response = await Runtime.Session.Call(request);
            return response;
        }
        
        private static void OnConnectComplete()
        {
            Log.Info("连接成功回调");
        }

        private static void OnConnectFail()
        {
            Log.Error("连接失败回调");
        }

        private static void OnConnectDisconnect()
        {
            Log.Warning("连接断开回调");
        }

        private static void OnDestroy()
        {
            // 清理资源
            Runtime.OnDestroy();
        }
    }
}