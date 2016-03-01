using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Internal;

namespace Engine.Utility
{

    public enum LogLevel
    {
        /// 输出信息
        LogLevel_Info = 1,
        /// 错误信息
        LogLevel_Error = 2,
        /// 警告信息
        LogLevel_Warning = 3,
        /// 跟踪信息
        LogLevel_Trace = 4,
    };

    public class Log
    {
        //日志是否重新创建
        private static bool m_IsOpen = false;
        private static Queue<string> m_LogQueue;

        // 是否打开Log
        static public bool EnableLog = true;
        // 允许输出的最大Log等级
        static public LogLevel MaxLogLevel = LogLevel.LogLevel_Trace;
        // 是否允许写log文件
        static public bool m_bWriteLogFile = false;

        static private StreamWriter m_LogFile = null;

        static public List<string> m_LogError = null;

        /// <summary>
        /// 开始记录
        /// </summary>
        public static void Open(LogLevel maxLevel, bool bWriteLogFile)
        {
            if (m_IsOpen)
            {
                return;
            }

            m_IsOpen = true;

            // 插件Log目录
            //EngineNativeMethod.SetLogFile(Application.persistentDataPath + "/log.log");

            // Editor模式下写在各项目自己的目录 多开时不会影响
            string strOutPath = "";
            if( Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer )
            {
                strOutPath = Application.persistentDataPath + "/Star_Age_Log.txt";
            }
            else
            {
                strOutPath = Application.dataPath + "/Star_Age_Log.txt";
            }

            MaxLogLevel = maxLevel;
            m_bWriteLogFile = bWriteLogFile;
            m_LogFile = new StreamWriter(strOutPath, true, Encoding.UTF8);

            // 注册系统Log回调
            Application.logMessageReceived += HandleLog;
        }

        /// <summary>
        /// 关闭记录
        /// </summary>
        public static void Close()
        {
            Application.logMessageReceived -= HandleLog;

            if (m_LogFile != null)
            {
                m_LogFile.Close();
            }
        }

        /// <summary>
        /// 读取缓存的Log最多100条
        /// </summary>
        public static void ReadFromStack(ref List<string> OutList)
        {
            if (m_LogQueue != null)
            {
                string[] Arr = m_LogQueue.ToArray();
                for (int i = 0; i < Arr.Length; ++i)
                {
                    OutList.Add(Arr[i]);
                }
            }
        }

