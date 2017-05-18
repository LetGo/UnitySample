using System;
using System.Collections.Generic;

namespace Utility
{
    public class EventEngine : Singleton<EventEngine>
    {

        public delegate void EventCallback(int nEventID,object param);

        private Dictionary<int, List<EventCallback>> m_dicEvent = new Dictionary<int, List<EventCallback>>();

        public void AddEventListener(int nEventID, EventCallback callback)
        {
            List<EventCallback> lstEvent = null;

            if (!m_dicEvent.TryGetValue(nEventID, out lstEvent))
            {
                lstEvent = new List<EventCallback>();
                m_dicEvent.Add(nEventID, lstEvent);
            }

            if (!lstEvent.Contains(callback))
            {
                lstEvent.Add(callback);
            }
        }

        public void RemoveEventListerner(int nEventID, EventCallback callback)
        {
            List<EventCallback> lstEvent = null;

            if (m_dicEvent.TryGetValue(nEventID, out lstEvent))
            {
                lstEvent.Remove(callback);
            }
        }

        public void RemoveAllListerner(int nEventID)
        {
            List<EventCallback> lstEvent = null;
            if (m_dicEvent.TryGetValue(nEventID, out lstEvent))
            {
                lstEvent.Clear();
                m_dicEvent.Remove(nEventID);
            }
        }

        public void DispatchEvent(int nEventID, object param)
        {
            List<EventCallback> lstEvent = null;
            if (m_dicEvent.TryGetValue(nEventID, out lstEvent))
            {
                for (int i = 0,imax = lstEvent.Count; i < imax; i++)
                {
                    lstEvent[i].Invoke(nEventID, param);
                }
            }
        }
    }
}
