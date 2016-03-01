using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Engine.NetWork
{
    /// <summary>
    /// 网络线程
    /// </summary>
    public abstract class BasicThread
    {
        private Thread m_thread = null;
        private bool m_terminateFlag;
        private System.Object m_terminateFlagMutex;

        private NetworkStream m_netStream;

        public BasicThread(NetworkStream stream)
        {
            m_thread = new Thread(ThreadProc);
            m_terminateFlag = false;
            m_terminateFlagMutex = new System.Object();
            m_netStream = stream;
        }
        
        protected NetworkStream NetStream
        {
            get
            {
                return m_netStream;
            }
        }

        /// <summary>
        /// 开始线程
        /// </summary>
        public void Start()
        {
            m_thread.Start(this);
        }

        protected static void ThreadProc(object obj)
        {
            BasicThread me = (BasicThread)obj;
            me.Run();
        }

        protected abstract void Run();

        public void WaitTermination()
        {
            m_thread.Join();
        }

        public void SetTerminateFlag()
        {
            lock (m_terminateFlagMutex)
            {
                m_terminateFlag = true;
            }
        }

        public bool IsTerminateFlagSet()
        {
            lock (m_terminateFlagMutex)
            {
                return m_terminateFlag;
            }
        }
    }
}
