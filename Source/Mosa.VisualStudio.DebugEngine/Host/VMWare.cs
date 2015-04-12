using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witschi.Debug.Engine.Host
{
    class VMWare : DebugHost
    {
        public override void Attach()
        {
            throw new NotImplementedException();
        }

        public override void Detach()
        {
            throw new NotImplementedException();
        }

        protected override void CreateBreakpoint(ulong addr)
        {
            throw new NotImplementedException();
        }

        protected override void DeleteBreakpoint(ulong addr)
        {
            throw new NotImplementedException();
        }

        protected override void DoLaunchSuspended(string strFile)
        {
            throw new NotImplementedException();
        }

        protected override void DoResume()
        {
            throw new NotImplementedException();
        }

        protected override void DoSuspend()
        {
            throw new NotImplementedException();
        }

        protected override void DoTerminate()
        {
            throw new NotImplementedException();
        }

        public override bool IsAttached
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsRunning
        {
            get { throw new NotImplementedException(); }
        }

        public override byte[] ReadMemory(ulong address, ulong length)
        {
            throw new NotImplementedException();
        }

        public override x86StackContext[] StackFrames
        {
            get { throw new NotImplementedException(); }
        }

        public override Register[] Registers
        {
            get { throw new NotImplementedException(); }
        }

        public override bool WalkStack(AD7.AD7Thread thread)
        {
            throw new NotImplementedException();
        }
    }
}
