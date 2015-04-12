using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witschi.Debug.Engine.AD7.Impl;
using Witschi.Debug.Engine.Host;

namespace Witschi.Debug.Engine.AD7
{
    class AD7DisassemblyStream : IDebugDisassemblyStream2
    {
        DebugHost _host;
        ulong _location = 0;
        AD7MemoryAddress _currentCtx;
        FunctionSourceInfo _fsi;
        DisassemblyData[] _datas;

        public AD7DisassemblyStream(DebugHost host, AD7MemoryAddress current, FunctionSourceInfo fsi)
        {
            _host = host;
            _currentCtx = current;
            _fsi = fsi;
        }

        int IDebugDisassemblyStream2.GetCodeContext(ulong uCodeLocationId, out IDebugCodeContext2 ppCodeContext)
        {
            ppCodeContext = new AD7MemoryAddress(_currentCtx.Program, uCodeLocationId);
            return VSConstants.S_OK;
        }

        int IDebugDisassemblyStream2.GetCodeLocationId(IDebugCodeContext2 pCodeContext, out ulong puCodeLocationId)
        {
            AD7MemoryAddress mem = (AD7MemoryAddress)pCodeContext;
            if (mem != null)
            {
                puCodeLocationId = mem.Address;
                return VSConstants.S_OK;
            }
            throw new NotImplementedException();
        }

        int IDebugDisassemblyStream2.GetCurrentLocation(out ulong puCodeLocationId)
        {
            puCodeLocationId = _currentCtx.Address;
            return VSConstants.S_OK;
        }

        int IDebugDisassemblyStream2.GetDocument(string bstrDocumentUrl, out IDebugDocument2 ppDocument)
        {
            throw new NotImplementedException();
        }

        int IDebugDisassemblyStream2.GetScope(enum_DISASSEMBLY_STREAM_SCOPE[] pdwScope)
        {
            throw new NotImplementedException();
        }

        int IDebugDisassemblyStream2.GetSize(out ulong pnSize)
        {
            List<DisassemblyData> datas = new List<DisassemblyData>();
            ulong offset = 0;
            ulong len = _fsi.DebugInfo.EndingAddress - _fsi.DebugInfo.StartingAddress;
            byte[] data = _host.ReadMemory(_fsi.DebugInfo.StartingAddress, len);
            BeaSharp.UnmanagedBuffer buffer = new BeaSharp.UnmanagedBuffer(data);

            while (offset < len)
            {
                Bea.Disasm disasm = new Bea.Disasm();
                disasm.EIP = new IntPtr(buffer.Ptr.ToInt64() + (long)offset);
                disasm.VirtualAddr = _fsi.DebugInfo.StartingAddress + offset;

                int result = Bea.BeaEngine.Disasm(disasm);

                if (result == (int)Bea.BeaConstants.SpecialInfo.UNKNOWN_OPCODE)
                    break;

                DisassemblyData dd = new DisassemblyData();
                dd.bstrAddress = "0x" + _currentCtx.Address.ToString("X8");
                dd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_ADDRESS;
                dd.bstrAddressOffset = "0x" + (_fsi.DebugInfo.StartingAddress + offset).ToString("X8");
                dd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_ADDRESSOFFSET;
                dd.bstrOpcode = disasm.Instruction.Mnemonic;
                dd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPCODE;
                dd.bstrCodeBytes = result.ToString();
                dd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_CODEBYTES;
                dd.uCodeLocationId = _fsi.DebugInfo.StartingAddress + offset;
                dd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_CODELOCATIONID;
                dd.dwByteOffset = (uint)offset;
                dd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_BYTEOFFSET;
                dd.bstrOperands = "";
                if ((disasm.Argument1.ArgSize > 0) && (disasm.Argument1.ArgMnemonic != ""))
                {
                    if ((disasm.Argument1.ArgType & Bea.BeaConstants.ArgumentType.MEMORY_TYPE) == Bea.BeaConstants.ArgumentType.MEMORY_TYPE)
                        dd.bstrOperands = "[" + disasm.Argument1.ArgMnemonic + "]";
                    else
                        dd.bstrOperands = disasm.Argument1.ArgMnemonic;
                    if ((disasm.Argument2.ArgSize > 0) && (disasm.Argument2.ArgMnemonic != ""))
                    {
                        if ((disasm.Argument2.ArgType & Bea.BeaConstants.ArgumentType.MEMORY_TYPE) == Bea.BeaConstants.ArgumentType.MEMORY_TYPE)
                            dd.bstrOperands += ", [" + disasm.Argument2.ArgMnemonic + "]";
                        else
                            dd.bstrOperands += ", " + disasm.Argument2.ArgMnemonic;
                    }
                }
                else if ((disasm.Argument2.ArgSize > 0) && (disasm.Argument2.ArgMnemonic != ""))
                {
                    if ((disasm.Argument2.ArgType & Bea.BeaConstants.ArgumentType.MEMORY_TYPE) == Bea.BeaConstants.ArgumentType.MEMORY_TYPE)
                        dd.bstrOperands = "[" + disasm.Argument2.ArgMnemonic + "]";
                    else
                        dd.bstrOperands = disasm.Argument2.ArgMnemonic;
                }
                dd.dwFields |= enum_DISASSEMBLY_STREAM_FIELDS.DSF_OPERANDS;
                datas.Add(dd);
                offset += (ulong)result;
            }

            _datas = datas.ToArray();

            pnSize = (ulong)_datas.Length;
            return VSConstants.S_OK;
        }

        int IDebugDisassemblyStream2.Read(uint dwInstructions, enum_DISASSEMBLY_STREAM_FIELDS dwFields, out uint pdwInstructionsRead, DisassemblyData[] prgDisassembly)
        {
            pdwInstructionsRead = 0;
            for (uint i = 0; i < dwInstructions; i++)
            {
                if ((i + _location) >= (ulong)_datas.Length)
                    return VSConstants.S_FALSE;
                prgDisassembly[i] = _datas[_location + i];
                pdwInstructionsRead++;
            }
            _location += pdwInstructionsRead;
            return VSConstants.S_OK;
        }

        int IDebugDisassemblyStream2.Seek(enum_SEEK_START dwSeekStart, IDebugCodeContext2 pCodeContext, ulong uCodeLocationId, long iInstructions)
        {
            if (dwSeekStart == enum_SEEK_START.SEEK_START_BEGIN)
            {
                _location = 0 + uCodeLocationId;
                return VSConstants.S_OK;
            }
            else
                throw new NotImplementedException();
        }
    }
}
