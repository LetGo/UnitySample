using System;
using System.Collections.Generic;

namespace Engine.NetWork
{
    class SwitchList<T>
    {
        public List<T> m_InList = null;    //写入队列
        public List<T> m_OutList = null;   //写出队列

        private object m_lockObj = null;    //锁对象

        public SwitchList()
        {
            m_InList = new List<T>();
            m_OutList = new List<T>();

            m_lockObj = new object();
        }

        public void Push(T cmd)
        {
            if (cmd == null)
            {
                return;
            }

            lock (m_lockObj)
            {
                m_InList.Add(cmd);
            }
        }

        public bool Switch()
        {
            lock (m_lockObj)
            {
                if (m_OutList.Count > 0)
                {
                    return false;
                }

                List<T> tempList = m_InList;
                m_InList = m_OutList;
                m_OutList = tempList;
                return true;
            }
        }

    }
}
