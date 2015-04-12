using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witschi.Debug.Engine.AD7.Impl
{
    class FunctionSourceInfo
    {
        public Witschi.Debug.Metadata.DebugMethod DebugInfo
        {
            get;
            private set;
        }

        public Witschi.Debug.Metadata.DebugSource Source
        {
            get;
            private set;
        }

        public Witschi.Debug.Metadata.DebugLine Line
        {
            get;
            private set;
        }

        public FunctionSourceInfo(Witschi.Debug.Metadata.DebugMethod method, Witschi.Debug.Metadata.DebugSource source, Witschi.Debug.Metadata.DebugLine line)
        {
            this.DebugInfo = method;
            this.Source = source;
            this.Line = line;
        }
    }
}