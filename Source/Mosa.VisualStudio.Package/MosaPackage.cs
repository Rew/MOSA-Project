using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Project;

namespace Mosa.VisualStudio.Package
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(Guids.ProjectPackage)]
	[DefaultRegistryRoot(@"HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\12.0exp_config")]
	[ProvideProjectFactory(typeof(Project.MosaProjectFactory), "Mosa Project", "Mosa Project Files (*.wcproj);*.wcproj", "wcproj", "wcproj", @"..\..\Templates\Projects\WitchCraft", LanguageVsTemplate = "Mosa", NewProjectRequireNewFolderVsTemplate = false)]
    public sealed class MosaPackage : ProjectPackage
    {
        private System.Diagnostics.TraceListener _listener;

        #region Overridden Implementation
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            //_listener = new System.Diagnostics.TextWriterTraceListener(@"C:\test\trace.txt");
            //System.Diagnostics.Trace.Listeners.Add(_listener);

            this.RegisterProjectFactory(new Project.MosaProjectFactory(this));
        }

        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Trace.Flush();
            _listener.Dispose();

            base.Dispose(disposing);
        }

        public override string ProductUserContext
        {
            get { return "MosaProj"; }
        }

        #endregion
    }
}
