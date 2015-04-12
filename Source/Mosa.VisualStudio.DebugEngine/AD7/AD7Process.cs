using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witschi.Debug.Engine.AD7.Impl;
using Witschi.Debug.Engine.Host;

namespace Witschi.Debug.Engine.AD7
{
    class AD7Process : IDebugProcess2
    {
        readonly Guid _physicalProcessId = Guid.NewGuid();
        readonly Guid _processId = Guid.NewGuid();
        AD7Engine _engine;
        IDebugPort2 _port;
        DebugHost _host;
        AD7Module _primaryModule;
        AD7Thread _primaryThread;
        AD7Program _program;
        SymbolEngine _symbolEngine;
        List<AD7Module> _modules = new List<AD7Module>();

        internal AD7Process(AD7Engine engine, IDebugPort2 port, DebugHost host)
        {
            _symbolEngine = new SymbolEngine();
            _engine = engine;
            _port = port;
            _host = host;
            _host.Disconnected += _host_Disconnected;
            _host.Suspended += _host_Suspended;
        }

        void _host_Disconnected()
        {
            if(_engine.Callback != null)
                _engine.Callback.OnProgramExit(_program, 99);
        }

        void _host_Suspended()
        {
            if(_engine.Callback != null)
                _engine.Callback.OnAsyncBreakComplete(_primaryThread);
        }

        public AD_PROCESS_ID PhysicalProcessId
        {
            get
            {
                AD_PROCESS_ID id = new AD_PROCESS_ID();
                id.guidProcessId = _physicalProcessId;
                id.ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID;
                return id;
            }
        }

        public Guid Id
        {
            get { return _processId; }
        }

        public AD7Program Program
        {
            get { return _program; }
        }

        public AD7Module PrimaryModule
        {
            get { return _primaryModule; }
        }

        public AD7Thread PrimaryThread
        {
            get { return _primaryThread; }
        }

        public DebugHost Host
        {
            get { return _host; }
        }

        public SymbolEngine SymbolEngine
        {
            get { return _symbolEngine; }
        }

        public void Attach(IDebugProgram2 program)
        {
            _host.Attach();
            Guid programId;
            program.GetProgramId(out programId);
            
            _program = new AD7Program(_engine, this, programId, "Dummy Program Name");
            AD7ProgramCreateEvent.Send(_engine, _program);

            _primaryModule = new AD7Module(_program);
            _program.Module = _primaryModule;
            _modules.Add(_primaryModule);

            string symbolPath;
            if(_symbolEngine.LoadSymbolsForModule(this.File, out symbolPath))
            {
                _primaryModule.SymbolPath = symbolPath;
                _primaryModule.SymbolsLoaded = true;
            }

            _engine.Callback.OnModuleLoad(_primaryModule);

            _primaryThread = new AD7Thread(_program, 17);
            _engine.Callback.OnThreadStart(_primaryThread);
        }

        public void Break()
        {
            _host.Suspend();
        }

        string File { get; set; }
        public void LaunchSuspended(string fileName)
        {
            this.File = fileName;
            _host.LaunchSuspended(fileName);
        }

        public void ResumeFromLaunch()
        {
            _host.Resume();
            _engine.Callback.OnSymbolSearch(PrimaryModule, "Starting symbol search", 0);
        }

        public void Terminate()
        {
            _host.Terminate();
        }

        public AD7Module ResolveAddress(ulong address)
        {
            return _primaryModule;
        }

        int IDebugProcess2.Attach(IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach)
        {
            throw new NotImplementedException();
        }

        int IDebugProcess2.CanDetach()
        {
            if (_host.IsAttached)
                return VSConstants.S_OK;
            else
                return VSConstants.S_FALSE;
        }

        int IDebugProcess2.CauseBreak()
        {
            Task.Factory.StartNew(new Action(() => { Break(); _engine.Callback.OnAsyncBreakComplete(PrimaryThread); }));
            return VSConstants.S_OK;
        }

        int IDebugProcess2.Detach()
        {
            if (_host.IsAttached)
            {
                _host.Detach();
                return VSConstants.S_OK;
            }
            else
                return VSConstants.E_FAIL;
        }

        int IDebugProcess2.EnumPrograms(out IEnumDebugPrograms2 ppEnum)
        {
            ppEnum = new AD7ProgramEnum(new IDebugProgram2[] { _program });
            return VSConstants.S_OK;
        }

        int IDebugProcess2.EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            ppEnum = new AD7ThreadEnum(new IDebugThread2[] { _primaryThread });
            return VSConstants.S_OK;
        }

        int IDebugProcess2.GetAttachedSessionName(out string pbstrSessionName)
        {
            throw new NotImplementedException();
        }

