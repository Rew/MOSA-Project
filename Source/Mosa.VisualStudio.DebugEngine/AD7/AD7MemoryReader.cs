using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witschi.Debug.Engine.AD7
{
    class AD7MemoryReader : IDebugMemoryBytes2
    {
        Host.DebugHost _host;

        public AD7MemoryReader(Host.DebugHost host)
        {
            _host = host;
        }

        int IDebugMemoryBytes2.GetSize(out ulong pqwSize)
        {
            throw new NotImplementedException();
        }

        int IDebugMemoryBytes2.ReadAt(IDebugMemoryContext2 pStartContext, uint dwCount, byte[] rgbMemory, out uint pdwRead, ref uint pdwUnreadable)
        {
            byte[] data = _host.ReadMemory((pStartContext as AD7MemoryAddress).Address, dwCount);
            for (int i = 0; i < data.Length; i++)
            {
                rgbMemory[i] = data[i];
            }
            pdwRead = (uint)data.Length;
            pdwUnreadable = 0;
            return VSConstants.S_OK;
        }

        int IDebugMemoryBytes2.WriteAt(IDebugMemoryContext2 pStartContext, uint dwCount, byte[] rgbMemory)
        {
            throw new NotImplementedException();
        }
    }
}
