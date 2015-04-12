using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Witschi.Debug.Engine.AD7
{
    class AD7DocumentContext : IDebugDocumentContext2
    {
        string m_fileName;
        TEXT_POSITION m_begPos;
        TEXT_POSITION m_endPos;
        AD7MemoryAddress m_codeContext;

        public AD7DocumentContext(string fileName, TEXT_POSITION begPos, TEXT_POSITION endPos, AD7MemoryAddress codeContext)
        {
            m_fileName = fileName;
            m_begPos = begPos;
            m_endPos = endPos;
            m_codeContext = codeContext;
        }


        #region IDebugDocumentContext2 Members

        int IDebugDocumentContext2.Compare(enum_DOCCONTEXT_COMPARE Compare, IDebugDocumentContext2[] rgpDocContextSet, uint dwDocContextSetLen, out uint pdwDocContext)
        {
            dwDocContextSetLen = 0;
            pdwDocContext = 0;

            return VSConstants.E_NOTIMPL;
        }

        int IDebugDocumentContext2.EnumCodeContexts(out IEnumDebugCodeContexts2 ppEnumCodeCxts)
        {
            ppEnumCodeCxts = null;
            AD7MemoryAddress[] codeContexts = new AD7MemoryAddress[1];
            codeContexts[0] = m_codeContext;
            ppEnumCodeCxts = new AD7CodeContextEnum(codeContexts);
            return VSConstants.S_OK;
        }

        int IDebugDocumentContext2.GetDocument(out IDebugDocument2 ppDocument)
        {
            ppDocument = null;
            return VSConstants.E_FAIL;
        }

        int IDebugDocumentContext2.GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            pbstrLanguage = "WC#";
            pguidLanguage = new Guid("e17f3766-36a2-4022-9b88-7771e1aba163");
            return VSConstants.S_OK;
        }

        int IDebugDocumentContext2.GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName)
        {
            pbstrFileName = m_fileName;
            return VSConstants.S_OK;
        }

        int IDebugDocumentContext2.GetSourceRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            throw new NotImplementedException("This method is not implemented");
        }

        int IDebugDocumentContext2.GetStatementRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            pBegPosition[0].dwColumn = m_begPos.dwColumn;
            pBegPosition[0].dwLine = m_begPos.dwLine;

            pEndPosition[0].dwColumn = m_endPos.dwColumn;
            pEndPosition[0].dwLine = m_endPos.dwLine;

            return VSConstants.S_OK;
        }

        int IDebugDocumentContext2.Seek(int nCount, out IDebugDocumentContext2 ppDocContext)
        {
            ppDocContext = null;
            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }
}
