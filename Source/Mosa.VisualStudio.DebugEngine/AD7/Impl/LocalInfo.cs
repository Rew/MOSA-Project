using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witschi.Debug.Engine.AD7.Impl
{
    class LocalInfo
    {
        public LocalInfo(string name, ulong offset, Witschi.Compiler.IR.IType type)
        {
            this.Name = name;
            this.Offset = offset;
            this.Type = type;
        }

        public string Name
        {
            get;
            private set;
        }

        public ulong Offset
        {
            get;
            private set;
        }

        public Witschi.Compiler.IR.IType Type
        {
            get;
            private set;
        }
    }
}
