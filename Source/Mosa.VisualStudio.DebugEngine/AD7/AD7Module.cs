using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witschi.Debug.Engine.AD7
{
    class AD7Module : IDebugModule3
    {
        string _name = "DUMMY NAME";
        AD7Program _program;

        public AD7Module(AD7Program program)
        {
            SymbolsLoaded = false;
            SymbolPath = "";
            _program = program;
        }

        public bool SymbolsLoaded
        {
            get;
            set;
        }

        public string SymbolPath
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name; }
        }

        public AD7Program Program
        {
            get { return _program; }
        }

        int IDebugModule2.GetInfo(enum_MODULE_INFO_FIELDS dwFields, MODULE_INFO[] pinfo)
        {
            MODULE_INFO info = new MODULE_INFO();

            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_NAME) == enum_MODULE_INFO_FIELDS.MIF_NAME)
            {
                info.m_bstrName = this.Name;
                info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_NAME;
            }
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_URL) == enum_MODULE_INFO_FIELDS.MIF_URL)
            {
                info.m_bstrUrl = this.Name;
                info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_URL;
            }
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_LOADADDRESS) == enum_MODULE_INFO_FIELDS.MIF_LOADADDRESS)
            {
                //info.m_addrLoadAddress = (ulong)this.BaseAddress;
                //info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_LOADADDRESS;
            }
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_PREFFEREDADDRESS) != 0)
            {
                // A debugger that actually supports showing the preferred base should crack the PE header and get 
                // that field. This debugger does not do that, so assume the module loaded where it was suppose to.                   
                //info.m_addrPreferredLoadAddress = (ulong)this.DebuggedModule.BaseAddress;
                //info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_PREFFEREDADDRESS; ;
            }
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_SIZE) != 0)
            {
               // info.m_dwSize = this.DebuggedModule.Size;
               // info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_SIZE;
            }
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_LOADORDER) != 0)
            {
                //info.m_dwLoadOrder = this.DebuggedModule.GetLoadOrder();
                //info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_LOADORDER;
            }
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_URLSYMBOLLOCATION) != 0)
            {
               // if (this.DebuggedModule.SymbolsLoaded)
                //{
               //     info.m_bstrUrlSymbolLocation = this.DebuggedModule.SymbolPath;
               //     info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_URLSYMBOLLOCATION;
               // }
            }
            if ((dwFields & enum_MODULE_INFO_FIELDS.MIF_FLAGS) != 0)
            {
                info.m_dwModuleFlags = 0;
               // if (this.DebuggedModule.SymbolsLoaded)
                //{
                //    info.m_dwModuleFlags |= (enum_MODULE_FLAGS.MODULE_FLAG_SYMBOLS);
               // }
                info.dwValidFields |= enum_MODULE_INFO_FIELDS.MIF_FLAGS;
            }

            pinfo[0] = info;

            return VSConstants.S_OK;
        }

        int IDebugModule2.ReloadSymbols_Deprecated(string pszUrlToSymbols, out string pbstrDebugMessage)
        {
            throw new NotImplementedException();
        }

        int IDebugModule3.GetInfo(enum_MODULE_INFO_FIELDS dwFields, MODULE_INFO[] pinfo)
        {
            return ((IDebugModule2)this).GetInfo(dwFields, pinfo);
        }

        int IDebugModule3.GetSymbolInfo(enum_SYMBOL_SEARCH_INFO_FIELDS dwFields, MODULE_SYMBOL_SEARCH_INFO[] pinfo)
        {
            pinfo[0] = new MODULE_SYMBOL_SEARCH_INFO();
            pinfo[0].dwValidFields = 1; // SSIF_VERBOSE_SEARCH_INFO;

            if (this.SymbolsLoaded)
                pinfo[0].bstrVerboseSearchInfo = "Symbols Loaded - " + this.SymbolPath;
            else
                pinfo[0].bstrVerboseSearchInfo = "Symbols not loaded";

            return VSConstants.S_OK;
        }

        int IDebugModule3.IsUserCode(out int pfUser)
        {
            pfUser = 1;
            return VSConstants.S_OK;
        }

        int IDebugModule3.LoadSymbols()
        {
            throw new NotImplementedException();
        }

        int IDebugModule3.ReloadSymbols_Deprecated(string pszUrlToSymbols, out string pbstrDebugMessage)
        {
            throw new NotImplementedException();
        }

        int IDebugModule3.SetJustMyCodeState(int fIsUserCode)
        {
            throw new NotImplementedException();
        }
    }
}
