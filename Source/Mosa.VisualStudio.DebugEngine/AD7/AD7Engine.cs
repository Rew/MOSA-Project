using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Witschi.Debug.Engine.AD7.Impl;
using Witschi.Debug.Engine.Host;

namespace Witschi.Debug.Engine.AD7
{
    [ComVisible(true)]
    [Guid(Guids.engineGuid)]
    public class AD7Engine : IDebugEngine2, IDebugEngineLaunch2
    {
        Dictionary<Guid, AD7Process> _processes = new Dictionary<Guid, AD7Process>();
        Dictionary<Guid, AD7Program> _programs = new Dictionary<Guid, AD7Program>();
        BreakpointManager _breakpointManager;

        public AD7Engine()
        {
            _breakpointManager = new BreakpointManager(this);
        }

        EngineCallback _engineCallback;
        internal EngineCallback Callback
        {
            get { return _engineCallback; }
        }

        internal IEnumerable<AD7Process> ListProcesses()
        {
            return _processes.Values.ToArray();
        }

        #region IDebugEngine2

        int IDebugEngine2.Attach(IDebugProgram2[] rgpPrograms, IDebugProgramNode2[] rgpProgramNodes, uint celtPrograms, IDebugEventCallback2 pCallback, enum_ATTACH_REASON dwReason)
        {
            if (celtPrograms != 1)
                return VSConstants.E_FAIL;

            if (_engineCallback == null)
            {
                _engineCallback = new EngineCallback(this, pCallback);
                AD7EngineCreateEvent.Send(this);
            }

            AD7ProgramNode node = rgpProgramNodes[0] as AD7ProgramNode;
            AD7Process process = null;

            foreach (AD7Process p in _processes.Values)
            {
                if (p.PhysicalProcessId.guidProcessId == node.PhysicalProcessId)
                    process = p;
            }

            if(process == null)
                throw new NotImplementedException();

            process.Attach(rgpPrograms[0]); // this calls program loaded event

            Callback.OnLoadComplete(process.PrimaryThread);
            AD7EntrypointEvent.Send(this, process.Program, process.PrimaryThread);
            return VSConstants.S_OK;
        }

        int IDebugEngine2.CauseBreak()
        {
            throw new NotImplementedException();
        }

        int IDebugEngine2.ContinueFromSynchronousEvent(IDebugEvent2 pEvent)
        {
            if (pEvent is AD7ProgramDestroyEvent)
            {
                _engineCallback = null;
                return VSConstants.S_OK;
            }
            else
                throw new NotImplementedException();
        }

        int IDebugEngine2.CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
        {
            ppPendingBP = null;
            try
            {
                _breakpointManager.CreatePendingBreakpoint(pBPRequest, out ppPendingBP);
            }
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }

            return VSConstants.S_OK;
        }

        int IDebugEngine2.DestroyProgram(IDebugProgram2 pProgram)
        {
            throw new NotImplementedException();
        }

        int IDebugEngine2.EnumPrograms(out IEnumDebugPrograms2 ppEnum)
        {
            ppEnum = null;
            throw new NotImplementedException();
        }

        int IDebugEngine2.GetEngineId(out Guid pguidEngine)
        {
            pguidEngine = new Guid(Guids.engineId);
            return VSConstants.E_NOTIMPL;
        }

        int IDebugEngine2.RemoveAllSetExceptions(ref Guid guidType)
        {
            throw new NotImplementedException();
        }

        int IDebugEngine2.RemoveSetException(EXCEPTION_INFO[] pException)
        {
            throw new NotImplementedException();
        }

        int IDebugEngine2.SetException(EXCEPTION_INFO[] pException)
        {
            throw new NotImplementedException();
        }

        int IDebugEngine2.SetLocale(ushort wLangID)
        {
            return VSConstants.E_NOTIMPL;
        }

        int IDebugEngine2.SetMetric(string pszMetric, object varValue)
        {
            throw new NotImplementedException();
        }

        int IDebugEngine2.SetRegistryRoot(string pszRegistryRoot)
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion

        #region IDebugEngineLaunch2

        int IDebugEngineLaunch2.CanTerminateProcess(IDebugProcess2 pProcess)
        {
            return VSConstants.S_OK;
        }

        int IDebugEngineLaunch2.LaunchSuspended(string pszServer, IDebugPort2 pPort, string pszExe, string pszArgs, string pszDir, string bstrEnv, string pszOptions, enum_LAUNCH_FLAGS dwLaunchFlags, uint hStdInput, uint hStdOutput, uint hStdError, IDebugEventCallback2 pCallback, out IDebugProcess2 ppProcess)
        {
            try
            {
                DebugHost host = new Qemu();
                AD7Process process = new AD7Process(this, pPort, host);
                process.LaunchSuspended(pszExe);
                _processes.Add(process.Id, process);

                ppProcess = process;

                return VSConstants.S_OK;
            }
            catch
            {
                ppProcess = null;
                return VSConstants.E_FAIL;
            }
        }

        int IDebugEngineLaunch2.ResumeProcess(IDebugProcess2 pProcess)
        {
            Guid processId;
            AD7Process process = pProcess as AD7Process;
            
            if((pProcess.GetProcessId(out processId) != VSConstants.S_OK))
                return VSConstants.S_FALSE;
            if((process == null) || (!_processes.ContainsKey(processId)))
                return VSConstants.S_FALSE;

            // Send a program node to the SDM. This will cause the SDM to turn around and call IDebugEngine2.Attach
            // which will complete the hookup with AD7
            IDebugPort2 port;
            EngineUtils.RequireOk(pProcess.GetPort(out port));

            IDebugDefaultPort2 defaultPort = (IDebugDefaultPort2)port;

            IDebugPortNotify2 portNotify;
            EngineUtils.RequireOk(defaultPort.GetPortNotify(out portNotify));

            EngineUtils.RequireOk(portNotify.AddProgramNode(new AD7ProgramNode(process.PhysicalProcessId)));

            if(process.Program == null)
            {
                System.Diagnostics.Debug.Fail("Unexpected problem -- IDebugEngine2.Attach wasn't called");
                return VSConstants.E_FAIL;
            }

            process.ResumeFromLaunch();

            return VSConstants.S_OK;
        }

        int IDebugEngineLaunch2.TerminateProcess(IDebugProcess2 pProcess)
        {
            Guid processId;
            pProcess.GetProcessId(out processId);

            if (_processes.ContainsKey(processId))
            {
                _processes[processId].Terminate();
                Callback.OnProgramExit(_processes[processId].Program, 0);
                _processes.Remove(processId);
                return VSConstants.S_OK;
            }
            else
                return VSConstants.E_FAIL;
        }

        #endregion
    }
}
