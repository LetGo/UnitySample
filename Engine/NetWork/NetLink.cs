using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Engine.NetWork
{
    /// <summary>
    /// 网络连接
    /// </summary>
    class NetLink : INetLink
    {
        private SCOKET_CONNECT_STATE socketstate = SCOKET_CONNECT_STATE.UNCONNECTED;

        private TcpClient m_tcpClient;
        private ReceiverThread m_receiver;
        private SenderThread m_sender;

        // 网络连接回调
        private INetLinkSink m_NetLinkSink = null;
        // 网络监测回调
        private INetLinkMonitor m_NetLinkMonitor = null;

        private int m_nLinkID = 0;
        private string curIP = "";
        private int curPort;

        // 掉线数据包缓存
        private List<NetPackageOut> DiaoxpackOut = null;

        private float m_nLastTestTime = 0;//测试连接时间

        public SCOKET_CONNECT_STATE ScoketState
        {
            get { return socketstate; }
        }

        public NetLink(int nLinkID, INetLinkSink linkSink, INetLinkMonitor monitor = null)
        {
            m_NetLinkSink = linkSink;
            m_NetLinkMonitor = monitor;
            m_nLinkID = nLinkID;

            //TODO
           // NetManager.Instance().NetLinkSink = linkSink;
            //NetManager.Instance().NetLinkMonitor = monitor;
        }

        #region
        public int GetLinkID()
        {
            return m_nLinkID;
        }

        public void Connect(string strServerIP, int port)
        {
            Disconnect();

            m_tcpClient = new TcpClient();
            if (strServerIP.Length < 0)
            {
                return;
            }

            //Utility.Log.Trace("Connecting " + strServerIP + ":" + port.ToString());
            curIP = strServerIP;
            curPort = port;

            m_tcpClient.BeginConnect(curIP, curPort, new System.AsyncCallback(OnConnectCallback), m_tcpClient.Client);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            if (null != m_tcpClient && m_tcpClient.Connected)
            {
                m_tcpClient.GetStream().Close();
                m_tcpClient.Close();
                m_tcpClient = null;
                this.socketstate = SCOKET_CONNECT_STATE.UNCONNECTED;
                //TODO
                //        NetManager.Instance().PushClose();
            }
            SetTerminateFlag();
        }

        public void SendMsg(NetPackageOut pkg)
        {
            if (this.IsConnected())
            {
                if (m_sender != null)
                {
                    m_sender.Send(pkg);

                    if (m_NetLinkMonitor != null)
                    {
                        m_NetLinkMonitor.OnSend(pkg);
                    }
                }
            }
            else
            {
              //  Utility.Log.Trace("已经断开连接，发包错误，包进入缓存");
                if (m_sender != null)
                {
                    m_sender.EnqueErrorPack(pkg);
                }
            }
        }

        public bool IsConnected()
        {
            if (null == m_tcpClient)
            {
                return false;
            }
            
            CheckConnnectState();

            return socketstate == SCOKET_CONNECT_STATE.CONNECTED;
        }
        #endregion

        public void Run()
        {
            if (null == m_tcpClient)
            {
                return;
            }

            float nCurTime = Time.realtimeSinceStartup;
            if (nCurTime - m_nLastTestTime > 2.0f)
            {
                TestConnect();
                m_nLastTestTime = nCurTime;
            }
                    //TODO
        //    NetManager.Instance().OnEvent();


            if (m_NetLinkSink != null)
            {
                m_NetLinkSink.OnUpdate();
            }
        }

        void CheckConnnectState()
        {
            // 另外说明：tcpc.Connected同tcpc.Client.Connected；
            // tcpc.Client.Connected只能表示Socket上次操作(send,recieve,connect等)时是否能正确连接到资源,
            // 不能用来表示Socket的实时连接状态。
            if (!m_tcpClient.Client.Connected ||
                //如果连接已关闭、重置或终止，则返回 true （更多...）     //从网络接收的、可供读取的数据的字节数。
                (m_tcpClient.Client.Poll(1000, SelectMode.SelectRead) && (m_tcpClient.Client.Available == 0)))
            {
                byte[] buff = new byte[32];
                int nRead = m_tcpClient.Client.Receive(buff);
                if (nRead == 0)
                {
                    socketstate = SCOKET_CONNECT_STATE.CONNECTE_STOP;
                }
            }
        }
        /// <summary>
        /// 连接回调
        /// </summary>
        /// <param name="ar"></param>
        private void OnConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;
                handler.EndConnect(ar);
            }
            catch (SocketException ex)
            {
                //TODO
               // Utility.Log.Error("连接服务器{0}:{1}失败:{2}", curIP, curPort, ex.ToString());
               // NetManager.Instance().PushConnectError(NetWorkError.NetWorkError_ConnectFailed);
            }
            finally
            {
                if (m_tcpClient.Connected)
                {
                    //  设置属性
                    m_tcpClient.NoDelay = true;
                    m_tcpClient.ReceiveBufferSize = 1024 * 1024;// *2;
                    m_tcpClient.ReceiveTimeout = 10000;
                    m_tcpClient.SendBufferSize = 1024 * 1024;// *2;
                    m_tcpClient.SendTimeout = 10000;
                    //Utility.Log.Trace("m_tcpClient.ReceiveBufferSize :  " + m_tcpClient.ReceiveBufferSize.ToString());
                    //Utility.Log.Trace("m_tcpClient.SendBufferSize :  " + m_tcpClient.SendBufferSize.ToString());

                    if (DiaoxpackOut != null)
                    {
                        DiaoxpackOut.Clear();
                    }

                    if (socketstate == SCOKET_CONNECT_STATE.CONNECTE_STOP)
                    {
                        if (m_sender != null)
                        {
                            m_sender.PopPackage(out DiaoxpackOut);
                          //  Utility.Log.Trace("断线重连``````````````弹出包->count=" + DiaoxpackOut.Count);
                        }

                        // 重新连接回调
                        if (m_NetLinkSink != null)
                        {
                            m_NetLinkSink.OnReConnected();
                        }
                    }

                    // 启动收包， 发包线程
                    StartSendThread();
                    StartReceiveThread();
                    socketstate = SCOKET_CONNECT_STATE.CONNECTED;
                    //TODO
                    //NetManager.Instance().PushConnectSuccess();
                }
                else
                {//TODO
                    socketstate = SCOKET_CONNECT_STATE.UNCONNECTED;
                   // NetManager.Instance().PushConnectError(NetWorkError.NetWorkError_UnConnect);
                }
            }
        }
        /// <summary>
        ///开启发送线程
        /// </summary>
        protected void StartSendThread()
        {
            SenderThread sendThread = new SenderThread(m_tcpClient, m_tcpClient.GetStream());
            if (m_sender != null)
            {
                m_sender = null;
            }

            m_sender = sendThread;
            m_sender.Start();
        }

        private void TestConnect()
        {
            if (socketstate == SCOKET_CONNECT_STATE.CONNECTED && m_tcpClient != null && (m_tcpClient.Client != null))
            {
                CheckConnnectState();
                if (socketstate == SCOKET_CONNECT_STATE.CONNECTE_STOP)
                {
                    //TODO
                    //NetManager.Instance().PushConnectError(NetWorkError.NetWorkError_DisConnect);
                }
            }

        }

        /// <summary>
        /// 启动接收线程
        /// </summary>
        protected void StartReceiveThread()
        {
            if (m_receiver != null)
            {
                m_receiver = null;
            }
            m_receiver = new ReceiverThread(m_tcpClient.GetStream());
            m_receiver.Start();
        }

        protected void SetTerminateFlag()
        {
            if (m_sender != null)
            {
                m_sender.SetTerminateFlag();
            }

            if (m_receiver != null)
            {
                m_receiver.SetTerminateFlag();
            }
        }

        protected void WaitTermination()
        {
            if (m_sender != null)
                m_sender.WaitTermination();
            if (m_receiver != null)
                m_receiver.WaitTermination();
        }

        /// <summary>
        ///重连后发送掉线前的包
        /// </summary>
        public void ReSendPackOut()
        {
            if (socketstate == SCOKET_CONNECT_STATE.CONNECTED && DiaoxpackOut != null)
            {
                //Utility.Log.Trace("断线重连``````````````发1包->count=" + DiaoxpackOut.Count);
                for (int i = 0; i < DiaoxpackOut.Count; ++i)
                {
                    int code = DiaoxpackOut[i].getCode();
                   // Utility.Log.Trace("  断线重连``````````````发包" + code);
                    SendMsg(DiaoxpackOut[i]);
                }
                m_sender.PopPackage(out DiaoxpackOut);
               // Utility.Log.Trace("断线重连``````````````发2包->count=" + DiaoxpackOut.Count);
                for (int i = 0; i < DiaoxpackOut.Count; ++i)
                {
                    int code = DiaoxpackOut[i].getCode();
                    SendMsg(DiaoxpackOut[i]);
                }

                DiaoxpackOut.Clear();
            }
        }
    }
}
