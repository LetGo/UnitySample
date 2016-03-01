using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Engine.NetWork
{
    class ReceiverThread : BasicThread
    {
        const uint MaxPacketSize = 1024 * 512;
        //缓冲区;
        private byte[] _readBuffer;
        //缓冲区  读取 下标;
        private int _readOffset;
        //缓冲区  写入 下标;
        private int _writeOffset;

        public ReceiverThread(NetworkStream stream): base(stream)
        {
            _readBuffer = new byte[2 * MaxPacketSize];
            _writeOffset = 0;
            _readOffset = 0;
        }

        protected override void Run()
        {
            while (!IsTerminateFlagSet())
            {
                ReadFromStream();
                if (_writeOffset > 1)
                {
                    //                    _readOffset = 0;
                    if (_writeOffset >= NetPackageIn.HEADER_SIZE)
                    {
                        ReadPackage();
                    }
                }
            }
        }
        protected void ReadFromStream()
        {
            if (NetStream.DataAvailable)
            {
                _writeOffset += NetStream.Read(_readBuffer, _writeOffset, _readBuffer.Length - _writeOffset);
            }
            else
            {
                Thread.Sleep(16);
            }
        }
        protected void ReadPackage()
        {
            int dataLeft = _writeOffset - _readOffset;
            do
            {
                int len = -1;
                int nIndex = 0;
                if (_readOffset + NetPackageIn.HEADER_SIZE <= _writeOffset)
                {
                    // 长度不加密 支持PtotoBuff
                    while (nIndex < NetPackageIn.HEADER_SIZE)
                    {
                        nIndex++;
                        if (TryReadUInt32Variant(_readBuffer, _readOffset, nIndex, out len) > 0)
                        {
                            _readOffset += nIndex;
                            break;
                        }
                    }
                }

                if (len == 0 || len == -1)
                {
                    _readOffset = 0;
                    return;
                }

                dataLeft = _writeOffset - _readOffset;
                //包足够长;
                if (dataLeft >= len && len != 0)
                {
                    byte[] buf = new byte[len];
                    Array.Copy(_readBuffer, _readOffset, buf, 0, len);
// 
//                     if (NetWork.DEENCODE)
//                     {
//                         /// decode
//                         //Engine.Encrypt.Decode(ref buf);
//                     }

                    NetPackageIn package = new NetPackageIn(buf);
                    //TODO
                    //NetManager.Instance().PushRecv(package);
                    _readOffset += len;
                    dataLeft = _writeOffset - _readOffset;
                    //AddPackage(buff);
                }
                else
                {
                    _readOffset -= nIndex;
                    dataLeft = _writeOffset - _readOffset;
                    break;
                }
            } while (dataLeft >= NetPackageIn.HEADER_SIZE);

            // 数据往前移
            if (dataLeft > 0)
            {
                for (int i = _readOffset, j = 0; i < _readOffset + dataLeft; i++, j++)
                {
                    _readBuffer[j] = _readBuffer[i];
                }
            }
            _readOffset = 0;
            _writeOffset = dataLeft;

        }

        public int TryReadUInt32Variant(byte[] source, int offset, int count, out int value)
        {
            var max = Math.Min(source.Length, offset + count);
            value = 0;
            int b = offset >= max ? -1 : source[offset++];
            if (b < 0)
            {
                return 0;
            }
            value = (int)b;
            if ((value & 128u) == 0u)
            {
                return 1;
            }
            value &= 127;
            b = offset >= max ? -1 : source[offset++];
            if (b < 0)
            {
                return 0;
            }
            value |= (int)((uint)(b & 127) << 7);
            if ((b & 128) == 0)
            {
                return 2;
            }
            b = offset >= max ? -1 : source[offset++];
            if (b < 0)
            {
                return 0;
            }
            value |= (int)((uint)(b & 127) << 14);
            if ((b & 128) == 0)
            {
                return 3;
            }
            b = offset >= max ? -1 : source[offset++];
            if (b < 0)
            {
                return 0;
            }
            value |= (int)((uint)(b & 127) << 21);
            if ((b & 128) == 0)
            {
                return 4;
            }
            b = offset >= max ? -1 : source[offset++];
            if (b < 0)
            {
                return 0;
            }
            value |= (int)((uint)b << 28);
            if ((b & 240) == 0)
            {
                return 5;
            }
            return 0;
        }
    }
}
