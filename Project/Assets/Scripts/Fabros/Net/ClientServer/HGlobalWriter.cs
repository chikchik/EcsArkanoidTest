using System;
using System.Runtime.InteropServices;

namespace Game.Fabros.Net.ClientServer
{
    public class HGlobalWriter: IDisposable
    {
        private IntPtr buffer;
        private int bufferSize;
        private int pos;

        public HGlobalWriter()
        {
            bufferSize = 255;
            buffer = Marshal.AllocHGlobal(bufferSize);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(buffer);
        }

        private void Extend(int size)
        {
            var reqSize = size + pos;
            if (reqSize > bufferSize)
            {
                reqSize += 16;
                do
                {
                    bufferSize *= 2;
                } while (bufferSize < reqSize);

                buffer = Marshal.ReAllocHGlobal(buffer, (IntPtr)bufferSize);
            }
        }

        public byte[] CopyToByteArray(byte[] array = null)
        {
            if (array == null)
                array = new byte[pos];
            Marshal.Copy(buffer, array, 0, pos);
            return array;
        }
        
        public HGlobalWriter Write<T>(T[] data, int len = -1) where T : struct
        {
            if (len == -1)
                len = data.Length;
            
            var oneSize = Marshal.SizeOf<T>();
            var size = oneSize * len;
            Extend(size);

            for (int i = 0; i < len; ++i)
            {
                Marshal.StructureToPtr(data[i], buffer + pos, false);
                pos += oneSize;
            }

            return this;
        }
        
        public HGlobalWriter Write(byte[] data, int len = -1)
        {
            if (len == -1)
                len = data.Length;
            
            var size = len;
            Extend(size);

            for (int i = 0; i < len; ++i)
            {
                Marshal.WriteByte(buffer + pos, data[i]);
                pos += 1;
            }

            return this;
        }

        public HGlobalWriter Write<T>(T data) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            Extend(size);

            Marshal.StructureToPtr(data, buffer + pos, false);
            pos += size;
            
            return this;
        }
        
        public HGlobalWriter WriteInt32(int value)
        {
            return Write(value);
        }
        
        public HGlobalWriter WriteByte(byte value)
        {
            return Write(value);
        }

        public HGlobalWriter Reset()
        {
            pos = 0;
            return this;
        }
    }
}