using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witschi.Debug.Engine.AD7.Impl;
using Witschi.Debug.Engine.Host;
using Witschi.Debug.Metadata;

namespace Witschi.Debug.Engine.AD7
{
    class AD7StackFrame : IDebugStackFrame2, IDebugExpressionContext2
    {
        private AD7Thread _thread;
        private x86StackContext _context;
        private Impl.FunctionSourceInfo _sourceInfo;

        public AD7StackFrame(AD7Thread thread, x86StackContext context)
        {
            _thread = thread;
            _context = context;

            _sourceInfo = thread.Program.Process.SymbolEngine.GetInfoForAddress(context.eip);
        }

        public FRAMEINFO GetFrame(enum_FRAMEINFO_FLAGS flags)
        {
            FRAMEINFO fi = new FRAMEINFO();
            AD7Module module = _thread.Program.Process.PrimaryModule;

            if ((flags & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME) == enum_FRAMEINFO_FLAGS.FIF_FUNCNAME)
            {
                if (_sourceInfo != null)
                {
                    fi.m_bstrFuncName = "";

                    if (((flags & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_MODULE) == enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_MODULE) && (module != null))
                        fi.m_bstrFuncName = module.Name + "!";

                    fi.m_bstrFuncName += _sourceInfo.DebugInfo.Name;

                    if ((flags & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_ARGS) != 0 && (_sourceInfo.DebugInfo.Parameters.Count > 0))
                    {
                        throw new NotImplementedException();
                    }

                    if ((flags & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_LINES) != 0)
                    {
                        if (_sourceInfo.Line != null)
                            fi.m_bstrFuncName += " Line:" + _sourceInfo.Line.LineBegin.ToString();
                    }
                }
                else
                {
                    if (((flags & enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_MODULE) == enum_FRAMEINFO_FLAGS.FIF_FUNCNAME_MODULE) && (module != null))
                        fi.m_bstrFuncName = module.Name + "!0x" + _context.eip.ToString("X");
                    else
                        fi.m_bstrFuncName = "0x" + _context.eip.ToString("X");
                }
                fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME;
            }
            if (((flags & enum_FRAMEINFO_FLAGS.FIF_MODULE) == enum_FRAMEINFO_FLAGS.FIF_MODULE) && (module != null))
            {
                fi.m_bstrModule = module.Name;
                fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_MODULE;
            }
            if ((flags & enum_FRAMEINFO_FLAGS.FIF_STACKRANGE) == enum_FRAMEINFO_FLAGS.FIF_STACKRANGE)
            {
                fi.m_addrMin = _context.esp;
                fi.m_addrMax = _context.ebp;
                fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_STACKRANGE;
            }
            if ((flags & enum_FRAMEINFO_FLAGS.FIF_FRAME) == enum_FRAMEINFO_FLAGS.FIF_FRAME)
            {
                fi.m_pFrame = this;
                fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FRAME;
            }
            if ((flags & enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO) == enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO)
            {
                fi.m_fHasDebugInfo = (_sourceInfo != null) ? 1 : 0;
                fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO;
            }
            if ((flags & enum_FRAMEINFO_FLAGS.FIF_STALECODE) == enum_FRAMEINFO_FLAGS.FIF_STALECODE)
            {
                fi.m_fStaleCode = 0;
                fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_STALECODE;
            }
            if ((flags & enum_FRAMEINFO_FLAGS.FIF_DEBUG_MODULEP) == enum_FRAMEINFO_FLAGS.FIF_DEBUG_MODULEP)
            {
                if (module != null)
                {
                    fi.m_pModule = module;
                    fi.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_DEBUG_MODULEP;
                }
            }
            return fi;
        }

