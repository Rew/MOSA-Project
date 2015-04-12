using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Witschi.Debug.Engine.AD7.Impl;

namespace Witschi.Debug.Engine.AD7
{
    class AD7PendingBreakpoint : IDebugPendingBreakpoint2
    {
        IDebugBreakpointRequest2 _bpRequest;
        BP_REQUEST_INFO _bpRequestInfo;
        BreakpointManager _breakpointManager;
        List<AD7BoundBreakpoint> _boundBreakpoints = new List<AD7BoundBreakpoint>();

        bool _enabled = false;
        bool _deleted = false;

        public AD7PendingBreakpoint(IDebugBreakpointRequest2 request, BreakpointManager mgr)
        {
            this._breakpointManager = mgr;
            this._bpRequest = request;

            BP_REQUEST_INFO[] info = new BP_REQUEST_INFO[1];
            EngineUtils.CheckOk(_bpRequest.GetRequestInfo(enum_BPREQI_FIELDS.BPREQI_BPLOCATION, info));
            this._bpRequestInfo = info[0];
        }

        // Get the document context for this pending breakpoint. A document context is a abstract representation of a source file 
        // location.
        public AD7DocumentContext GetDocumentContext(AD7Program program, ulong address)
        {
            IDebugDocumentPosition2 docPosition = (IDebugDocumentPosition2)(Marshal.GetObjectForIUnknown(_bpRequestInfo.bpLocation.unionmember2));
            string documentName;
            EngineUtils.CheckOk(docPosition.GetFileName(out documentName));

            // Get the location in the document that the breakpoint is in.
            TEXT_POSITION[] startPosition = new TEXT_POSITION[1];
            TEXT_POSITION[] endPosition = new TEXT_POSITION[1];
            EngineUtils.CheckOk(docPosition.GetRange(startPosition, endPosition));

            AD7MemoryAddress codeContext = new AD7MemoryAddress(program, address);

            return new AD7DocumentContext(documentName, startPosition[0], startPosition[0], codeContext);
        }

        public void ClearBoundBreakpoints()
        {
            lock (_boundBreakpoints)
            {
                for (int i = _boundBreakpoints.Count - 1; i >= 0; i--)
                {
                    ((IDebugBoundBreakpoint2)_boundBreakpoints[i]).Delete();
                }
            }
        }

        // Called by bound breakpoints when they are being deleted.
        public void OnBoundBreakpointDeleted(AD7BoundBreakpoint boundBreakpoint)
        {
            lock (_boundBreakpoints)
            {
                _boundBreakpoints.Remove(boundBreakpoint);
            }
        }

        bool CanBind()
        {
            // The sample engine only supports breakpoints on a file and line number. No other types of breakpoints are supported.
            if (this._deleted || _bpRequestInfo.bpLocation.bpLocationType != (uint)enum_BP_LOCATION_TYPE.BPLT_CODE_FILE_LINE)
            {
                return false;
            }

            return true;
        }

