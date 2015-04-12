using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Witschi.Debug.Engine.AD7
{
    class AD7BreakpointResolution : IDebugBreakpointResolution2
    {
        private AD7Program _program;
        private ulong _address;
        private AD7DocumentContext _documentContext;

        public AD7BreakpointResolution(AD7Program program, ulong address, AD7DocumentContext documentContext)
        {
            _program = program;
            _address = address;
            _documentContext = documentContext;
        }

        #region IDebugBreakpointResolution2 Members

        // Gets the type of the breakpoint represented by this resolution. 
        int IDebugBreakpointResolution2.GetBreakpointType(enum_BP_TYPE[] pBPType)
        {
            // The sample engine only supports code breakpoints.
            pBPType[0] = enum_BP_TYPE.BPT_CODE;
            return VSConstants.S_OK;
        }

        // Gets the breakpoint resolution information that describes this breakpoint.
        int IDebugBreakpointResolution2.GetResolutionInfo(enum_BPRESI_FIELDS dwFields, BP_RESOLUTION_INFO[] pBPResolutionInfo)
        {
            if ((dwFields & enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION) != 0)
            {
                // The sample engine only supports code breakpoints.
                BP_RESOLUTION_LOCATION location = new BP_RESOLUTION_LOCATION();
                location.bpType = (uint)enum_BP_TYPE.BPT_CODE;

                // The debugger will not QI the IDebugCodeContex2 interface returned here. We must pass the pointer
                // to IDebugCodeContex2 and not IUnknown.
                AD7MemoryAddress codeContext = new AD7MemoryAddress(_program, _address);
                codeContext.SetDocumentContext(_documentContext);
                location.unionmember1 = Marshal.GetComInterfaceForObject(codeContext, typeof(IDebugCodeContext2));
                pBPResolutionInfo[0].bpResLocation = location;
                pBPResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_BPRESLOCATION;

            }

            if ((dwFields & enum_BPRESI_FIELDS.BPRESI_PROGRAM) != 0)
            {
                pBPResolutionInfo[0].pProgram = _program;
                pBPResolutionInfo[0].dwFields |= enum_BPRESI_FIELDS.BPRESI_PROGRAM;
            }

            return VSConstants.S_OK;
        }

        #endregion
    }
}