        int IDebugStackFrame2.EnumProperties(enum_DEBUGPROP_INFO_FLAGS dwFields, uint nRadix, ref Guid guidFilter, uint dwTimeout, out uint pcelt, out IEnumDebugPropertyInfo2 ppEnum)
        {
            pcelt = 0;
            ppEnum = null;
            List<DEBUG_PROPERTY_INFO> props = new List<DEBUG_PROPERTY_INFO>();

            if (guidFilter == AD7Guids.guidFilterArgs)
            {
                return VSConstants.E_NOTIMPL;
            }
            else if (guidFilter == AD7Guids.guidFilterLocals)
            {
                return VSConstants.E_NOTIMPL;
            }
            else if (guidFilter == AD7Guids.guidFilterLocalsPlusArgs)
            {
                props.AddRange(CreateParams());
                props.AddRange(CreateLocals());
            }
            else if (guidFilter == AD7Guids.guidFilterAllLocals)
            {
                return VSConstants.E_NOTIMPL;
            }
            else if (guidFilter == AD7Guids.guidFilterAllLocalsPlusArgs)
            {
                return VSConstants.E_NOTIMPL;
            }
            else if (guidFilter == AD7Guids.guidFilterRegisters)
            {
                props.AddRange(CreateRegisters());
            }

            ppEnum = new AD7PropertyInfoEnum(props.ToArray());
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetCodeContext(out IDebugCodeContext2 ppCodeCxt)
        {
            ppCodeCxt = new AD7MemoryAddress(_thread.Program, _context.eip);
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            throw new NotImplementedException();
        }

        int IDebugStackFrame2.GetDocumentContext(out IDebugDocumentContext2 ppCxt)
        {
            if (_sourceInfo != null)
            {
                TEXT_POSITION begin = new TEXT_POSITION();
                begin.dwColumn = (uint)(_sourceInfo.Line.ColBegin - 1);
                begin.dwLine = _sourceInfo.Line.LineBegin - 1;

                TEXT_POSITION end = new TEXT_POSITION();
                end.dwColumn = (uint)(_sourceInfo.Line.ColEnd - 1);
                end.dwLine = _sourceInfo.Line.LineEnd - 1;

                ppCxt = new AD7DocumentContext(_sourceInfo.Source.File, begin, end, new AD7MemoryAddress(_thread.Program, _context.eip));
                return VSConstants.S_OK;
            }
            else
            {
                ppCxt = null;
                return VSConstants.S_FALSE;
            }
        }

        int IDebugStackFrame2.GetExpressionContext(out IDebugExpressionContext2 ppExprCxt)
        {
            ppExprCxt = (IDebugExpressionContext2)this;
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, FRAMEINFO[] pFrameInfo)
        {
            pFrameInfo[0] = GetFrame(dwFieldSpec);
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            pbstrLanguage = "WC#";
            pguidLanguage = new Guid("e17f3766-36a2-4022-9b88-7771e1aba163");
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetName(out string pbstrName)
        {
            pbstrName = GetFrame(enum_FRAMEINFO_FLAGS.FIF_FUNCNAME).m_bstrFuncName;
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetPhysicalStackRange(out ulong paddrMin, out ulong paddrMax)
        {
            paddrMin = _context.esp;
            paddrMax = _context.ebp;
            return VSConstants.S_OK;
        }

        int IDebugStackFrame2.GetThread(out IDebugThread2 ppThread)
        {
            ppThread = _thread;
            return VSConstants.S_OK;
        }

        #region IDebugExpressionContext2

        int IDebugExpressionContext2.GetName(out string pbstrName)
        {
            throw new NotImplementedException();
        }

        int IDebugExpressionContext2.ParseText(string pszCode, enum_PARSEFLAGS dwFlags, uint nRadix, out IDebugExpression2 ppExpr, out string pbstrError, out uint pichError)
        {
            pbstrError = "";
            pichError = 0;
            ppExpr = null;

            if (_sourceInfo.DebugInfo.Locals.Count > 0)
            {
                LocalInfo[] locs = _thread.Program.Process.SymbolEngine.CreateLocals(_sourceInfo, this);
                for (int i = 0; i < locs.Length; i++)
                {
                    if (locs[i].Name == pszCode)
                    {
                        // ppExpr = new localex 
                    }
                }
            }
            if (_sourceInfo.DebugInfo.Parameters.Count > 0)
            {
                foreach (DebugParameter p in _sourceInfo.DebugInfo.Parameters)
                {

                }
            }
            try
            {
                ulong l;
                if (pszCode.StartsWith("0x"))
                    l = ulong.Parse(pszCode.Substring(2), System.Globalization.NumberStyles.HexNumber);
                else
                    l = ulong.Parse(pszCode);
                ppExpr = new AD7ConstantExpression(_thread.Program, l);
                return VSConstants.S_OK;
            }
            catch { }
            pbstrError = "Invalid Expression";
            pichError = (uint)pbstrError.Length;
            return VSConstants.S_FALSE;
        }

        #endregion

        List<DEBUG_PROPERTY_INFO> CreateParams()
        {
            return new List<DEBUG_PROPERTY_INFO>();
        }

        List<DEBUG_PROPERTY_INFO> CreateLocals()
        {
            List<DEBUG_PROPERTY_INFO> locals = new List<DEBUG_PROPERTY_INFO>();
            LocalInfo[] locs = _thread.Program.Process.SymbolEngine.CreateLocals(_sourceInfo, this);
            for (int i = 0; i < locs.Length; i++)
            {
                locals.Add(new Variable(_thread, locs[i].Name, locs[i].Type, (uint)(_context.ebp - (uint)locs[i].Offset)).ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ALL));
            }
            return locals;
        }

        List<DEBUG_PROPERTY_INFO> CreateRegisters()
        {
            List<DEBUG_PROPERTY_INFO> cats = new List<DEBUG_PROPERTY_INFO>();
            cats.Add(new RegisterCategory("CPU", _thread.Program.Process.Host.Registers).ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ALL));
            return cats;
        }

        /*List<Register> CreateRegisters()
        {
            List<Register> regs = new List<Register>();
            foreach (Host.Register r in _thread.Program.Process.Host.Registers)
            {
                regs.Add(new Register(r));
            }
            return regs;
        }*/

        class RegisterCategory : IDebugProperty2
        {
            List<Register> regs = new List<Register>();
            string _name;

            public RegisterCategory(string name, IEnumerable<Host.Register> registers)
            {
                _name = name;
                foreach (Host.Register r in registers)
                {
                    regs.Add(new Register(r));
                }
            }

            public DEBUG_PROPERTY_INFO ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields)
            {
                DEBUG_PROPERTY_INFO propertyInfo = new DEBUG_PROPERTY_INFO();

                if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME) != 0)
                {
                    propertyInfo.bstrName = _name;
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
                }

