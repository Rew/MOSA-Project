using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witschi.Debug.Engine.Host
{
    class ThreadContext
    {
        public ThreadContext()
        {
        }

        public uint EIP
        {
            get;
            set;
        }
    }
}
