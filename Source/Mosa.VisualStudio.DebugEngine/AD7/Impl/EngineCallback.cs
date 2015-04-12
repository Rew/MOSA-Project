using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.IO;

namespace Witschi.Debug.Engine.AD7.Impl
{
    class EngineCallback
    {
        readonly IDebugEventCallback2 _ad7Callback;
        readonly AD7Engine _engine;

        public EngineCallback(AD7Engine engine, IDebugEventCallback2 ad7Callback)
        {
            _ad7Callback = ad7Callback;
            _engine = engine;
        }

        private void Send(IDebugEvent2 eventObject, string iidEvent, IDebugProcess2 process, IDebugProgram2 program, IDebugThread2 thread)
        {
            uint attributes;
            var riidEvent = new Guid(iidEvent);

            EngineUtils.RequireOk(eventObject.GetAttributes(out attributes));
            EngineUtils.RequireOk(_ad7Callback.Event(_engine, process, program, thread, eventObject, ref riidEvent, attributes));
        }

        public void Send(IDebugEvent2 eventObject, string iidEvent, IDebugProcess2 process)
        {
            Send(eventObject, iidEvent, process, null, null);
        }

        public void Send(IDebugEvent2 eventObject, string iidEvent, IDebugProgram2 program, IDebugThread2 thread)
        {
            Send(eventObject, iidEvent, null, program, thread);
        }

        /*public void OnError(int hrErr) {
          //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);

          // IDebugErrorEvent2 is used to report error messages to the user when something goes wrong in the debug engine.
          // The sample engine doesn't take advantage of this.
        }
        */
        public void OnModuleLoad(AD7Module aModule)
        {
            AD7ModuleLoadEvent eventObject = new AD7ModuleLoadEvent(aModule, true);
            Send(eventObject, AD7ModuleLoadEvent.IID, null, null);
        }

        public void OnModuleUnload(AD7Module module)
        {
            AD7ModuleLoadEvent eventObject = new AD7ModuleLoadEvent(module, false);

            Send(eventObject, AD7ModuleLoadEvent.IID, null, null);
        }
        /*
         // Call this one for internal Cosmos dev.
         // Can be turned off and should be turned off by default. Use an IFDEF or something.
         public void OnOutputString(string outputString) {
           if (false) {
             //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);
             var eventObject = new AD7OutputDebugStringEvent(outputString);
             Send(eventObject, AD7OutputDebugStringEvent.IID, null);
           }
         }

         // This is the user version, messages directly from Cosmos user code
         public void OnOutputStringUser(string outputString) {
           //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);
           var eventObject = new AD7OutputDebugStringEvent(outputString);
           Send(eventObject, AD7OutputDebugStringEvent.IID, null);
         }
        */

        public void OnProcessDestroy(AD7Process process)
        {
            AD7ProcessDestroyEvent.Send(_engine, process);
        }

        public void OnProgramExit(AD7Program program, uint exitCode)
        {
            AD7ProgramDestroyEvent eventObject = new AD7ProgramDestroyEvent(exitCode);
            Send(eventObject, AD7ProgramDestroyEvent.IID, program, null);
        }

        /*
         public void OnThreadExit() { //DebuggedThread debuggedThread, uint exitCode)
           //System.Diagnostics.Debug.Assert(Worker.CurrentThreadId == m_engine.DebuggedProcess.PollThreadId);

           //AD7Thread ad7Thread = (AD7Thread)debuggedThread.Client;
           //System.Diagnostics.Debug.Assert(ad7Thread != null);

           //AD7ThreadDestroyEvent eventObject = new AD7ThreadDestroyEvent(exitCode);

           //Send(eventObject, AD7ThreadDestroyEvent.IID, ad7Thread);
         }*/

        public void OnThreadStart(AD7Thread thread)
        {
            AD7ThreadCreateEvent eventObject = new AD7ThreadCreateEvent();
            IDebugProgram2 program;
            ((IDebugThread2)thread).GetProgram(out program);
            Send(eventObject, AD7ThreadCreateEvent.IID, program, thread);
        }

