using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witschi.Debug.Engine.AD7.Impl;

namespace Witschi.Debug.Engine.AD7
{
    class AD7Program : IDebugProgram2
    {
        AD7Engine _engine;
        public AD7Engine Engine
        {
            get { return _engine; }
        }

        AD7Process _process;
        public AD7Process Process
        {
            get { return _process; }
        }

        Guid _id;
        public Guid Id
        {
            get { return _id; }
        }

        string _name;
        public string Name
        {
            get { return _name; }
        }

        public AD7Module Module
        {
            get;
            set;
        }

        internal AD7Program(AD7Engine engine, AD7Process process, Guid id, string name)
        {
            _process = process;
            _engine = engine;
            _id = id;
            _name = name;
        }

        int IDebugProgram2.Attach(IDebugEventCallback2 pCallback)
        {
            throw new NotImplementedException();
        }

        int IDebugProgram2.CanDetach()
        {
            return ((IDebugProcess2)_process).CanDetach();
        }

        int IDebugProgram2.CauseBreak()
        {
            ((IDebugProcess2)Process).CauseBreak();
            return VSConstants.S_OK;
        }

        int IDebugProgram2.Continue(IDebugThread2 pThread)
        {
            System.Diagnostics.Debug.WriteLine("NYI:IDebugProgram2.Continue");
            return VSConstants.S_OK;
        }

        int IDebugProgram2.Detach()
        {
            return ((IDebugProcess2)_process).Detach();
        }

        int IDebugProgram2.EnumCodeContexts(IDebugDocumentPosition2 pDocPos, out IEnumDebugCodeContexts2 ppEnum)
        {
            throw new NotImplementedException();
        }

        int IDebugProgram2.EnumCodePaths(string pszHint, IDebugCodeContext2 pStart, IDebugStackFrame2 pFrame, int fSource, out IEnumCodePaths2 ppEnum, out IDebugCodeContext2 ppSafety)
        {
            throw new NotImplementedException();
        }

        int IDebugProgram2.EnumModules(out IEnumDebugModules2 ppEnum)
        {
            ppEnum = new AD7ModuleEnum(new IDebugModule2[] { Module });
            return VSConstants.S_OK;
        }

        int IDebugProgram2.EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            return ((IDebugProcess2)Process).EnumThreads(out ppEnum);
        }

        int IDebugProgram2.Execute()
        {
            _process.Host.Resume();
            //throw new NotImplementedException();
            return VSConstants.S_OK;
        }

        int IDebugProgram2.GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            throw new NotImplementedException();
        }

        int IDebugProgram2.GetDisassemblyStream(enum_DISASSEMBLY_STREAM_SCOPE dwScope, IDebugCodeContext2 pCodeContext, out IDebugDisassemblyStream2 ppDisassemblyStream)
        {
            AD7MemoryAddress addr = pCodeContext as AD7MemoryAddress;
            if (dwScope == enum_DISASSEMBLY_STREAM_SCOPE.DSS_ALL)
            {
            }
            else
            {
            }
            FunctionSourceInfo fsi = _process.SymbolEngine.GetInfoForAddress(addr.Address);
            if (fsi != null)
            {
                ppDisassemblyStream = new AD7DisassemblyStream(_process.Host, addr, fsi);
                return VSConstants.S_OK;
            }
            throw new NotImplementedException();
        }

        int IDebugProgram2.GetENCUpdate(out object ppUpdate)
        {
            throw new NotImplementedException();
        }

        int IDebugProgram2.GetEngineInfo(out string pbstrEngine, out Guid pguidEngine)
        {
            pguidEngine = new Guid(Guids.engineId);
            pbstrEngine = "WitchCraft Kernel";
            return VSConstants.S_OK;
        }

        int IDebugProgram2.GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            ppMemoryBytes = new AD7MemoryReader(this.Process.Host);
            return VSConstants.S_OK;
        }

        int IDebugProgram2.GetName(out string pbstrName)
        {
            pbstrName = _name;
            return VSConstants.S_OK;
        }

        int IDebugProgram2.GetProcess(out IDebugProcess2 ppProcess)
        {
            throw new NotImplementedException();
        }

        int IDebugProgram2.GetProgramId(out Guid pguidProgramId)
        {
            pguidProgramId = _id;
            return VSConstants.S_OK;
        }

        int IDebugProgram2.Step(IDebugThread2 pThread, enum_STEPKIND sk, enum_STEPUNIT Step)
        {
            throw new NotImplementedException();
        }

        int IDebugProgram2.Terminate()
        {
            return ((IDebugProcess2)Process).Terminate();
        }

        int IDebugProgram2.WriteDump(enum_DUMPTYPE DUMPTYPE, string pszDumpUrl)
        {
            throw new NotImplementedException();
        }
    }
}
