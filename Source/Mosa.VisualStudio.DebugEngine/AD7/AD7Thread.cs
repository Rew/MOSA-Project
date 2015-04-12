using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witschi.Debug.Engine.Host;

namespace Witschi.Debug.Engine.AD7
{
    class AD7Thread : IDebugThread2
    {
        private AD7Program _program;
        private uint _threadId;

        public AD7Program Program
        {
            get { return _program; }
        }

        public AD7Thread(AD7Program program, uint threadId)
        {
            _program = program;
            _threadId = threadId;
        }

        public uint Id
        {
            get { return _threadId; }
        }

        int IDebugThread2.CanSetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            return VSConstants.S_FALSE;
        }

        int IDebugThread2.EnumFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, out IEnumDebugFrameInfo2 ppEnum)
        {
            Program.Process.Host.WalkStack(this);
            x86StackContext[] stack = Program.Process.Host.StackFrames;
            FRAMEINFO[] fi = (FRAMEINFO[])Array.CreateInstance(typeof(FRAMEINFO), stack.Length);
            
            for(int i = 0; i < stack.Length; i++)
            {
                AD7StackFrame sf = new AD7StackFrame(this, stack[i]);
                fi[i] = sf.GetFrame(dwFieldSpec);
            }

            ppEnum = new AD7FrameInfoEnum(fi);
            if (fi.Length > 0)
                return VSConstants.S_OK;
            else
                return VSConstants.E_FAIL;
        }

        int IDebugThread2.GetLogicalThread(IDebugStackFrame2 pStackFrame, out IDebugLogicalThread2 ppLogicalThread)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.GetName(out string pbstrName)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.GetProgram(out IDebugProgram2 ppProgram)
        {
            ppProgram = _program;
            return VSConstants.S_OK;
        }

        int IDebugThread2.GetThreadId(out uint pdwThreadId)
        {
            pdwThreadId = _threadId;
            return VSConstants.S_OK;
        }

        int IDebugThread2.GetThreadProperties(enum_THREADPROPERTY_FIELDS dwFields, THREADPROPERTIES[] ptp)
        {
            THREADPROPERTIES props = new THREADPROPERTIES();

            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_ID) == enum_THREADPROPERTY_FIELDS.TPF_ID)
            {
                props.dwThreadId = _threadId;
                props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_ID;
            }
            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_LOCATION) == enum_THREADPROPERTY_FIELDS.TPF_LOCATION)
            {
                props.bstrLocation = "DummyLoc";
                props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_LOCATION;
            }
            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_NAME) == enum_THREADPROPERTY_FIELDS.TPF_NAME)
            {
                props.bstrName = "Dummy thread name";
                props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_NAME;
            }
            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_PRIORITY) == enum_THREADPROPERTY_FIELDS.TPF_PRIORITY)
            {
                props.bstrPriority = "Normal";
                props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_PRIORITY;
            }
            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_STATE) == enum_THREADPROPERTY_FIELDS.TPF_STATE)
            {
                props.dwThreadState = (uint)enum_THREADSTATE.THREADSTATE_RUNNING;
                props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_STATE;
            }
            if ((dwFields & enum_THREADPROPERTY_FIELDS.TPF_SUSPENDCOUNT) == enum_THREADPROPERTY_FIELDS.TPF_SUSPENDCOUNT)
            {
                props.dwFields |= enum_THREADPROPERTY_FIELDS.TPF_SUSPENDCOUNT;
            }
            ptp[0] = props;
            return VSConstants.S_OK;
        }

        int IDebugThread2.Resume(out uint pdwSuspendCount)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.SetNextStatement(IDebugStackFrame2 pStackFrame, IDebugCodeContext2 pCodeContext)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.SetThreadName(string pszName)
        {
            throw new NotImplementedException();
        }

        int IDebugThread2.Suspend(out uint pdwSuspendCount)
        {
            throw new NotImplementedException();
        }
    }
}
