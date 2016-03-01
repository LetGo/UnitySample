using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace Engine.NetWork
{
    class SenderThread : BasicThread
    {
        private TcpClient m_tcpClient;
        private SwitchList<NetPackageOut> m_packetList = new SwitchList<NetPackageOut>();
        // 线程事件
        private AutoResetEvent m_SendEvent = new AutoResetEvent(false);

        // 发送异常缓存队列
        private Queue<NetPackageOut> packout = new Queue<NetPackageOut>();
        private NetPackageOut packoutCmd = null;

        public SenderThread(TcpClient client, NetworkStream stream)
            : base(stream)
        {
            m_tcpClient = client;
        }

        /// <summary>
        /// 准备将数据包发送
        /// </summary>
        /// <param name="p"></param>
        public void Send(NetPackageOut p)
        {
            m_packetList.Push(p);
            m_SendEvent.Set();  //将事件状态设置为终止状态，允许一个或多个等待线程继续
        }

        /// <summary>
        /// 发包出错 进入缓存
        /// </summary>
        /// <param name="pkg"></param>
        public void EnqueErrorPack(NetPackageOut pkg)
        {
            packout.Enqueue(pkg);
        }

        /// <summary>
        /// 获取所有出错包
        /// </summary>
        /// <param name="lstPackage"></param>
        public void PopPackage(out List<NetPackageOut> lstPackage)
        {
            lstPackage = new List<NetPackageOut>();
            if (packoutCmd != null)
            {
                lstPackage.Add(packoutCmd);
            }
            if (packout != null)
            {
                while (packout.Count > 0)
                {
                    NetPackageOut pkg = packout.Dequeue();
                    if (pkg != packoutCmd)
                    {
                        lstPackage.Add(pkg);
                    }
                }
            }
        }

        protected override void Run()
        {
            while (!IsTerminateFlagSet() && m_SendEvent != null)
            {
                m_SendEvent.WaitOne();
                if(m_packetList.Switch() == false)return;

                while (m_packetList.m_OutList.Count > 0)
                {
                    NetPackageOut pkg = m_packetList.m_OutList[0];
                    //pkg.pack();
                    packoutCmd = pkg;
                    m_packetList.m_OutList.RemoveAt(0);

                    if (m_tcpClient != null && m_tcpClient.Connected)
                    {
                        try
                        {
                            // encode
                            byte[] buff = pkg.GetBuffer();

//                             if (NetWork.DEENCODE)
//                             {
//                                 // 加密编码
//                                 //Engine.Encrypt.Encode(ref buff);
//                             }

                            NetStream.Write(buff, 0, (int)pkg.Length);
                            NetStream.Flush();

                            if (m_tcpClient == null || !m_tcpClient.Connected)
                            {
                                EnqueErrorPack(pkg);
                            }
                        }
                        catch (IOException e)
                        {
                          //  Log.Error("发包{0}异常: {1}", pkg.getCode(), e.ToString());
                            EnqueErrorPack(pkg);
                        }
                    }
                    else
                    {
                        // Utility.Log.Trace("断线发包: code=" + pkg.getCode().ToString());
                        EnqueErrorPack(pkg);
                        break;
                    }
                } // end while(m_packetList.m_OutList.Count > 0)
            }//end while
        }
    }
}
