using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witschi.Debug.Engine.AD7
{
    class AD7ConstantProperty : IDebugProperty2
    {
        AD7Program prog;
        ulong val;

        public AD7ConstantProperty(AD7Program program, ulong value)
        {
            prog = program;
            val = value;
        }

        int IDebugProperty2.EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum)
        {
            throw new NotImplementedException();
        }

        int IDebugProperty2.GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost)
        {
            throw new NotImplementedException();
        }

        int IDebugProperty2.GetExtendedInfo(ref Guid guidExtendedInfo, out object pExtendedInfo)
        {
            throw new NotImplementedException();
        }

        int IDebugProperty2.GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            throw new NotImplementedException();
        }

        int IDebugProperty2.GetMemoryContext(out IDebugMemoryContext2 ppMemory)
        {
            ppMemory = new AD7MemoryAddress(prog, val);
            return VSConstants.S_OK;
        }

        int IDebugProperty2.GetParent(out IDebugProperty2 ppParent)
        {
            throw new NotImplementedException();
        }

        int IDebugProperty2.GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo)
        {
            DEBUG_PROPERTY_INFO pi = new DEBUG_PROPERTY_INFO();
            if((dwFields | enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME) != 0)
            {
                pi.bstrName = val.ToString();
                pi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
            }
            if ((dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE) != 0)
            {
                pi.bstrValue = val.ToString("X");
                pi.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
            }
            pPropertyInfo[0] = pi;
            return VSConstants.S_OK;
        }

        int IDebugProperty2.GetReference(out IDebugReference2 ppReference)
        {
            throw new NotImplementedException();
        }

        int IDebugProperty2.GetSize(out uint pdwSize)
        {
            throw new NotImplementedException();
        }

        int IDebugProperty2.SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue, uint dwTimeout)
        {
            throw new NotImplementedException();
        }

        int IDebugProperty2.SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout)
        {
            throw new NotImplementedException();
        }
    }
}
