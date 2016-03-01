using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.NetWork
{
    /// <summary>
    /// 网络连接回调
    /// </summary>
    public interface INetLinkSink
    {
        void OnConnectErrot(NetWorkError e);
        /// <summary>
        /// 重连
        /// </summary>
        void OnReConnected();
        /// <summary>
        /// 连接断开
        /// </summary>
        void OnDisConnect();
        /// <summary>
        /// 接受数据
        /// </summary>
        /// <param name="msg"></param>
        void OnReceive(NetPackageIn msg);

        void OnUpdate();

        void OnClose();
    }

    /// <summary>
    /// 网络监控
    /// </summary>
    public interface INetLinkMonitor
    {
        void OnReceive(NetPackageIn msg);

        void OnSend(NetPackageOut msg);
    }

    /// <summary>
    /// 网络连接接口
    /// </summary>
    public interface INetLink
    {
        // 连接ID
        int GetLinkID();

        void Connect(string strServerIP, int port);

        void Disconnect();

        bool IsConnected();

        // 发送消息
        void SendMsg(NetPackageOut pkg);
        
    }
}
