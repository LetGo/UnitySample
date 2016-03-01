using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Engine.NetWork
{
    /// <summary>
    /// 网络底层数据包
    /// </summary>
    public class NetPackageIn : MemoryStream
    {
        public static int HEADER_SIZE = 5;
        public NetPackageIn(byte[] buff)
            : base(buff)
        {

        }
    }
    /// <summary>
    /// 网络底层数据包
    /// </summary>
    public class NetPackageOut : MemoryStream
    {
        private int _code;

        public NetPackageOut(MemoryStream buff, int nCode)
        {
            Write(buff.GetBuffer(), 0, (int)buff.Length);
            Flush();
            _code = nCode;
        }

        public int getCode()
        {
            return _code;
        }
    }
}