        /*
         public void OnBreak(AD7Thread aThread) {
           var mBreak = new AD7BreakEvent();
           Send(mBreak, AD7BreakEvent.IID, aThread);
         }

         public void OnBreakpoint(AD7Thread thread, IList<IDebugBoundBreakpoint2> clients) {
           var boundBreakpoints = new IDebugBoundBreakpoint2[clients.Count];
           int i = 0;
           foreach (var objCurrentBreakpoint in clients) {
             boundBreakpoints[i] = objCurrentBreakpoint;
             i++;
           }

           // An engine that supports more advanced breakpoint features such as hit counts, conditions and filters
           // should notify each bound breakpoint that it has been hit and evaluate conditions here.
           // The sample engine does not support these features.
           var boundBreakpointsEnum = new AD7BoundBreakpointsEnum(boundBreakpoints);
           var eventObject = new AD7BreakpointEvent(boundBreakpointsEnum);
           var ad7Thread = (AD7Thread)thread;
           Send(eventObject, AD7BreakpointEvent.IID, ad7Thread);
         }

         public void OnException() { //DebuggedThread thread, uint code)
           // Exception events are sent when an exception occurs in the debuggee that the debugger was not expecting.
           // The sample engine does not support these.
           throw new Exception("The method or operation is not implemented.");
         }

         public void OnStepComplete() { //DebuggedThread thread)
           AD7StepCompletedEvent.Send(m_engine);
         }
        */

        public void OnAsyncBreakComplete(AD7Thread thread)
        {
            AD7AsyncBreakCompleteEvent eventObject = new AD7AsyncBreakCompleteEvent();
            Send(eventObject, AD7AsyncBreakCompleteEvent.IID, thread.Program, thread);
        }
        
        public void OnLoadComplete(AD7Thread thread)
        {
            AD7LoadCompleteEvent eventObject = new AD7LoadCompleteEvent();
            IDebugProgram2 program;
            ((IDebugThread2)thread).GetProgram(out program);
            Send(eventObject, AD7LoadCompleteEvent.IID, program, thread);
        }
        /*
         public void OnProgramDestroy(uint exitCode) {
           AD7ProgramDestroyEvent eventObject = new AD7ProgramDestroyEvent(exitCode);
           Send(eventObject, AD7ProgramDestroyEvent.IID, null);
         }
        */
         // Engines notify the debugger about the results of a symbol serach by sending an instance of IDebugSymbolSearchEvent2
        public void OnSymbolSearch(AD7Module module, string status, enum_MODULE_INFO_FLAGS dwStatusFlags)
        {
            string statusString = (dwStatusFlags == enum_MODULE_INFO_FLAGS.MIF_SYMBOLS_LOADED ? "Symbols Loaded - " : "No symbols loaded") + status;

            AD7SymbolSearchEvent eventObject = new AD7SymbolSearchEvent(module, statusString, (uint)dwStatusFlags);
            Send(eventObject, AD7SymbolSearchEvent.IID, module.Program, null);
        }
        /*
         // Engines notify the debugger that a breakpoint has bound through the breakpoint bound event.
         public void OnBreakpointBound(object objBoundBreakpoint, uint address) {
           AD7BoundBreakpoint boundBreakpoint = (AD7BoundBreakpoint)objBoundBreakpoint;
           IDebugPendingBreakpoint2 pendingBreakpoint;
           ((IDebugBoundBreakpoint2)boundBreakpoint).GetPendingBreakpoint(out pendingBreakpoint);

           AD7BreakpointBoundEvent eventObject = new AD7BreakpointBoundEvent((AD7PendingBreakpoint)pendingBreakpoint, boundBreakpoint);
           Send(eventObject, AD7BreakpointBoundEvent.IID, null);
         }
           */
    }

    //internal class AD7BreakEvent : AD7StoppingEvent, IDebugBreakEvent2 {
    //    public const string IID = "C7405D1D-E24B-44E0-B707-D8A5A4E1641B";
    //}
}
