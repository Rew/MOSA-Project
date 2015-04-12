using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Witschi.Debug.Engine.AD7.Impl;

namespace Witschi.Debug.Engine.AD7
{
    [ComVisible(true)]
    [Guid(Guids.programProviderGuid)]
    public class AD7ProgramProvider : IDebugProgramProvider2
    {
        int IDebugProgramProvider2.GetProviderProcessData(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, CONST_GUID_ARRAY EngineFilter, PROVIDER_PROCESS_DATA[] pProcess)
        {
            pProcess[0] = new PROVIDER_PROCESS_DATA();

            if (ProcessId.ProcessIdType == (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID)
            {
                IDebugProgramNode2 node = (IDebugProgramNode2)new AD7ProgramNode(ProcessId);

                IntPtr[] programNodes = { Marshal.GetComInterfaceForObject(node, typeof(IDebugProgramNode2)) };

                IntPtr destinationArray = Marshal.AllocCoTaskMem(IntPtr.Size * programNodes.Length);
                Marshal.Copy(programNodes, 0, destinationArray, programNodes.Length);

                pProcess[0].Fields = enum_PROVIDER_FIELDS.PFIELD_PROGRAM_NODES;
                pProcess[0].ProgramNodes.Members = destinationArray;
                pProcess[0].ProgramNodes.dwCount = (uint)programNodes.Length;

                return VSConstants.S_OK;
            }
            return VSConstants.S_FALSE;
        }

        int IDebugProgramProvider2.GetProviderProgramNode(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, ref Guid guidEngine, ulong programId, out IDebugProgramNode2 ppProgramNode)
        {
            MessageBox.Show("GetProviderProgramNode");
            ppProgramNode = null;
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramProvider2.SetLocale(ushort wLangID)
        {
            return VSConstants.S_OK;
        }

        int IDebugProgramProvider2.WatchForProviderEvents(enum_PROVIDER_FLAGS Flags, IDebugDefaultPort2 pPort, AD_PROCESS_ID ProcessId, CONST_GUID_ARRAY EngineFilter, ref Guid guidLaunchingEngine, IDebugPortNotify2 pEventCallback)
        {
            return VSConstants.S_OK;
        }
    }
}
