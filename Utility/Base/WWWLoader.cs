using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class WWWLoader : SingletonMono<WWWLoader>
    {
        public delegate void WWWCallback(WWW www, object param = null);
        private class WWWRequest
        {
            public WWW www = null;
            public string strURL = "";
            public WWWCallback callback = null;
            public bool m_bCache = false;
            public object m_param = null;

        }
        
        bool m_bIsLoading = false;
        List<WWWRequest> m_lstRequest = new List<WWWRequest>();


        public void LoadURL(string strURL, WWWCallback callback, object param = null, bool bCache = false)
        {
            WWWRequest request = new WWWRequest();
            request.strURL = strURL;
            request.callback = callback;
            request.m_param = param;
            request.m_bCache = bCache;

            m_lstRequest.Add(request);
            CheckRequest();
        }

        void CheckRequest()
        {
            if (m_bIsLoading)
            {
                return;
            }

            while (m_lstRequest.Count > 0)
            {
                WWWRequest www = m_lstRequest[0];
                if (www != null)
                {
                    StartCoroutine(DoRequest(www));
                    break;
                }
                else
                {
                    m_lstRequest.RemoveAt(0);
                }
            }
        }

        IEnumerator DoRequest(WWWRequest request)
        {
            m_bIsLoading = true;
            request.www = new WWW(request.strURL);
            
            yield return request.www;

            OnRequestFinish(request);

        }

        void OnRequestFinish(WWWRequest request)
        {
            if (request.callback != null)
            {
                request.callback(request.www,request.m_param);
            }

            m_lstRequest.Remove(request);

            m_bIsLoading = false;

            if (!request.m_bCache)
            {
                if (request.www.assetBundle != null)
                {
                    request.www.assetBundle.Unload(false);
                    request.www.Dispose();
                    request.www = null;
                }
            }
            CheckRequest();
        }
    }
}
