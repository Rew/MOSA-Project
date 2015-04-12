using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witschi.Debug.Engine.Host
{
    class Register
    {
        public string Name
        {
            get;
            private set;
        }

        public ulong Value
        {
            get;
            private set;
        }

        public Register(string name, ulong value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