        int IDebugPendingBreakpoint2.Bind()
        {
            try
            {
                if (CanBind())
                {
                    IDebugDocumentPosition2 docPosition = (IDebugDocumentPosition2)(Marshal.GetObjectForIUnknown(_bpRequestInfo.bpLocation.unionmember2));

                    string documentName;
                    EngineUtils.CheckOk(docPosition.GetFileName(out documentName));

                    // Get the location in the document that the breakpoint is in.
                    TEXT_POSITION[] startPosition = new TEXT_POSITION[1];
                    TEXT_POSITION[] endPosition = new TEXT_POSITION[1];
                    EngineUtils.CheckOk(docPosition.GetRange(startPosition, endPosition));

                    foreach (AD7Process process in _breakpointManager.Engine.ListProcesses())
                    {
                        // Ask the symbol engine to find all addresses in all modules with symbols that match this source and line number.
                        ulong[] addresses = process.SymbolEngine.GetAddressesForSourceLocation(documentName, startPosition[0].dwLine + 1, startPosition[0].dwColumn + 1);
                        lock (_boundBreakpoints)
                        {
                            foreach (ulong addr in addresses)
                            {
                                AD7BreakpointResolution breakpointResolution = new AD7BreakpointResolution(process.Program, addr, GetDocumentContext(process.Program, addr));
                                AD7BoundBreakpoint boundBreakpoint = new AD7BoundBreakpoint(process, addr, this, breakpointResolution);
                                _boundBreakpoints.Add(boundBreakpoint);
                                process.Host.SetBreakpoint(addr, boundBreakpoint);
                            }
                        }
                    }


                        return VSConstants.S_OK;
                }
                else
                {
                    // The breakpoint could not be bound. This may occur for many reasons such as an invalid location, an invalid expression, etc...
                    // The sample engine does not support this, but a real world engine will want to send an instance of IDebugBreakpointErrorEvent2 to the
                    // UI and return a valid instance of IDebugErrorBreakpoint2 from IDebugPendingBreakpoint2::EnumErrorBreakpoints. The debugger will then
                    // display information about why the breakpoint did not bind to the user.
                    return VSConstants.S_FALSE;
                }
            }
            catch (Exception e)
            {
                return EngineUtils.UnexpectedException(e);
            }
        }

        int IDebugPendingBreakpoint2.CanBind(out IEnumDebugErrorBreakpoints2 ppErrorEnum)
        {
            ppErrorEnum = null;
            return VSConstants.S_FALSE;
        }

        int IDebugPendingBreakpoint2.Delete()
        {
            lock (_boundBreakpoints)
            {
                for (int i = _boundBreakpoints.Count - 1; i >= 0; i--)
                {
                    ((IDebugBoundBreakpoint2)_boundBreakpoints[i]).Delete();
                }
            }
            _deleted = true;

            return VSConstants.S_OK;
        }

        int IDebugPendingBreakpoint2.Enable(int fEnable)
        {
            lock (_boundBreakpoints)
            {
                _enabled = fEnable == 0 ? false : true;

                foreach (AD7BoundBreakpoint bp in _boundBreakpoints)
                {
                    ((IDebugBoundBreakpoint2)_boundBreakpoints).Enable(fEnable);
                }
            }

            return VSConstants.S_OK;
        }

        int IDebugPendingBreakpoint2.EnumBoundBreakpoints(out IEnumDebugBoundBreakpoints2 ppEnum)
        {
            lock (_boundBreakpoints)
            {
                ppEnum = new AD7BoundBreakpointsEnum(_boundBreakpoints.Cast<IDebugBoundBreakpoint2>().ToArray());
            }
            return VSConstants.S_OK;
        }

        int IDebugPendingBreakpoint2.EnumErrorBreakpoints(enum_BP_ERROR_TYPE bpErrorType, out IEnumDebugErrorBreakpoints2 ppEnum)
        {
            throw new NotImplementedException();
        }

        int IDebugPendingBreakpoint2.GetBreakpointRequest(out IDebugBreakpointRequest2 ppBPRequest)
        {
            throw new NotImplementedException();
        }

        int IDebugPendingBreakpoint2.GetState(PENDING_BP_STATE_INFO[] pState)
        {
            if (_deleted)
            {
                pState[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_DELETED;
            }
            else if (_enabled)
            {
                pState[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_ENABLED;
            }
            pState[0].state = (enum_PENDING_BP_STATE)enum_BP_STATE.BPS_DISABLED;

            return VSConstants.S_OK;
        }

        int IDebugPendingBreakpoint2.SetCondition(BP_CONDITION bpCondition)
        {
            throw new NotImplementedException();
        }

        int IDebugPendingBreakpoint2.SetPassCount(BP_PASSCOUNT bpPassCount)
        {
            throw new NotImplementedException();
        }

        int IDebugPendingBreakpoint2.Virtualize(int fVirtualize)
        {
            return VSConstants.E_NOTIMPL;
        }
    }
}
