using System;

namespace Engine.NetWork
{
    // 网络层错误定义
    public enum NetWorkError
    {
        NetWorkError_null = 0,
        NetWorkError_UnConnect,
        NetWorkError_ConnectSuccess,    // 连接成功
        NetWorkError_DisConnect,         // 连接断开
        NetWorkError_ConnectFailed,     // 连接失败，服务器关闭或者连接超时
    }

    public enum SCOKET_CONNECT_STATE
    {
        /// <summary>
        /// 未连接
        /// </summary>
        UNCONNECTED = -1,
        /// <summary>
        /// 已经连接上
        /// </summary>
        CONNECTED = 1,
        /// <summary>
        /// 连接中断
        /// </summary>
        CONNECTE_STOP = 2
    }
}
