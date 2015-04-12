using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witschi.Debug.Metadata;

namespace Witschi.Debug.Engine.AD7.Impl
{
    class SymbolEngine
    {
        List<DebugDatabase> _databases = new List<DebugDatabase>();
        Compiler.TypeSys.CompilerTypeSystem _typeSystem = new Compiler.TypeSys.CompilerTypeSystem();

        public bool LoadSymbolsForModule(string modulePath, out string symbolPath)
        {
            symbolPath = "";
            string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(modulePath), System.IO.Path.GetFileNameWithoutExtension(modulePath) + ".pdb");
            if (System.IO.File.Exists(path))
            {
                try
                {
                    System.Xml.Serialization.XmlSerializer serial = new System.Xml.Serialization.XmlSerializer(typeof(DebugDatabase));
                    using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open))
                    {
                        DebugDatabase db = (DebugDatabase)serial.Deserialize(fs);
                        _databases.Add(db);
                        symbolPath = path;
                        foreach (DebugAssembly asm in db.Assemblies)
                        {
                            string fileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(modulePath), asm.PrimaryModule.Name + ".il");
                            if (System.IO.File.Exists(fileName))
                            {
                                using (System.IO.FileStream fs2 = new System.IO.FileStream(fileName, System.IO.FileMode.Open))
                                {
                                    _typeSystem.LoadAssembly(fileName, Witschi.Compiler.FileFormats.PE.PEParser.Parse(fs2));
                                }
                            }
                        }
                        return true;
                    }
                }
                catch
                {
                }
            }
            return false;
        }
        
        private DebugMethod FindAddress(UInt64 address)
        {
            foreach (DebugDatabase db in _databases)
            {
                foreach (DebugAssembly da in db.Assemblies)
                {
                    foreach (DebugType type in da.PrimaryModule.Types)
                    {
                        foreach (DebugMethod meth in type.Methods)
                        {
                            if ((meth.StartingAddress <= address) && (meth.EndingAddress >= address))
                                return meth;
                        }
                    }
                }
            }
            return null;
        }

        public ulong[] GetAddressesForSourceLocation(string documentName, uint line, uint col)
        {
            List<ulong> addrs = new List<ulong>();
            foreach (DebugDatabase db in _databases)
            {
                foreach (DebugAssembly da in db.Assemblies)
                {
                    foreach (DebugType dt in da.PrimaryModule.Types)
                    {
                        foreach (DebugMethod dm in dt.Methods)
                        {
                            foreach (DebugSource ds in dm.Sources)
                            {
                                if (ds.File.ToUpperInvariant() == documentName.ToUpperInvariant())
                                {
                                    foreach(DebugLine dl in ds.Lines)
                                    {
                                        if ((dl.LineBegin <= line) && (dl.LineEnd >= line) && (dl.ColBegin <= col) && (dl.ColEnd >= col))
                                        {
                                            addrs.Add(dm.StartingAddress + dl.Offset);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return addrs.ToArray();
        }

        public FunctionSourceInfo GetInfoForAddress(UInt64 address)
        {
            DebugMethod meth = FindAddress(address);
            if (meth != null)
            {
                ulong currentOffset = address - meth.StartingAddress;
                DebugSource beforeSource = null;
                DebugSource afterSource = null;
                DebugLine bestLine = null;
                DebugLine bestAfter = null;
                foreach (DebugSource src in meth.Sources)
                {
                    foreach (DebugLine line in src.Lines)
                    {
                        if (line.Offset <= currentOffset)
                        {
                            if ((bestLine == null) || (line.Offset > bestLine.Offset))
                            {
                                beforeSource = src;
                                bestLine = line;
                            }
                        }
                        else if (line.LineBegin != 0xFEEFEE)
                        {
                            if ((bestAfter == null) || (line.Offset < bestAfter.Offset))
                            {
                                afterSource = src;
                                bestAfter = line;
                            }
                        }
                    }
                }
                if (bestLine != null)
                {
                    if (bestLine.LineBegin == 0xFEEFEE)
                    {
                        bestLine = bestAfter;
                        beforeSource = afterSource;
                    }
                }
                return new FunctionSourceInfo(meth, beforeSource, bestLine);
            }
            else
            {
                return null;
            }
        }

        public LocalInfo[] CreateLocals(FunctionSourceInfo fsi, AD7StackFrame frame)
        {
            List<LocalInfo> lst = new List<LocalInfo>();
            foreach (DebugLocal loc in fsi.DebugInfo.Locals)
            {
                lst.Add(new LocalInfo(loc.Name, loc.Offset, _typeSystem.GetType(loc.Type)));
            }
            return lst.ToArray();
        }
    }
}
