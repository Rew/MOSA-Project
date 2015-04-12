using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witschi.Debug.Engine.AD7;

namespace Witschi.Debug.Engine.Host
{
    abstract class DebugHost
    {
        class BreakpointData
        {
            public ulong Address
            {
                get;
                private set;
            }

            public List<AD7BoundBreakpoint> BreakPoints
            {
                get;
                private set;
            }

            public BreakpointData(ulong address)
            {
                Address = address;
                BreakPoints = new List<AD7BoundBreakpoint>();
            }
        }

        private bool _tempSuspend = false;
        private bool _stopping = false;
        private Dictionary<ulong, BreakpointData> _breakpoints = new Dictionary<ulong, BreakpointData>();
        private int _suspendCount = 0;
        protected bool IsSuspended
        {
            get { return _suspendCount > 0; }
        }

        public event Action Disconnected;
        protected void OnDisconnected()
        {
            if (Disconnected != null)
                Disconnected();
        }

        public event Action Suspended;
        protected void OnSuspended()
        {
            if (_suspendCount < 1)
                _suspendCount++;
            if (!_tempSuspend)
            {
                if (Suspended != null)
                    Suspended();
            }
        }

        public void Resume()
        {
            lock (this)
            {
                _suspendCount--;
                if (_suspendCount < 0)
                    _suspendCount = 0;
                if (_suspendCount > 0)
                    return;

                if (!_stopping)
                    DoResume();
            }
        }

        public void Suspend()
        {
            lock (this)
            {
                _suspendCount++;
                if (_suspendCount > 1)
                    return;

                if (!_stopping)
                    DoSuspend();
            }
        }

        public void LaunchSuspended(string strFile)
        {
            _suspendCount = 1;
            DoLaunchSuspended(strFile);
        }

        public void SetBreakpoint(ulong address, AD7BoundBreakpoint breakpoint)
        {
            _tempSuspend = true;
            try
            {
                Suspend();

                lock (_breakpoints)
                {
                    if (_breakpoints.ContainsKey(address))
                        _breakpoints[address].BreakPoints.Add(breakpoint);
                    else
                    {
                        BreakpointData d = new BreakpointData(address);
                        d.BreakPoints.Add(breakpoint);
                        _breakpoints.Add(address, d);
                        CreateBreakpoint(address);
                    }
                }

                Resume();
            }
            finally
            {
                _tempSuspend = false;
            }
        }

        public void RemoveBreakpoint(ulong address, AD7BoundBreakpoint breakpoint)
        {
            try
            {
                _tempSuspend = true;
                Suspend();

                lock (_breakpoints)
                {
                    if (_breakpoints.ContainsKey(address))
                    {
                        _breakpoints[address].BreakPoints.Remove(breakpoint);
                        if (_breakpoints[address].BreakPoints.Count < 1)
                        {
                            _breakpoints.Remove(address);
                            if (!_stopping)
                                DeleteBreakpoint(address);
                        }
                    }
                }

                Resume();
            }
            finally
            {
                _tempSuspend = false;
            }
        }

        public abstract void Attach();
        public abstract void Detach();
        protected abstract void CreateBreakpoint(ulong addr);
        protected abstract void DeleteBreakpoint(ulong addr);
        protected abstract void DoLaunchSuspended(string strFile);
        protected abstract void DoResume();
        protected abstract void DoSuspend();
        public void Terminate()
        {
            _stopping = true;
            DoTerminate();
        }
        protected abstract void DoTerminate();

        public abstract bool IsAttached { get; }
        public abstract bool IsRunning { get; }

        public abstract byte[] ReadMemory(ulong address, ulong length);
        public abstract x86StackContext[] StackFrames { get; }
        public abstract Register[] Registers { get; }
        public abstract bool WalkStack(AD7Thread thread);
    }
}