                if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB) != 0)
                {
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB;
                    // The sample does not support writing of values displayed in the debugger, so mark them all as read-only.
                    propertyInfo.dwAttrib = enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_READONLY | enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE | enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_AUTOEXPANDED;
                }

                if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP) != 0)
                {
                    propertyInfo.pProperty = (IDebugProperty2)this;
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP;
                }
                return propertyInfo;
            }

            int IDebugProperty2.EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum)
            {
                List<DEBUG_PROPERTY_INFO> lst = new List<DEBUG_PROPERTY_INFO>();
                foreach (Register r in regs)
                {
                    lst.Add(r.ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ALL));
                }
                ppEnum = new AD7PropertyEnum(lst.ToArray());
                return VSConstants.S_OK;
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
                throw new NotImplementedException();
            }

            int IDebugProperty2.GetParent(out IDebugProperty2 ppParent)
            {
                throw new NotImplementedException();
            }

            int IDebugProperty2.GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo)
            {
                pPropertyInfo[0] = ConstructDebugPropertyInfo(dwFields);
                rgpArgs = null;
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

        class Register : IDebugProperty2
        {
            private Host.Register _reg;

            public Register(Host.Register r)
            {
                _reg = r;
            }

            // Construct a DEBUG_PROPERTY_INFO representing this local or parameter.
            public DEBUG_PROPERTY_INFO ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields)
            {
                DEBUG_PROPERTY_INFO propertyInfo = new DEBUG_PROPERTY_INFO();

                if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME) != 0)
                {
                    propertyInfo.bstrName = _reg.Name;
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
                }

                if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE) != 0)
                {
                    propertyInfo.bstrValue = _reg.Value.ToString("X8");
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
                }

                if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB) != 0)
                {
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB;
                    // The sample does not support writing of values displayed in the debugger, so mark them all as read-only.
                    propertyInfo.dwAttrib = enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_READONLY;

                    /*if (this.m_variableInformation.child != null)
                    {
                        propertyInfo.dwAttrib |= (enum_DBG_ATTRIB_FLAGS)DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
                    }*/
                }

                if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP) != 0)
                {
                    propertyInfo.pProperty = this;
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP;
                }

                return propertyInfo;
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
                throw new NotImplementedException();
            }

            int IDebugProperty2.GetParent(out IDebugProperty2 ppParent)
            {
                throw new NotImplementedException();
            }

            int IDebugProperty2.GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo)
            {
                pPropertyInfo[0] = ConstructDebugPropertyInfo(dwFields);
                rgpArgs = null;
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

        class Variable : IDebugProperty2
        {
            AD7Thread _thread;
            string _name;
            Compiler.IR.IType _type;
            uint _addr;

            public Variable(AD7Thread thread, string name, Witschi.Compiler.IR.IType type, uint addr)
            {
                _thread = thread;
                _name = name;
                _type = type;
                _addr = addr;
            }

            // Construct a DEBUG_PROPERTY_INFO representing this local or parameter.
            public DEBUG_PROPERTY_INFO ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields)
            {
                DEBUG_PROPERTY_INFO propertyInfo = new DEBUG_PROPERTY_INFO();

                if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME) != 0)
                {
                    propertyInfo.bstrFullName = _name;
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME;
                }

                if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME) != 0)
                {
                    propertyInfo.bstrName = _name;
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
                }

                if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE) != 0)
                {
                    propertyInfo.bstrType = _type != null ? _type.Fullname : "Unknown Type";
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
                }

                if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE) != 0)
                {
                    propertyInfo.bstrValue = GetValue();
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
                }

                if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB) != 0)
                {
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB;
                    // The sample does not support writing of values displayed in the debugger, so mark them all as read-only.
                    propertyInfo.dwAttrib = enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_READONLY;

                    if ((_type != null) && ((_type.IRType == CLI.ElementType.Object) || (_type.IRType == CLI.ElementType.ValueType)))
                    {
                        propertyInfo.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
                    }
                }

                // If the debugger has asked for the property, or the property has children (meaning it is a pointer in the sample)
                // then set the pProperty field so the debugger can call back when the chilren are enumerated.
                if (((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP) != 0) || (false/*this.m_variableInformation.child != null*/))
                {
                    propertyInfo.pProperty = (IDebugProperty2)this;
                    propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP;
                }

                return propertyInfo;
            }


            int IDebugProperty2.EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter, enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout, out IEnumDebugPropertyInfo2 ppEnum)
            {
                List<DEBUG_PROPERTY_INFO> lst = new List<DEBUG_PROPERTY_INFO>();
                if (_type != null)
                {
                    foreach(Compiler.IR.IField fld in _type.DefinedFields)
                    {
                        lst.Add(new Variable(_thread, fld.Name, fld.Type, _addr + (fld as Compiler.TypeSys.CompilerField).Offset).ConstructDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ALL));
                    }
                }

                ppEnum = new AD7PropertyInfoEnum(lst.ToArray());
                return VSConstants.S_OK;
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
                throw new NotImplementedException();
            }

            int IDebugProperty2.GetParent(out IDebugProperty2 ppParent)
            {
                throw new NotImplementedException();
            }

            int IDebugProperty2.GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout, IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo)
            {
                pPropertyInfo[0] = ConstructDebugPropertyInfo(dwFields);
                rgpArgs = null;
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

            string GetValue()
            {
                if (_type == null)
                    return "<Unknown Value>";

                byte[] b = _thread.Program.Process.Host.ReadMemory(_addr, 8); // read enough for all native types
                switch (_type.IRType)
                {
                    case CLI.ElementType.I1:
                        return ((sbyte)b[0]).ToString();
                    case CLI.ElementType.U1:
                        return b[0].ToString();
                    case CLI.ElementType.I2:
                        short s = BitConverter.ToInt16(b, 0);
                        return s.ToString();
                    case CLI.ElementType.U2:
                        ushort us = BitConverter.ToUInt16(b, 0);
                        return us.ToString();
                    case CLI.ElementType.I4:
                    case CLI.ElementType.I:
                        int i = BitConverter.ToInt32(b, 0);
                        return i.ToString();
                    case CLI.ElementType.U4:
                    case CLI.ElementType.U:
                        uint ui = BitConverter.ToUInt32(b, 0);
                        return ui.ToString();
                    case CLI.ElementType.Object:
                        if (_type.Fullname == "System.String")
                        {
                            uint strPtr = BitConverter.ToUInt32(b, 0);
                            b = _thread.Program.Process.Host.ReadMemory(strPtr, 8);
                            int len = BitConverter.ToInt32(b, 4);
                            string str = "";
                            if (len < 255)
                            {
                                b = _thread.Program.Process.Host.ReadMemory((ulong)strPtr + (ulong)8, (ulong)len * 2);

                                for (int j = 0; j < len; j++)
                                {
                                    str += (char)(((int)b[(j * 2) + 1] << 8) + b[j * 2]);
                                }
                            }
                            else
                                str = "";
                        }
                        return "NYI: Object.ToString()";
                    default:
                        return "NYI: " + _type.IRType.ToString();
                }
            }
        }
    }
}
