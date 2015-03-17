using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mosa.VisualStudio.Package.Project
{
    class MosaProjectConfig: ProjectConfig
    {
        public MosaProjectConfig(ProjectNode project, string configuration)
            : base(project, configuration)
        {
        }

        public override int DebugLaunch(uint grfLaunch)
        {
            CCITracing.TraceCall();

            try
            {
                VsDebugTargetInfo info = new VsDebugTargetInfo();
                info.cbSize = (uint)Marshal.SizeOf(info);
                info.dlo = Microsoft.VisualStudio.Shell.Interop.DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;

                // On first call, reset the cache, following calls will use the cached values
                string property = GetConfigurationProperty("StartProgram", true);
                if (string.IsNullOrEmpty(property))
                {
                    info.bstrExe = ProjectMgr.GetOutputAssembly(this.ConfigName);
                }
                else
                {
                    info.bstrExe = property;
                }

                property = GetConfigurationProperty("WorkingDirectory", false);
                if (string.IsNullOrEmpty(property))
                {
                    info.bstrCurDir = Path.GetDirectoryName(info.bstrExe);
                }
                else
                {
                    info.bstrCurDir = property;
                }

                property = GetConfigurationProperty("CmdArgs", false);
                if (!string.IsNullOrEmpty(property))
                {
                    info.bstrArg = property;
                }

                property = GetConfigurationProperty("RemoteDebugMachine", false);
                if (property != null && property.Length > 0)
                {
                    info.bstrRemoteMachine = property;
                }

                info.fSendStdoutToOutputWindow = 0;

                info.clsidCustom = new Guid(Guids.DebugEngine);
                info.clsidPortSupplier = new Guid("708C1ECA-FF48-11D2-904F-00C04FA302A1");

                info.grfLaunch = grfLaunch;
                VsShellUtilities.LaunchDebugger(ProjectMgr.Site, info);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Exception : " + e.Message);

                return Marshal.GetHRForException(e);
            }

            return VSConstants.S_OK;
        }
    }
}