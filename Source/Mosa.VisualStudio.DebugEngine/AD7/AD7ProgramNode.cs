using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witschi.Debug.Engine.AD7
{
    class AD7ProgramNode : IDebugProgramNode2
    {
        private AD_PROCESS_ID _processId;

        public Guid PhysicalProcessId
        {
            get { return _processId.guidProcessId; }
        }

        public AD7ProgramNode(AD_PROCESS_ID processId)
        {
            _processId = processId;
        }

        int IDebugProgramNode2.Attach_V7(IDebugProgram2 pMDMProgram, IDebugEventCallback2 pCallback, uint dwReason)
        {
            throw new NotImplementedException();
        }

        int IDebugProgramNode2.DetachDebugger_V7()
        {
            throw new NotImplementedException();
        }

        int IDebugProgramNode2.GetEngineInfo(out string pbstrEngine, out Guid pguidEngine)
        {
            pguidEngine = new Guid(Guids.engineId);
            pbstrEngine = "WitchCraft Kernel";
            return VSConstants.S_OK;
        }

        int IDebugProgramNode2.GetHostMachineName_V7(out string pbstrHostMachineName)
        {
            throw new NotImplementedException();
        }

        int IDebugProgramNode2.GetHostName(enum_GETHOSTNAME_TYPE dwHostNameType, out string pbstrHostName)
        {
            pbstrHostName = "";
            return VSConstants.E_NOTIMPL;
        }

        int IDebugProgramNode2.GetHostPid(AD_PROCESS_ID[] pHostProcessId)
        {
            pHostProcessId[0] = _processId;
            return VSConstants.S_OK;
        }

        int IDebugProgramNode2.GetProgramName(out string pbstrProgramName)
        {
            pbstrProgramName = "";
            return VSConstants.E_NOTIMPL;
        }
    }
}