        int IDebugProcess2.GetInfo(enum_PROCESS_INFO_FIELDS Fields, PROCESS_INFO[] pProcessInfo)
        {
            PROCESS_INFO pi = new PROCESS_INFO();
            if ((Fields & enum_PROCESS_INFO_FIELDS.PIF_ATTACHED_SESSION_NAME) == enum_PROCESS_INFO_FIELDS.PIF_ATTACHED_SESSION_NAME)
            {
                pi.bstrAttachedSessionName = "blah session name";
                pi.Fields |= enum_PROCESS_INFO_FIELDS.PIF_ATTACHED_SESSION_NAME;
            }
            if ((Fields & enum_PROCESS_INFO_FIELDS.PIF_BASE_NAME) == enum_PROCESS_INFO_FIELDS.PIF_BASE_NAME)
            {
                pi.bstrBaseName = "blah base name";
                pi.Fields |= enum_PROCESS_INFO_FIELDS.PIF_BASE_NAME;
            }
            if ((Fields & enum_PROCESS_INFO_FIELDS.PIF_CREATION_TIME) == enum_PROCESS_INFO_FIELDS.PIF_CREATION_TIME)
            {
            }
            if ((Fields & enum_PROCESS_INFO_FIELDS.PIF_FILE_NAME) == enum_PROCESS_INFO_FIELDS.PIF_FILE_NAME)
            {
                pi.bstrFileName = "blah file name";
                pi.Fields |= enum_PROCESS_INFO_FIELDS.PIF_FILE_NAME;
            }
            if ((Fields & enum_PROCESS_INFO_FIELDS.PIF_FLAGS) == enum_PROCESS_INFO_FIELDS.PIF_FLAGS)
            {
                if (_host.IsAttached)
                    pi.Flags = enum_PROCESS_INFO_FLAGS.PIFLAG_DEBUGGER_ATTACHED;
                if (_host.IsRunning)
                    pi.Flags = enum_PROCESS_INFO_FLAGS.PIFLAG_PROCESS_RUNNING;
                else
                    pi.Flags = enum_PROCESS_INFO_FLAGS.PIFLAG_PROCESS_STOPPED;
                pi.Fields |= enum_PROCESS_INFO_FIELDS.PIF_FLAGS;
            }
            if ((Fields & enum_PROCESS_INFO_FIELDS.PIF_PROCESS_ID) == enum_PROCESS_INFO_FIELDS.PIF_PROCESS_ID)
            {
                pi.ProcessId = PhysicalProcessId;
                pi.Fields |= enum_PROCESS_INFO_FIELDS.PIF_PROCESS_ID;
            }
            if ((Fields & enum_PROCESS_INFO_FIELDS.PIF_SESSION_ID) == enum_PROCESS_INFO_FIELDS.PIF_SESSION_ID)
            {
                pi.dwSessionId = 27;
                pi.Fields |= enum_PROCESS_INFO_FIELDS.PIF_SESSION_ID;
            }
            if ((Fields & enum_PROCESS_INFO_FIELDS.PIF_TITLE) == enum_PROCESS_INFO_FIELDS.PIF_TITLE)
            {
                pi.bstrTitle = "blah title";
                pi.Fields |= enum_PROCESS_INFO_FIELDS.PIF_TITLE;
            }
            if ((Fields & enum_PROCESS_INFO_FIELDS.PIF_FILE_NAME) == enum_PROCESS_INFO_FIELDS.PIF_FILE_NAME)
            {
                pi.bstrFileName = "blah file name";
                pi.Fields |= enum_PROCESS_INFO_FIELDS.PIF_FILE_NAME;
            }
            return VSConstants.S_OK;
        }

        int IDebugProcess2.GetName(enum_GETNAME_TYPE gnType, out string pbstrName)
        {
            pbstrName = "stupid name";
            return VSConstants.S_OK;
        }

        int IDebugProcess2.GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId)
        {
            pProcessId[0].guidProcessId = _physicalProcessId;
            pProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID;

            return VSConstants.S_OK;
        }

        int IDebugProcess2.GetPort(out IDebugPort2 ppPort)
        {
            ppPort = _port;
            if (_port == null)
                return VSConstants.E_FAIL;
            else
                return VSConstants.S_OK;
        }

        int IDebugProcess2.GetProcessId(out Guid pguidProcessId)
        {
            pguidProcessId = _processId;
            return VSConstants.S_OK;
        }

        int IDebugProcess2.GetServer(out IDebugCoreServer2 ppServer)
        {
            throw new NotImplementedException();
        }

        int IDebugProcess2.Terminate()
        {
            Terminate();
            return VSConstants.S_OK;
        }
    }
}