        static public void Info(string strFormat, params object[] args)
        {
            if (!EnableLog)
            {
                return;
            }

            string strFileLine = "";
            StackTrace insStackTrace = new StackTrace(true);
            if (insStackTrace != null)
            {
                StackFrame insStackFrame = insStackTrace.GetFrame(1);
                if (insStackFrame != null)
                {
                    strFileLine = String.Format("{0}({1})", insStackFrame.GetFileName(), insStackFrame.GetFileLineNumber());
                    strFileLine = GetScriptFileName(strFileLine);
                }
            }
            LogOutput(LogLevel.LogLevel_Info, strFileLine, strFormat, args);
        }
        static public void Error(string strFormat, params object[] args)
        {
            if (!EnableLog)
            {
                return;
            }

            string strFileLine = "";
            StackTrace insStackTrace = new StackTrace(true);
            if (insStackTrace != null)
            {
                StackFrame insStackFrame = insStackTrace.GetFrame(1);
                if (insStackFrame != null)
                {
                    strFileLine = String.Format("{0}({1})", insStackFrame.GetFileName(), insStackFrame.GetFileLineNumber());
                    strFileLine = GetScriptFileName(strFileLine);
                }
            }
            LogOutput(LogLevel.LogLevel_Error, strFileLine, strFormat, args);
        }
        static public void Warning(string strFormat, params object[] args)
        {
            if (!EnableLog)
            {
                return;
            }

            string strFileLine = "";
            StackTrace insStackTrace = new StackTrace(true);
            if (insStackTrace != null)
            {
                StackFrame insStackFrame = insStackTrace.GetFrame(1);
                if (insStackFrame != null)
                {
                    strFileLine = String.Format("{0}({1})", insStackFrame.GetFileName(), insStackFrame.GetFileLineNumber());
                    strFileLine = GetScriptFileName(strFileLine);
                }
            }
            LogOutput(LogLevel.LogLevel_Warning, strFileLine, strFormat, args);
        }
        static public void Trace(string strFormat, params object[] args)
        {
            if (!EnableLog)
            {
                return;
            }

            string strFileLine = "";
            StackTrace insStackTrace = new StackTrace(true);
            if (insStackTrace != null)
            {
                StackFrame insStackFrame = insStackTrace.GetFrame(1);
                if (insStackFrame != null)
                {
                    strFileLine = String.Format("{0}({1})", insStackFrame.GetFileName(), insStackFrame.GetFileLineNumber());
                    strFileLine = GetScriptFileName(strFileLine);
                }
            }
            LogOutput(LogLevel.LogLevel_Trace, strFileLine, strFormat, args);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 以下为私有接口

        private static string GetScriptFileName(string strFileLine)
        {
            if (strFileLine == null || strFileLine == "")
            {
                return "";
            }

            strFileLine = strFileLine.Replace("\\", "/");
            int pos = strFileLine.IndexOf("/Scripts/");
            string strResult = strFileLine.Substring(pos + 1, strFileLine.Length - pos - 1);
            return strResult;
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        private static void PushLog(string str)
        {
            if (m_LogQueue == null)
            {
                m_LogQueue = new Queue<string>();
            }

            m_LogQueue.Enqueue(str);

            if (m_LogQueue.Count > 100)
            {
                m_LogQueue.Dequeue();
            }
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        private static void PushLogToFile(string str)
        {
            if (m_LogFile != null && m_bWriteLogFile)
            {
                m_LogFile.WriteLine(str);
                m_LogFile.Flush();
            }
        }

        ///// <summary>
        ///// 读取缓存的Log最多50条
        ///// </summary>
        //public static void ReadFromFile(int NeedMax = 50)
        //{
        //    m_bWriteLogFile = false;
        //    Close();
        //    string str = string.Empty;
        //    if (m_LogError != null)
        //    {
        //        m_LogError.Clear();
        //    }
        //    using (StreamReader sr = new StreamReader(Application.persistentDataPath + "/Star_Age_Log.txt"))
        //    {
        //        str = sr.ReadLine();
        //        while (!string.IsNullOrEmpty(str))
        //        {
        //            if (m_LogError == null)
        //            {
        //                m_LogError = new List<string>();
        //            }
        //            m_LogError.Add(str);
        //            str = sr.ReadLine();
        //        }
        //        sr.Close();
        //    }
        //    m_bWriteLogFile = true;
        //    m_IsOpen = false;
        //    //Utility.Log.Open(SystemConfig.eMaxLogLevel, Set.EnableLog, true);
        //    if (m_LogError != null)
        //    {
        //        if (m_LogError.Count > NeedMax)
        //        {
        //            m_LogError.RemoveRange(0, m_LogError.Count - NeedMax);
        //        }
        //        //m_LogError.RemoveAll(item => item.StartsWith("[Error]") == false);
        //    }
        //}

        /// <summary>
        /// 处理日志文件函数
        /// </summary>
        private static void HandleLog(string logString, string stackTrace, LogType type)
        {
            string strLog = logString;
            string strStackTrace = stackTrace;
            switch (type)
            {
                case LogType.Log:
                    {
                        PushLogToFile(strLog);
                        strLog = "[00FF00]" + strLog;
                        PushLog(strLog);
                        break;
                    }
                case LogType.Error:
                    {
                        PushLogToFile(strLog);
                        strLog = "[FF0000]" + strLog;
                        PushLog(strLog);
                        break;
                    }
                case LogType.Warning:
                    {
                        PushLogToFile(strLog);
                        strLog = "[FFFF00]" + strLog;
                        PushLog(strLog);
                        break;
                    }
                case LogType.Exception:
                    {
                        strLog = "[Exception]" + DateTime.Now.ToString() + ":" + logString;
                        PushLogToFile(strLog);
                        strLog = "[FF0000]" + strLog;
                        PushLog(strLog);

                        strStackTrace = "[StackTrace]" + DateTime.Now.ToString() + ":" + stackTrace;
                        PushLogToFile(strStackTrace);
                        strStackTrace = "[FF0000]" + strStackTrace;
                        PushLog(strStackTrace);

                        //EventManager.send(EventType_Global.GAME_ERROR);
                    }
                    break;
            }
        }

        static private void LogOutput(LogLevel level, string strFileLine, string strFormat, params object[] args)
        {
            if (!EnableLog)
            {
                return;
            }

            if (level > MaxLogLevel)
            {
                return;
            }

            // 添加时间参数
            DateTime now = DateTime.Now;
            string strTime = string.Format("{0}-{1}-{2} {3}:{4}:{5}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            string strLog = "";
            string strOutLog = string.Format(strFormat, args);
            switch (level)
            {
                case LogLevel.LogLevel_Info:
                    strLog = string.Format("{0}{1} {2} - {3}", "[Info]", strOutLog, strFileLine, strTime);
                    UnityEngine.Debug.Log(strLog, null);
                    break;
                case LogLevel.LogLevel_Trace:
                    strLog = string.Format("{0}{1} {2} - {3}", "[Trace]", strOutLog, strFileLine, strTime);
                    UnityEngine.Debug.Log(strLog, null);
                    break;
                case LogLevel.LogLevel_Error:
                    strLog = string.Format("{0}{1} {2} - {3}", "[Error]", strOutLog, strFileLine, strTime);
                    UnityEngine.Debug.LogError(strLog, null);
                    break;
                case LogLevel.LogLevel_Warning:
                    strLog = string.Format("{0}{1} {2} - {3}", "[Warning]", strOutLog, strFileLine, strTime);
                    UnityEngine.Debug.LogWarning(strLog, null);
                    break;
            }
        }

        // 不常用接口
        public static void Break()
        {
            UnityEngine.Debug.Break();
        }

        public static void ClearDeveloperConsole()
        {
            UnityEngine.Debug.ClearDeveloperConsole();
        }

        public static void DebugBreak()
        {
            UnityEngine.Debug.DebugBreak();
        }

    }
}