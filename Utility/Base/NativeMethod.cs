using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;

namespace Utility
{
#pragma warning disable 414
    public class MonoPInvokeCallbackAttribute : System.Attribute
    {
        private Type type;
        public MonoPInvokeCallbackAttribute(Type t) { type = t; }
    }
#pragma warning restore 414

    public class NativeMethod
    {
        public delegate void PluginCallbackDelegate(int nCur, int nTotal, string strName, IntPtr param);

#if UNITY_IPHONE || UNITY_XBOX360
		public const string LibName = "__Internal";
#else
        public const string LibName = "Plugin";
#endif

        #region  支持IOS文件读写
        public static int GetFileLength_IOS(string strFileName, int nThreadIndex = 0)
        {
            FileInfo fileInfo = new FileInfo(strFileName);
            if (!fileInfo.Exists)
            {
                return 0;
            }
            return (int)fileInfo.Length;
        }


        public static int GetFileBuff_IOS(string strFileName, [MarshalAs(UnmanagedType.LPArray)] ref byte[] lpBuffer, int nStartPos, int nSize, int nThreadIndex = 0)
        {
            FileStream fs = new FileStream(strFileName, FileMode.Open, FileAccess.Read);
            fs.Seek(nStartPos, SeekOrigin.Begin);
            BinaryReader br = new BinaryReader(fs);
            lpBuffer = br.ReadBytes(nSize);
            fs.Close();
            return nSize;
        }
        #endregion 

        // 设置C++中Log文件路径
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetLogFile([MarshalAs(UnmanagedType.LPArray)] string strLogFileName);

        /**
        @brief 获取文件长度
        @param strFileName 文件名
        @param nThreadIndex 线程索引 0 主线程 1异步线程 目前只支持0,1
        */
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetFileLength([MarshalAs(UnmanagedType.LPArray)] string strFileName, int nThreadIndex = 0);


        /**
        @brief 读取文件内容
        @param strFileName 文件名
        @param lpBuffer 文件内容
        @param nStartPos 起始位置
        @param nSize 大小
        @param nThreadIndex 线程索引 0 主线程 1异步线程 目前只支持0,1
        */
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetFileBuff([MarshalAs(UnmanagedType.LPArray)] string strFileName, [MarshalAs(UnmanagedType.LPArray)] ref byte[] lpBuffer, int nStartPos, int nSize, int nThreadIndex = 0);

        /**
        @brief 解压补丁包
        @param strZipFile
        @param strDestDir 目标目录
        @param callback 回调 
        */
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int UnPackage([MarshalAs(UnmanagedType.LPArray)] string strPatchFile, [MarshalAs(UnmanagedType.LPArray)] string strDestDir, PluginCallbackDelegate callback);

        //-------------------------------------------------------------------------------------------------------
        /**
        @brief 
        @param 解压zip文件
        @param strZipFile
        @param strDestDir 目标目录
        @param callback 回调
        @param strFilter 解压筛选器 只解压指定前缀路径的文件
        @param strExclude 解压排除器 排除指定前缀路径的文件
        */
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]       
        public static extern int ExtractZip([MarshalAs(UnmanagedType.LPArray)] string strZipFile, [MarshalAs(UnmanagedType.LPArray)] string strDestDir, PluginCallbackDelegate callback, [MarshalAs(UnmanagedType.LPArray)] string strFilter, [MarshalAs(UnmanagedType.LPArray)] string strExclude);
    }
}
