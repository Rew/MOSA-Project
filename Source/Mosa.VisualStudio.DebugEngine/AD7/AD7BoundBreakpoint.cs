using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Witschi.Debug.Engine.AD7
{
    // This class represents a breakpoint that has been bound to a location in the debuggee. It is a child of the pending breakpoint
    // that creates it. Unless the pending breakpoint only has one bound breakpoint, each bound breakpoint is displayed as a child of the
    // pending breakpoint in the breakpoints window. Otherwise, only one is displayed.
    class AD7BoundBreakpoint : IDebugBoundBreakpoint2
    {
        private AD7PendingBreakpoint _pendingBreakpoint;
        private AD7BreakpointResolution _breakpointResolution;
        private AD7Process _process;
        private ulong _address;

        private bool _enabled;
        private bool _deleted;

        public AD7BoundBreakpoint(AD7Process process, ulong address, AD7PendingBreakpoint pendingBreakpoint, AD7BreakpointResolution breakpointResolution)
        {
            _process = process;
            _address = address;
            _pendingBreakpoint = pendingBreakpoint;
            _breakpointResolution = breakpointResolution;
            _enabled = true;
            _deleted = false;
        }

        #region IDebugBoundBreakpoint2 Members

        // Called when the breakpoint is being deleted by the user.
        int IDebugBoundBreakpoint2.Delete()
        {
            //Debug.Assert(Worker.MainThreadId == Worker.CurrentThreadId);

            if (!_deleted)
            {
                _deleted = true;
                _process.Host.RemoveBreakpoint(_address, this);
                _pendingBreakpoint.OnBoundBreakpointDeleted(this);
            }

            return VSConstants.S_OK;
        }

        // Called by the debugger UI when the user is enabling or disabling a breakpoint.
        int IDebugBoundBreakpoint2.Enable(int fEnable)
        {
            //Debug.Assert(Worker.MainThreadId == Worker.CurrentThreadId);

            bool enabled = fEnable == 0 ? false : true;
            if (_enabled != enabled)
            {
                // A production debug engine would remove or add the underlying int3 here. The sample engine does not support true disabling
                // of breakpionts.
            }
            _enabled = fEnable == 0 ? false : true;
            return VSConstants.S_OK;
        }

        // Return the breakpoint resolution which describes how the breakpoint bound in the debuggee.
        int IDebugBoundBreakpoint2.GetBreakpointResolution(out IDebugBreakpointResolution2 ppBPResolution)
        {
            ppBPResolution = _breakpointResolution;
            return VSConstants.S_OK;
        }

        // Return the pending breakpoint for this bound breakpoint.
        int IDebugBoundBreakpoint2.GetPendingBreakpoint(out IDebugPendingBreakpoint2 ppPendingBreakpoint)
        {
            ppPendingBreakpoint = _pendingBreakpoint;
            return VSConstants.S_OK;
        }

        // 
        int IDebugBoundBreakpoint2.GetState(enum_BP_STATE[] pState)
        {
            pState[0] = 0;

            if (_deleted)
            {
                pState[0] = enum_BP_STATE.BPS_DELETED;
            }
            else if (_enabled)
            {
                pState[0] = enum_BP_STATE.BPS_ENABLED;
            }
            pState[0] = enum_BP_STATE.BPS_DISABLED;


            return VSConstants.S_OK;
        }

        // The sample engine does not support hit counts on breakpoints. A real-world debugger will want to keep track 
        // of how many times a particular bound breakpoint has been hit and return it here.
        int IDebugBoundBreakpoint2.GetHitCount(out uint pdwHitCount)
        {
            throw new NotImplementedException();
        }

        // The sample engine does not support conditions on breakpoints.
        // A real-world debugger will use this to specify when a breakpoint will be hit
        // and when it should be ignored.
        int IDebugBoundBreakpoint2.SetCondition(BP_CONDITION bpCondition)
        {
            throw new NotImplementedException();
        }

        // The sample engine does not support hit counts on breakpoints. A real-world debugger will want to keep track 
        // of how many times a particular bound breakpoint has been hit. The debugger calls SetHitCount when the user 
        // resets a breakpoint's hit count.
        int IDebugBoundBreakpoint2.SetHitCount(uint dwHitCount)
        {
            throw new NotImplementedException();
        }

        // The sample engine does not support pass counts on breakpoints.
        // This is used to specify the breakpoint hit count condition.
        int IDebugBoundBreakpoint2.SetPassCount(BP_PASSCOUNT bpPassCount)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}