using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

//文件操作
namespace Utility
{
    public class FileUtils : Singleton<FileUtils>
    {

        // 资源路径 用户自定义路径
        public string ResourcePath
        {
            set;
            get;
        }

        private static string ApplicationStreamingPath = "";
        private static string ApplicationPersistentDataPath = "";
        private static string ApplicationTemporaryDataPath = "";
        private static bool m_bIsEditor = false;

        /// <summary>
        /// Unity支持目录类别定义
        /// </summary>
        public enum UnityPathType
        {
            UnityPath_Resources = 0,    // Unity内置路径，只支持Resource.Load方法读取 
            UnityPath_StreamAsset,      // Unity StreamAsset目录，只读目录
            UnityPath_PersistentData,   // Untity持久目录， 支持读写 文本文件或者二进制文件可以存放 IOS下该目录会自动备份到iCound
            UnityPath_TemporaryCache,   // Unity临时缓存目录 功能同 UnityPath_PersistentData IOS下不会自动备份到iCound
            UnityPath_CustomPath,       // 用户自定义路径 即ResourcePath 
        }

        public FileUtils()
        {
            if (Application.platform == RuntimePlatform.Android) /// Application.streamingAssetsPath在安卓平台下已经自带了 jar:file:///
            {
                ApplicationStreamingPath = Application.dataPath + "!/assets/";
            }
            else
            {
                ApplicationStreamingPath = Application.streamingAssetsPath + "/";
            }

            ApplicationPersistentDataPath = Application.persistentDataPath + "/";
            ApplicationTemporaryDataPath = Application.temporaryCachePath + "/";

            m_bIsEditor = Application.isEditor;
        }

        #region 文件路径合成

        /// <summary>
        /// 文件路径合成
        /// </summary>
        /// <param name="strFileName">文件名</param>
        /// <param name="pathType">目录类别</param>
        /// <returns></returns>
        public string FullPathFileName(string strFileName, UnityPathType pathType = UnityPathType.UnityPath_PersistentData)
        {
            string strFullPath = string.Empty;

            switch (pathType)
            {
                case UnityPathType.UnityPath_StreamAsset:
                    strFullPath = ApplicationStreamingPath + strFileName;
                    break;
                case UnityPathType.UnityPath_PersistentData:
                    strFullPath = ApplicationPersistentDataPath + strFileName;
                    break;
                case UnityPathType.UnityPath_TemporaryCache:
                    strFullPath = ApplicationTemporaryDataPath + strFileName;
                    break;
                case UnityPathType.UnityPath_CustomPath:
                    strFullPath = ResourcePath + strFileName;
                    break;
            }

            return strFullPath;
        }

        /// <summary>
        /// 获取文件URL路径 URL路径只能www加载
        /// </summary>
        /// <param name="strFileName"></param>
        /// <param name="pathType"></param>
        /// <returns></returns>
        public string FullUrlFileName(string strFileName, UnityPathType pathType = UnityPathType.UnityPath_StreamAsset)
        {
            string strFullUrl = "";

            switch (pathType)
            {
                case UnityPathType.UnityPath_StreamAsset:
                    strFullUrl = ApplicationStreamingPath + strFileName;
                    break;
                case UnityPathType.UnityPath_PersistentData:
                    strFullUrl = ApplicationPersistentDataPath + strFileName;
                    break;
                case UnityPathType.UnityPath_TemporaryCache:
                    strFullUrl = ApplicationTemporaryDataPath + strFileName;
                    break;
                case UnityPathType.UnityPath_CustomPath:
                    strFullUrl = ResourcePath + strFileName;
                    break;
            }

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    if (pathType == UnityPathType.UnityPath_StreamAsset)
                    {
                        strFullUrl = "jar:file://" + strFullUrl;
                    }
                    else
                    {
                        strFullUrl = "file://" + strFullUrl;
                    }
                    break;
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    {
                        strFullUrl = "file://" + strFullUrl;
                    }
                    break;
            }

            return strFullUrl;
        }

#endregion

        /************************************************************************/
        /* 计算文件md5                                                          */
        /************************************************************************/
        public string GetFileMD5(string path)
        {
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(file);
            file.Close();
            string md5str = System.BitConverter.ToString(bytes);
            md5str = md5str.Replace("-", "");
            md5str = md5str.ToLower();
            return md5str;
        }

