using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Witschi.Debug.Engine.AD7.Impl
{
    // This class manages breakpoints for the engine. 
    class BreakpointManager
    {
        public AD7Engine Engine { get; private set; }
        List<AD7PendingBreakpoint> _pendingBreakpoints = new List<AD7PendingBreakpoint>();

        public BreakpointManager(AD7Engine aEngine)
        {
            Engine = aEngine;
        }

        // A helper method used to construct a new pending breakpoint.
        public void CreatePendingBreakpoint(IDebugBreakpointRequest2 pBPRequest, out IDebugPendingBreakpoint2 ppPendingBP)
        {
            var pendingBreakpoint = new AD7PendingBreakpoint(pBPRequest, this);
            ppPendingBP = (IDebugPendingBreakpoint2)pendingBreakpoint;
            _pendingBreakpoints.Add(pendingBreakpoint);
        }

        // Called from the engine's detach method to remove the debugger's breakpoint instructions.
        public void ClearBoundBreakpoints()
        {
            foreach (AD7PendingBreakpoint pendingBreakpoint in _pendingBreakpoints)
            {
                pendingBreakpoint.ClearBoundBreakpoints();
            }
        }
    }
}
