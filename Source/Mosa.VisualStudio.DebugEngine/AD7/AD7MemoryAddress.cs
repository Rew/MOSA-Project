using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witschi.Debug.Engine.AD7
{
    class AD7MemoryAddress : IDebugCodeContext2
    {
        AD7Program _program;
        ulong _address;
        IDebugDocumentContext2 _documentContext;

        public AD7Program Program
        {
            get { return _program; }
        }

        public ulong Address
        {
            get { return _address; }
        }

        public AD7MemoryAddress(AD7Program program, ulong address)
        {
            _program = program;
            _address = address;
        }

        public void SetDocumentContext(IDebugDocumentContext2 ctx)
        {
            _documentContext = ctx;
        }

        #region IDebugCodeContext

        int IDebugCodeContext2.Add(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            ppMemCxt = new AD7MemoryAddress(_program, _address + dwCount);
            return VSConstants.S_OK;
        }

        int IDebugCodeContext2.Compare(enum_CONTEXT_COMPARE Compare, IDebugMemoryContext2[] rgpMemoryContextSet, uint dwMemoryContextSetLen, out uint pdwMemoryContext)
        {
            pdwMemoryContext = uint.MaxValue;

            for (uint c = 0; c < dwMemoryContextSetLen; c++)
            {
                AD7MemoryAddress compareTo = rgpMemoryContextSet[c] as AD7MemoryAddress;
                if (compareTo == null)
                {
                    continue;
                }

                if (!AD7Engine.ReferenceEquals(this._program, compareTo._program))
                {
                    continue;
                }

                bool result;

                switch (Compare)
                {
                    case enum_CONTEXT_COMPARE.CONTEXT_EQUAL:
                        result = (this._address == compareTo._address);
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN:
                        result = (this._address < compareTo._address);
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN:
                        result = (this._address > compareTo._address);
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN_OR_EQUAL:
                        result = (this._address <= compareTo._address);
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN_OR_EQUAL:
                        result = (this._address >= compareTo._address);
                        break;

                    // The sample debug engine doesn't understand scopes or functions
                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_SCOPE:
                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_FUNCTION:
                        result = (this._address == compareTo._address);
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_MODULE:
                        result = (this._address == compareTo._address);
                        if (result == false)
                        {
                            AD7Module module = _program.Process.ResolveAddress(_address);

                            if (module != null)
                            {
                               // result = (compareTo._address >= module.BaseAddress) && (compareTo._address < module.BaseAddress + module.Size);
                                result = true;
                            }
                        }
                        break;

                    case enum_CONTEXT_COMPARE.CONTEXT_SAME_PROCESS:
                        result = true;
                        break;

                    default:
                        // A new comparison was invented that we don't support
                        return VSConstants.E_NOTIMPL;
                }

                if (result)
                {
                    pdwMemoryContext = c;
                    return VSConstants.S_OK;
                }
            }

            return VSConstants.S_FALSE;
        }

        int IDebugCodeContext2.GetDocumentContext(out IDebugDocumentContext2 ppSrcCxt)
        {
            ppSrcCxt = _documentContext;
            return VSConstants.S_FALSE;
        }

        int IDebugCodeContext2.GetInfo(enum_CONTEXT_INFO_FIELDS dwFields, CONTEXT_INFO[] pinfo)
        {
            pinfo[0].dwFields = 0;

            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS) != 0)
            {
                pinfo[0].bstrAddress = "0x" + _address.ToString("X");
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS;
            }
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSABSOLUTE) != 0)
            {
                pinfo[0].bstrAddressAbsolute = "0x" + _address.ToString("X");
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSABSOLUTE;
            }
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSOFFSET) != 0)
            {
            }
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION) != 0)
            {
                pinfo[0].bstrFunction = "BeBusy";
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_FUNCTION;
            }
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_FUNCTIONOFFSET) != 0)
            {
                pinfo[0].posFunctionOffset = new TEXT_POSITION();
                pinfo[0].posFunctionOffset.dwLine = 2;
                pinfo[0].posFunctionOffset.dwColumn = 1;
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_FUNCTIONOFFSET;
            }
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL) != 0)
            {
                pinfo[0].bstrModuleUrl = "DUMMY NAME";
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL;
            }

            return VSConstants.S_OK;
        }

        int IDebugCodeContext2.GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            if (_documentContext != null)
            {
                _documentContext.GetLanguageInfo(ref pbstrLanguage, ref pguidLanguage);
                return VSConstants.S_OK;
            }
            else
            {
                return VSConstants.S_FALSE;
            }
        }

        int IDebugCodeContext2.GetName(out string pbstrName)
        {
            throw new NotImplementedException();
        }

        int IDebugCodeContext2.Subtract(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            ppMemCxt = new AD7MemoryAddress(_program, _address - dwCount);
            return VSConstants.S_OK;
        }

        #endregion

        #region IDebugMemoryContext2

        int IDebugMemoryContext2.Add(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            ppMemCxt = new AD7MemoryAddress(Program, _address + dwCount);
            return VSConstants.S_OK;
        }

        int IDebugMemoryContext2.Compare(enum_CONTEXT_COMPARE Compare, IDebugMemoryContext2[] rgpMemoryContextSet, uint dwMemoryContextSetLen, out uint pdwMemoryContext)
        {
            pdwMemoryContext = uint.MaxValue;

            try
            {
                for (uint c = 0; c < dwMemoryContextSetLen; c++)
                {
                    AD7MemoryAddress compareTo = rgpMemoryContextSet[c] as AD7MemoryAddress;
                    if (compareTo == null)
                    {
                        continue;
                    }

                    if (!AD7Engine.ReferenceEquals(this.Program.Engine, compareTo.Program.Engine))
                    {
                        continue;
                    }

                    bool result;

                    switch (Compare)
                    {
                        case enum_CONTEXT_COMPARE.CONTEXT_EQUAL:
                            result = (this.Address == compareTo.Address);
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN:
                            result = (this.Address < compareTo.Address);
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN:
                            result = (this.Address > compareTo.Address);
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_LESS_THAN_OR_EQUAL:
                            result = (this.Address <= compareTo.Address);
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_GREATER_THAN_OR_EQUAL:
                            result = (this.Address >= compareTo.Address);
                            break;

                        // The sample debug engine doesn't understand scopes or functions
                        case enum_CONTEXT_COMPARE.CONTEXT_SAME_SCOPE:
                        case enum_CONTEXT_COMPARE.CONTEXT_SAME_FUNCTION:
                            result = (this.Address == compareTo.Address);
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_SAME_MODULE:
                            result = (this.Address == compareTo.Address);
                            if (result == false)
                            {
                                //DebuggedModule module = m_engine.DebuggedProcess.ResolveAddress(m_address);

                                //if (module != null)
                                //{
                                //    result = (compareTo.m_address >= module.BaseAddress) &&
                                //        (compareTo.m_address < module.BaseAddress + module.Size);
                                //}
                            }
                            break;

                        case enum_CONTEXT_COMPARE.CONTEXT_SAME_PROCESS:
                            result = true;
                            break;

                        default:
                            // A new comparison was invented that we don't support
                            return VSConstants.E_NOTIMPL;
                    }

                    if (result)
                    {
                        pdwMemoryContext = c;
                        return VSConstants.S_OK;
                    }
                }

                return VSConstants.S_FALSE;
            }
            //catch (ComponentException e)
            //{
            //    return e.HResult;
            //}
            catch (Exception e)
            {
                return Witschi.Debug.Engine.AD7.Impl.EngineUtils.UnexpectedException(e);
            }
        }

        int IDebugMemoryContext2.GetInfo(enum_CONTEXT_INFO_FIELDS dwFields, CONTEXT_INFO[] pinfo)
        {
            CONTEXT_INFO ci = new CONTEXT_INFO();

            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSOFFSET) != 0)
            {
                ci.bstrAddressOffset = _address.ToString("X");
                ci.dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSOFFSET;
            }
            /*if((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS) != 0)
            {
                ci.bstrAddress = _address.ToString("X");
                ci.dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS;
            }
            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSABSOLUTE) != 0)
            {
                ci.bstrAddressAbsolute = _address.ToString("X");
                ci.dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_ADDRESSABSOLUTE;
            }*/
            pinfo[0] = ci;
            return VSConstants.S_OK;
        }

        int IDebugMemoryContext2.GetName(out string pbstrName)
        {
            throw new NotImplementedException();
        }

        int IDebugMemoryContext2.Subtract(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            ppMemCxt = new AD7MemoryAddress(Program, _address - dwCount);
            return VSConstants.S_OK;
        }

        #endregion
    }
}