        // 读文件 使用相对路径 先尝试外部存储卡，再尝试包内文件 需要外部先设置自定义资源路径
        public string ReadTextFile(string strFileName, int nStartPos = 0, int nSize = 0, int nThreadIndex = 0)
        {
            string strContent = "";
            GetTextFileBuff(strFileName, out strContent, nThreadIndex);
            return strContent;
        }

        // 读取文本文件 返回UTF-8编码数据
        public int GetTextFileBuff(string strFileName, out string strBuff, int nTheadIndex = 0)
        {
            byte[] buff = null;
            int nSize = GetBinaryFileBuff(strFileName, out buff, nTheadIndex);
            if (nSize == 0)
            {
                buff = null;
                strBuff = "";
                return nSize;
            }
            strBuff = System.Text.Encoding.UTF8.GetString(buff);
            buff = null;
            return nSize;
        }

        // 读取二进制数据 注意 数据使用完成需要将 buff置null 以释放内存
        // strFileName 外面读取资源时，全部使用相对路径（相对于资源目录）
        public int GetBinaryFileBuff(string strFileName, out byte[] buff, int nStartPos = 0, int nSize = 0, int nThreadIndex = 0)
        {
            string strResPath = FullPathFileName( strFileName, UnityPathType.UnityPath_CustomPath); // 资源路径
            //Log.Trace("GetBinaryFileBuff:{0}", strResPath);
            int nFileSize = 0;
            try
            {
                nFileSize = GetFileLength(strResPath, nThreadIndex);
                if (nFileSize == 0)
                {
                    strResPath = FullPathFileName( strFileName, UnityPathType.UnityPath_StreamAsset);
                    nFileSize = GetFileLength(strResPath, nThreadIndex);
                    //Log.Trace("GetBinaryFileBuff3:{0}", strResPath);
                }
            }
            catch (System.Exception ex)
            {
               // Log.Error("GetBinaryFileBuff {0} Filed! {1}", strResPath, ex);
            }

            if (nFileSize == 0)
            {
                buff = null;
              //  Engine.Utility.Log.Error("读取文件失败：{0}不存在!", strFileName);
                return 0;
            }

            if (nSize == 0)
            {
                nSize = nFileSize;
            }

            buff = new byte[nSize];

            int nRet = 0;
            try
            {
                nRet = GetFileBuff(strResPath, ref buff, nStartPos, nSize, nThreadIndex);
            }
            catch (System.Exception ex)
            {
               // Log.Error("GetBinaryFileBuff {0} Filed! {1}", strResPath, ex);
            }
            return nRet;
        }

        private int GetFileLength(string strFileName, int nThreadIndex = 0)
        {
            int nFileSize = 0;
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.IPhonePlayer) // MAC读取文件
            {
                try
                {
                    nFileSize = NativeMethod.GetFileLength_IOS(strFileName, nThreadIndex);
                }
                catch (Exception e)
                {
                   // Engine.Utility.Log.Error("C++Plugin获取文件{0}失败:{1}", strFileName, e.ToString());
                }
            }
            else
            {
                try
                {
                    nFileSize = NativeMethod.GetFileLength(strFileName, nThreadIndex);
                }
                catch (Exception e)
                {
                    //Engine.Utility.Log.Error("C++Plugin获取文件{0}失败:{1}", strFileName, e.ToString());
                }
            }

            return nFileSize;
        }

        private int GetFileBuff(string strFileName, [MarshalAs(UnmanagedType.LPArray)] ref byte[] lpBuffer, int nStartPos, int nSize, int nThreadIndex = 0)
        {
            int nRet = 0;
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.IPhonePlayer) // MAC读取文件
            {
                try
                {
                    nRet = NativeMethod.GetFileBuff_IOS(strFileName, ref lpBuffer, nStartPos, nSize, nThreadIndex);
                }
                catch (Exception e)
                {
                 //   Engine.Utility.Log.Error("C++Plugin读取文件{0}失败:{1}", strFileName, e.ToString());
                }
            }
            else
            {
                try
                {
                    nRet = NativeMethod.GetFileBuff(strFileName, ref lpBuffer, nStartPos, nSize, nThreadIndex);
                }
                catch (Exception e)
                {
                    //Engine.Utility.Log.Error("C++Plugin读取文件{0}失败:{1}", strFileName, e.ToString());
                }

            }

            return nRet;
        }
    }
}
