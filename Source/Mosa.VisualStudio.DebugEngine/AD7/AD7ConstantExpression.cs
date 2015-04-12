using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witschi.Debug.Engine.AD7
{
    class AD7ConstantExpression : IDebugExpression2
    {
        AD7Program program;
        ulong val;

        public AD7ConstantExpression(AD7Program prog, ulong value)
        {
            program = prog;
            val = value;
        }

        int IDebugExpression2.Abort()
        {
            throw new NotImplementedException();
        }

        int IDebugExpression2.EvaluateAsync(enum_EVALFLAGS dwFlags, IDebugEventCallback2 pExprCallback)
        {
            throw new NotImplementedException();
        }

        int IDebugExpression2.EvaluateSync(enum_EVALFLAGS dwFlags, uint dwTimeout, IDebugEventCallback2 pExprCallback, out IDebugProperty2 ppResult)
        {
            ppResult = new AD7ConstantProperty(program, val);
            return VSConstants.S_OK;
        }
    }
}
